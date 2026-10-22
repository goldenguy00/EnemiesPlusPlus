using EntityStates.Gup;
using MiscFixes.Modules;
using R2API;
using RoR2;
using RoR2.Skills;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EnemiesPlus.Content.Gup
{
    public class GupChanges
    {
        public static GupChanges Instance { get; private set; }
        public static void Init() => Instance ??= new GupChanges();

        private GupChanges()
        {
            var esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(RoR2_DLC1_Gup.EntityStates_Gup_GupSpikesState_asset).WaitForCompletion();
            var newEsc = ScriptableObject.Instantiate(esc);
            newEsc.serializedFieldsCollection.serializedFields = [..esc.serializedFieldsCollection.serializedFields];
            newEsc.targetType = (HG.SerializableSystemType)typeof(GupSpikesPlus);
            newEsc.SetNameFromTargetType();
            newEsc.TryModifyFieldValue("crossfadeDuration", 0.1f);
            ContentAddition.AddEntityStateConfiguration(newEsc);

            var skillDef = Addressables.LoadAssetAsync<SkillDef>(RoR2_DLC1_Gup.GupSpikes_asset).WaitForCompletion();
            skillDef.activationState = ContentAddition.AddEntityState(typeof(GupSpikesPlus), out _);
        }
    }

    public class GupSpikesPlus : GupSpikesState
    {
        public override bool allowExitFire => false;
        public override bool GetShouldFire() => base.GetShouldFire() && fixedAge > duration * 0.3f;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (initialHitBox)
            {
                initialHitBox.enabled &= fixedAge > duration * 0.3f;
            }
        }
    }
}
