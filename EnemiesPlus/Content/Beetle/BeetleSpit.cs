using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EnemiesPlus.Content.Beetle
{
    public class BeetleSpit : BaseSkillState
    {
        public static float baseDuration = 1f;
        public static string attackSoundString = "Play_beetle_worker_attack";
        public static GameObject projectilePrefab;

        private bool hasFired;
        private float duration;
        private float fireTime;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = duration * 0.9f;
            StartAimMode(fireTime);
            GetModelAnimator();

            if (this.characterMotor)
                this.characterMotor.walkSpeedPenaltyCoefficient = 0f;

            Util.PlayAttackSpeedSound(attackSoundString, this.gameObject, this.attackSpeedStat);
            this.PlayCrossfade("Body", "EmoteSurprise", "Headbutt.playbackRate", this.duration, 0.1f);
        }

        public void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;
                var aimRay = GetAimRay();
                if (base.isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat, 0.0f, base.RollCrit());
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= fireTime)
            {
                Fire();
            }

            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.characterMotor)
                this.characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
