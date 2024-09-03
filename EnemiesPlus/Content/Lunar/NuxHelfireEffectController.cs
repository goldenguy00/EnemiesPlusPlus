using RoR2;
using UnityEngine;

namespace EnemiesPlus.Content.Lunar
{
    public class NuxHelfireEffectController : MonoBehaviour
    {
        private BurnEffectController burnEffectController;

        private void Start()
        {
            var victimBody = this.GetComponent<CharacterBody>();
            var modelLocator = this.GetComponent<ModelLocator>();

            if (victimBody)
                EffectManager.SpawnEffect(LunarChanges.Instance.HelfireIgniteEffect, new EffectData()
                {
                    origin = victimBody.corePosition
                }, true);
            if (modelLocator && modelLocator.modelTransform)
            {
                this.burnEffectController = this.gameObject.AddComponent<BurnEffectController>();
                this.burnEffectController.effectType = BurnEffectController.helfireEffect;
                this.burnEffectController.target = modelLocator.modelTransform.gameObject;
            }
        }

        private void OnDestroy()
        {
            if (this.burnEffectController)
                Destroy(this.burnEffectController);
        }
    }
}