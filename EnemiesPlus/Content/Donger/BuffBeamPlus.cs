using RoR2;
using System.Linq;
using UnityEngine;
using EntityStates.Bell.BellWeapon;

namespace EnemiesPlus.Content.Donger
{
    public class BuffBeamPlus : BuffBeam
    {
        public override void OnEnter()
        {
            if (base.characterBody)
            {
                attackSpeedStat = base.characterBody.attackSpeed;
                damageStat = base.characterBody.damage;
                critStat = base.characterBody.crit;
                moveSpeedStat = base.characterBody.moveSpeed;
            }

            Util.PlaySound(playBeamSoundString, base.gameObject);
            var aimRay = GetAimRay();
            var bs = new BullseyeSearch
            {
                filterByLoS = false,
                maxDistanceFilter = 50f,
                maxAngleFilter = 180f,
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                sortMode = BullseyeSearch.SortMode.Angle,
                teamMaskFilter = TeamMask.none
            };
            if (base.teamComponent)
                bs.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);

            bs.RefreshCandidates();
            bs.FilterOutGameObject(base.gameObject);
            target = bs.GetResults().Where(x => x.healthComponent.body && x.healthComponent.body.bodyIndex != this.characterBody.bodyIndex).FirstOrDefault();

            if (target)
            {
                this.StartAimMode(BuffBeam.duration);
                Debug.LogFormat("Buffing target {0}", target);
                targetBody = target.healthComponent.body;
                targetBody.AddBuff(RoR2Content.Buffs.ElephantArmorBoost.buffIndex);
            }

            string childName = "Muzzle";
            var modelTransform = GetModelTransform();
            if (!modelTransform)
            {
                return;
            }

            var component = modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                muzzleTransform = component.FindChild(childName);
                buffBeamInstance = Object.Instantiate(buffBeamPrefab);
                var component2 = buffBeamInstance.GetComponent<ChildLocator>();
                if (component2)
                {
                    beamTipTransform = component2.FindChild("BeamTip");
                }

                healBeamCurve = buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (targetBody)
            {
                targetBody.RemoveBuff(RoR2Content.Buffs.ElephantArmorBoost.buffIndex);
            }
        }
    }
}
