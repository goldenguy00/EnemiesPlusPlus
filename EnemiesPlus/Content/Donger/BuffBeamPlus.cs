using RoR2;
using System.Linq;
using UnityEngine;
using EntityStates.Bell.BellWeapon;
using EntityStates;
using UnityEngine.Networking;
using Inferno.Stat_AI;
using UnityEngine.AddressableAssets;

namespace EnemiesPlus.Content.Donger
{
    public class BuffBeamPlus : BaseState
    {
        public static float duration = 12f;

        public static GameObject buffBeamPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBuffBeam.prefab").WaitForCompletion();
        public static AnimationCurve beamWidthCurve;

        public static string playBeamSoundString = "Play_emergency_drone_heal_loop";
        public static string stopBeamSoundString = "Stop_emergency_drone_heal_loop";

        public HurtBox targetHurtbox;

        public GameObject buffBeamInstance;
        public BezierCurveLine healBeamCurve;
        public Transform muzzleTransform;
        public Transform beamTipTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound(playBeamSoundString, base.gameObject);
            var aimRay = GetAimRay();
            var buddySearch = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.none,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = 75f,
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                maxAngleFilter = 180f,
                filterByLoS = true
            };
            buddySearch.teamMaskFilter.AddTeam(teamComponent.teamIndex);
            buddySearch.RefreshCandidates();
            buddySearch.FilterOutGameObject(base.characterBody.gameObject);

            foreach (var hurtBox in buddySearch.GetResults())
            {
                var body = hurtBox.healthComponent ? hurtBox.healthComponent.body : null;
                if (body && body.hullClassification != HullClassification.BeetleQueen && body.bodyIndex != base.characterBody.bodyIndex)
                {
                    targetHurtbox = hurtBox;
                }
            }

            if (targetHurtbox)
            {
                this.StartAimMode(BuffBeam.duration);
                targetHurtbox.healthComponent.body.AddBuff(RoR2Content.Buffs.ElephantArmorBoost.buffIndex);
            }
            else
            {
                skillLocator.secondary.AddOneStock();
                this.outer.SetNextStateToMain();

                return;
            }

            string childName = "Muzzle";
            var childLoc = GetModelChildLocator();
            if (childLoc)
            {
                muzzleTransform = childLoc.FindChild(childName);
                buffBeamInstance = Object.Instantiate(buffBeamPrefab);
                var beamChildLoc = buffBeamInstance.GetComponent<ChildLocator>();
                if (beamChildLoc)
                {
                    beamTipTransform = beamChildLoc.FindChild("BeamTip");
                    healBeamCurve = buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!base.isAuthority)
                return;

            if (base.fixedAge >= duration || !targetHurtbox || !targetHurtbox.healthComponent || !targetHurtbox.healthComponent.alive)
            {
                outer.SetNextStateToMain();
            }
        }

        public void UpdateHealBeamVisuals()
        {
            if (healBeamCurve)
            {
                float widthMultiplier = beamWidthCurve.Evaluate(base.age / duration);
                healBeamCurve.lineRenderer.widthMultiplier = widthMultiplier;
                healBeamCurve.v0 = muzzleTransform.forward * 3f;
                healBeamCurve.transform.position = muzzleTransform.position;
                if (targetHurtbox)
                {
                    beamTipTransform.position = targetHurtbox.transform.position;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateHealBeamVisuals();
        }

        public override void OnExit()
        {
            Util.PlaySound(stopBeamSoundString, base.gameObject);

            if (buffBeamInstance)
                Destroy(buffBeamInstance);

            if (NetworkServer.active && targetHurtbox && targetHurtbox.healthComponent && targetHurtbox.healthComponent.body)
            {
                targetHurtbox.healthComponent.body.RemoveBuff(RoR2Content.Buffs.ElephantArmorBoost.buffIndex);
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            HurtBoxReference.FromHurtBox(targetHurtbox).Write(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            HurtBoxReference hurtBoxReference = default;
            hurtBoxReference.Read(reader);
            targetHurtbox = hurtBoxReference.ResolveGameObject()?.GetComponent<HurtBox>();
        }
    }
}
