using UnityEngine.Networking;
using UnityEngine;
using EntityStates;
using RoR2;

namespace EnemiesPlus.Content.Beetle
{
    public class EnterBurrow : BaseState
    {
        public static float animSpeed = 1.3f;
        public static float crossfadeDelay = 1.1f;
        public static float crossfadeDuration = 0.2f;
        public static string burrowSoundString = "Play_treeBot_sprint_end";
        public static string startSoundString = "Play_beetle_worker_idle";

        private Animator modelAnimator;
        private float duration;
        private bool didCrossfade;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = crossfadeDelay + crossfadeDuration; //Instance.baseBurrowEntryDuration;
            PlayCrossfade("Body", "EmoteSurprise", 0.1f);
            modelAnimator = GetModelAnimator();
            if (modelAnimator)
                modelAnimator.speed = animSpeed;
            Util.PlaySound(startSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= crossfadeDelay)
                TryCrossfade();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(new BeetleBurrow());
                return;
            }
        }

        public override void OnExit()
        {
            TryCrossfade();
            if (NetworkServer.active)
                Util.CleanseBody(characterBody, true, false, false, true, false, false);
            base.OnExit();
        }

        public void TryCrossfade()
        {
            if (!didCrossfade && modelAnimator)
            {
                modelAnimator.speed = 1f;
                modelAnimator.Update(0f);
                modelAnimator.SetFloat("Spawn1.playbackRate", 0f);
                modelAnimator.CrossFadeInFixedTime("Spawn1", crossfadeDuration, modelAnimator.GetLayerIndex("Body"));

                EffectManager.SimpleEffect(BeetleChanges.burrowFX, characterBody.footPosition, Quaternion.identity, false);
                Util.PlaySound(burrowSoundString, gameObject);
                didCrossfade = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}
