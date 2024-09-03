using EntityStates;
using RoR2;
using UnityEngine;

namespace EnemiesPlus.Content.Beetle
{
    public class ExitBurrow : BaseState
    {
        public static float ANIM_DURATION_COEF = 0.7f;
        public static float baseBurrowExitDuration = 1.3f;
        public static float exitJumpMarker = 0.4f;
        public static string endSoundString = "Play_hermitCrab_unburrow";
        public static string burrowSoundString = "Play_treeBot_sprint_end";

        private float duration;
        private bool didJump;
        private bool didCancelAnimation;
        private bool movementHitAuthority;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseBurrowExitDuration / attackSpeedStat;
            PlayAnimation("Body", "Spawn1", "Spawn1.playbackRate", duration / ANIM_DURATION_COEF);
            if (isAuthority)
                characterMotor.onMovementHit += OnMovementHit;
            /*if (characterMotor)
            {
                characterMotor.Motor.ForceUnground();
                //characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, 5f), characterMotor.velocity.z);
                characterMotor.velocity += Vector3.up * Instance.exitVelocityBonus;
            }*/
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration * exitJumpMarker / ANIM_DURATION_COEF)
                TryJump();
            if (fixedAge < duration)
                return;
            TryCancelAnimation();
            if (isAuthority && (fixedAge >= duration || movementHitAuthority || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            TryJump();
            TryCancelAnimation();
            if (isAuthority)
                characterMotor.onMovementHit -= OnMovementHit;
            base.OnExit();
        }

        public void TryJump()
        {
            if (!didJump && characterMotor)
            {
                EffectManager.SimpleEffect(BeetleChanges.burrowFX, characterBody.footPosition, Quaternion.identity, false);
                Util.PlaySound(burrowSoundString, gameObject);
                Util.PlaySound(endSoundString, gameObject);
                characterMotor.Motor.ForceUnground();
                didJump = true;
            }
        }

        public void TryCancelAnimation()
        {
            if (!didCancelAnimation)
            {
                PlayCrossfade("Body", isGrounded ? "Idle" : "Jump", 1f);
                didCancelAnimation = true;
            }
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            movementHitAuthority = true;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
