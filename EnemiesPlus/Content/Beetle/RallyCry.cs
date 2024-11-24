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
        public static float baseDuration = 3.5f;
        public static float buffDuration = 8f;
        private float delay;
        private float duration;
        private bool hasCastBuff;
        private BullseyeSearch bullseyeSearch;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            this.delay = this.duration * 0.5f;

            this.bullseyeSearch = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.none,
                filterByLoS = false,
                maxDistanceFilter = 24f,
                sortMode = BullseyeSearch.SortMode.Distance,
                filterByDistinctEntity = true,
                viewer = this.characterBody,
                searchOrigin = this.transform.position,
                searchDirection = this.transform.forward
            };
            if (base.teamComponent)
                bullseyeSearch.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);

            this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, buffDuration);
            var ed = new EffectData()
            {
                origin = this.transform.position,
                rotation = Quaternion.identity
            };
            var loc = this.GetModelChildLocator();
            if (loc)
            {
                int childIndex = loc.FindChildIndex("Head");
                if (childIndex != -1)
                {
                    ed.SetChildLocatorTransformReference(base.gameObject, childIndex);
                }
            }
            EffectManager.SpawnEffect(DefenseUp.defenseUpPrefab, ed, true);

            Util.PlayAttackSpeedSound("Play_beetle_guard_death", this.gameObject, 0.5f);
            this.PlayCrossfade("Body", nameof(DefenseUp), "DefenseUp.playbackRate", this.duration, 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && this.fixedAge > this.delay && !this.hasCastBuff)
            {
                this.hasCastBuff = true;
                this.bullseyeSearch.RefreshCandidates();
                this.bullseyeSearch.FilterOutGameObject(this.gameObject);

                foreach (var nearbyAlly in this.bullseyeSearch.GetResults())
                {
                    if (nearbyAlly.healthComponent.body)
                        nearbyAlly.healthComponent.body.AddTimedBuff(RoR2Content.Buffs.TeamWarCry, buffDuration);
                }
            }

            if (this.fixedAge >= this.duration && this.isAuthority)
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