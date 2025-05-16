using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Projectile;
using EntityStates;
using RoR2.CharacterAI;
using RoR2;
using RoR2.Skills;

namespace EnemiesPlus.Content.Imp
{
    internal class ImpChanges
    {
        public static GameObject impVoidSpikes;

        private GameObject ImpMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpMaster.prefab").WaitForCompletion();
        private GameObject ImpBody => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpBody.prefab").WaitForCompletion();
        public static ImpChanges Instance { get; private set; }

        public static void Init() => Instance ??= new ImpChanges();

        private ImpChanges()
        {
            impVoidSpikes = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectile.prefab").WaitForCompletion(), "ImpVoidSpikeProjectileScore");
            impVoidSpikes.GetComponent<ProjectileImpactExplosion>().destroyOnWorld = true;
            ContentAddition.AddProjectile(impVoidSpikes);
            ImpVoidSpike.projectilePrefab = impVoidSpikes;

            CreateSpikeSkillFamily();

            var spikeDriver = ImpMaster.AddComponent<AISkillDriver>();
            spikeDriver.customName = "ImpVoidSpikes";
            spikeDriver.skillSlot = SkillSlot.Secondary;
            spikeDriver.maxDistance = 30f;
            spikeDriver.minDistance = 15f;
            spikeDriver.shouldSprint = false;
            spikeDriver.selectionRequiresAimTarget = true;
            spikeDriver.selectionRequiresTargetLoS = true;
            spikeDriver.requireSkillReady = true;
            spikeDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            spikeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            spikeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;

            ImpMaster.ReorderSkillDrivers(2);
        }

        private void CreateSpikeSkillFamily()
        {
            var impSpikes = ScriptableObject.CreateInstance<SkillDef>();

            impSpikes.skillName = "ImpVoidSpikes";
            (impSpikes as ScriptableObject).name = "ImpVoidSpikes";
            impSpikes.skillNameToken = "Void Spikes";
            impSpikes.skillDescriptionToken = "Throw spikes";

            impSpikes.activationState = ContentAddition.AddEntityState<ImpVoidSpike>(out _);
            impSpikes.activationStateMachineName = "Weapon";
            impSpikes.interruptPriority = InterruptPriority.Pain;

            impSpikes.baseMaxStock = 1;
            impSpikes.baseRechargeInterval = 5f;

            impSpikes.rechargeStock = 1;
            impSpikes.requiredStock = 1;
            impSpikes.stockToConsume = 1;

            impSpikes.dontAllowPastMaxStocks = true;
            impSpikes.beginSkillCooldownOnSkillEnd = true;
            impSpikes.canceledFromSprinting = false;
            impSpikes.forceSprintDuringState = false;
            impSpikes.fullRestockOnAssign = true;
            impSpikes.resetCooldownTimerOnUse = false;
            impSpikes.isCombatSkill = true;
            impSpikes.mustKeyPress = false;
            impSpikes.cancelSprintingOnActivation = false;

            ContentAddition.AddSkillDef(impSpikes);

            var skill = ImpBody.AddComponent<GenericSkill>();
            skill.skillName = "ImpVoidSpikes";

            var newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = "ImpVoidSpikesFamily";
            newFamily.variants = [new SkillFamily.Variant() { skillDef = impSpikes }];

            skill._skillFamily = newFamily;
            ContentAddition.AddSkillFamily(newFamily);

            ImpBody.GetComponent<SkillLocator>().secondary = skill;
        }
    }
}
