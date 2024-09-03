using EnemiesPlus.Prediction;
using EntityStates;
using EntityStates.ImpMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EnemiesPlus.Content.Imp
{
    public class ImpVoidSpike : BaseState
    {
        private Animator modelAnimator;
        private float duration;
        private int slashCount;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = DoubleSlash.baseDuration / this.attackSpeedStat;
            this.modelAnimator = this.GetModelAnimator();
            this.characterMotor.walkSpeedPenaltyCoefficient = DoubleSlash.walkSpeedPenaltyCoefficient;
            Util.PlayAttackSpeedSound(DoubleSlash.enterSoundString, this.gameObject, this.attackSpeedStat);
            if (this.modelAnimator)
            {
                this.PlayAnimation("Gesture, Additive", "DoubleSlash", "DoubleSlash.playbackRate", this.duration);
                this.PlayAnimation("Gesture, Override", "DoubleSlash", "DoubleSlash.playbackRate", this.duration);
            }
            if (!this.characterBody)
                return;
            this.characterBody.SetAimTimer(this.duration + 2f);
        }

        public override void OnExit()
        {
            this.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            base.OnExit();
        }

        private void HandleSlash(string animatorParamName, string muzzleName)
        {
            if (this.modelAnimator.GetFloat(animatorParamName) <= 0.1)
                return;

            Util.PlaySound(DoubleSlash.slashSoundString, this.gameObject);
            EffectManager.SimpleMuzzleFlash(DoubleSlash.swipeEffectPrefab, this.gameObject, muzzleName, true);
            this.slashCount++;
            var aimRay = PredictionUtils.PredictAimray(this.GetAimRay(), this.characterBody, ImpChanges.impVoidSpikes);
            ProjectileManager.instance.FireProjectile(ImpChanges.impVoidSpikes, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat, 0.0f, base.RollCrit());
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && this.modelAnimator)
            {
                switch (this.slashCount)
                {
                    case 0:
                        this.HandleSlash("HandR.hitBoxActive", "SwipeRight");
                        break;
                    case 1:
                        this.HandleSlash("HandL.hitBoxActive", "SwipeLeft");
                        break;
                }
            }

            if (this.fixedAge >= this.duration && this.isAuthority)
                this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
