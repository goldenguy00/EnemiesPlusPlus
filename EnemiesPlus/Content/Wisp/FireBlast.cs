using EntityStates;
using EntityStates.Wisp1Monster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EnemiesPlus.Content.Wisp
{
    public class FireBlast : BaseSkillState
    {
        private float duration;
        public static GameObject projectilePrefab;

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = FireEmbers.baseDuration;
            var aimRay = this.GetAimRay();

            Util.PlayAttackSpeedSound(FireEmbers.attackString, this.gameObject, this.attackSpeedStat);
            this.StartAimMode(aimRay, 2f, false);
            this.PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", this.duration);

            Fire();
        }

        public void Fire()
        {
            if (this.isAuthority)
            {
                bool isCrit = this.RollCrit();
                var aimRay = this.GetAimRay();
                for (int i = 0; i < FireEmbers.bulletCount; i++)
                {
                    aimRay.direction = Util.ApplySpread(aimRay.direction, FireEmbers.minSpread, FireEmbers.maxSpread, 0f, 0f, 0f, 0f);
                    ProjectileManager.instance.FireProjectile(
                        projectilePrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        this.gameObject,
                        this.damageStat * FireEmbers.damageCoefficient,
                        FireEmbers.force / 3f,
                        isCrit);

                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.isAuthority && this.fixedAge >= duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Pain;
    }
}
