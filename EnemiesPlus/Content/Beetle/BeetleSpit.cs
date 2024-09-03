using EnemiesPlus.Prediction;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EnemiesPlus.Content.Beetle
{
    public class BeetleSpit : BaseState
    {
        public static float baseDuration = 1f;
        public static float damageCoefficient;
        public static string attackSoundString = "Play_beetle_worker_attack";

        private bool hasFired;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            this.StartAimMode();
            Util.PlayAttackSpeedSound(attackSoundString, this.gameObject, this.attackSpeedStat);
            this.PlayCrossfade("Body", "EmoteSurprise", "Headbutt.playbackRate", this.duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!this.hasFired && this.fixedAge >= this.duration * 0.8f && this.isAuthority)
            {
                this.hasFired = true;
                var aimRay = PredictionUtils.PredictAimray(this.GetAimRay(), this.characterBody, BeetleChanges.beetleSpit);
                ProjectileManager.instance.FireProjectile(BeetleChanges.beetleSpit, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat, 0.0f, base.RollCrit());
            }

            if (this.isAuthority && this.fixedAge >= this.duration)
                this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
