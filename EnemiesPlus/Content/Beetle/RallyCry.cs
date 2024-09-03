using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using EntityStates.BeetleGuardMonster;
using System.Linq;

namespace EnemiesPlus.Content.Beetle
{
    public class RallyCry : BaseState
    {
        public float baseDuration = 3.5f;
        public float buffDuration = 6f;
        private float delay;
        private Animator modelAnimator;
        private float duration;
        private bool hasCastBuff;
        private BullseyeSearch bullseyeSearch;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.delay = this.duration * 0.5f;

            var mask = TeamMask.none;
            if (this.teamComponent)
                mask.AddTeam(this.teamComponent.teamIndex);

            this.bullseyeSearch = new BullseyeSearch
            {
                teamMaskFilter = mask,
                filterByLoS = false,
                maxDistanceFilter = 16f,
                maxAngleFilter = 360f,
                sortMode = BullseyeSearch.SortMode.Distance,
                filterByDistinctEntity = true,
                viewer = this.characterBody
            };


            this.modelAnimator = this.GetModelAnimator();
            if (!this.modelAnimator)
                return;

            Util.PlayAttackSpeedSound("Play_beetle_guard_death", this.gameObject, 0.5f);
            this.PlayCrossfade("Body", nameof(DefenseUp), "DefenseUp.playbackRate", this.duration, 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.modelAnimator && this.fixedAge > this.delay && !this.hasCastBuff)
            {
                var ed = new EffectData()
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity
                };
                var modelTransform = this.modelLocator ? this.modelLocator.modelTransform : null;
                if (modelTransform && modelTransform.TryGetComponent<ChildLocator>(out var loc))
                {
                    int childIndex = loc.FindChildIndex("Head");
                    if (childIndex != -1)
                    {
                        ed.SetChildLocatorTransformReference(base.gameObject, childIndex);
                    }
                }
                EffectManager.SpawnEffect(DefenseUp.defenseUpPrefab, ed, true);

                this.hasCastBuff = true;
                if (NetworkServer.active)
                {
                    this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, this.buffDuration);

                    var aimRay = this.GetAimRay();
                    this.bullseyeSearch.searchOrigin = aimRay.origin;
                    this.bullseyeSearch.searchDirection = aimRay.direction;
                    this.bullseyeSearch.RefreshCandidates();
                    this.bullseyeSearch.FilterOutGameObject(this.gameObject);

                    foreach (var nearbyAlly in this.bullseyeSearch.GetResults().Where(a => a.healthComponent && a.healthComponent.body && !a.healthComponent.body.HasBuff(RoR2Content.Buffs.TeamWarCry)))
                    {
                        nearbyAlly.healthComponent.body.AddTimedBuff(RoR2Content.Buffs.TeamWarCry, this.buffDuration);
                    }
                }
            }

            if (this.fixedAge < this.duration || !this.isAuthority)
                return;

            this.outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.characterBody && this.characterBody.HasBuff(RoR2Content.Buffs.ArmorBoost))
                this.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Pain;
    }
}