using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using EntityStates;
using System.Linq;
using RoR2.CharacterAI;
using EntityStates.LunarWisp;
using RoR2.Skills;
using EntityStates.LunarGolem;

namespace EnemiesPlus.Content.Lunar
{
    internal class LunarChanges
    {
        public static GameObject lunarWispTrackingBomb;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().InstantiateClone("LunarWispOrbScore");
        public static BuffDef helfireDebuff;
        public static DamageAPI.ModdedDamageType helfireDT;
        public static DotController.DotIndex helfireDotIdx;

        internal Sprite FireBuffSprite => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffOnFireIcon.tif").WaitForCompletion();
        internal GameObject LunarGolemMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemMaster.prefab").WaitForCompletion();
        internal SkillDef LunarGolemShield => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/LunarGolem/LunarGolemBodyShield.asset").WaitForCompletion();
        internal GameObject LunarWisp => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab").WaitForCompletion();
        internal GameObject LunarWispMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab").WaitForCompletion();
        internal EntityStateConfiguration LunarSeekingBomb => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LunarWisp/EntityStates.LunarWisp.SeekingBomb.asset").WaitForCompletion();
        internal EntityStateConfiguration LunarShell => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LunarGolem/EntityStates.LunarGolem.Shell.asset").WaitForCompletion();

        public static LunarChanges Instance { get; private set; }

        public static void Init() => Instance ??= new LunarChanges();

        private LunarChanges()
        {
            if (EnemiesPlusConfig.enableTweaks.Value)
            {
                if (EnemiesPlusConfig.helfireChanges.Value)
                    CreateHelfireDebuff();

                if (EnemiesPlusConfig.lunarWispChanges.Value)
                    LunarWispChanges();
            }

            if (EnemiesPlusConfig.enableSkills.Value && EnemiesPlusConfig.lunarGolemSkills.Value)
                LunarGolemChanges();
        }

        #region Changes
        private void CreateHelfireDebuff()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;

            helfireDebuff = ScriptableObject.CreateInstance<BuffDef>();
            (helfireDebuff as ScriptableObject).name = "HelfireDebuff";
            helfireDebuff.canStack = true;
            helfireDebuff.isCooldown = false;
            helfireDebuff.isDebuff = true;
            helfireDebuff.buffColor = Color.cyan;
            helfireDebuff.iconSprite = FireBuffSprite;
            ContentAddition.AddBuffDef(helfireDebuff);

            helfireDT = DamageAPI.ReserveDamageType();
            helfireDotIdx = DotAPI.RegisterDotDef(0.2f, 0f, DamageColorIndex.DeathMark, helfireDebuff, AddPercentHelfireDamage, AddHelfireDotVisuals);

            lunarWispTrackingBomb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().InstantiateClone("LunarWispOrbScore");
            lunarWispTrackingBomb.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(helfireDT);
            ContentAddition.AddProjectile(lunarWispTrackingBomb);

            LunarSeekingBomb.TryModifyFieldValue(nameof(SeekingBomb.projectilePrefab), lunarWispTrackingBomb);
        }

        private static void AddHelfireDotVisuals(DotController self)
        {
            if (self.victimObject)
                return;

            if (self.HasDotActive(helfireDotIdx))
            {
                if (!self.helfireEffectController)
                {
                    var modelLocator = self.victimObject.GetComponent<ModelLocator>();
                    if ((bool)modelLocator && (bool)modelLocator.modelTransform)
                    {
                        self.helfireEffectController = self.gameObject.AddComponent<BurnEffectController>();
                        self.helfireEffectController.effectType = BurnEffectController.helfireEffect;
                        self.helfireEffectController.target = modelLocator.modelTransform.gameObject;
                    }
                }
            }
            else if ((bool)self.helfireEffectController)
            {
                self.helfireEffectController.HandleDestroy();
                self.helfireEffectController = null;
            }
        }

        private static void AddPercentHelfireDamage(DotController self, DotController.DotStack dotStack)
        {
            if (dotStack?.dotIndex == helfireDotIdx)
            {
                if (self.victimBody && self.victimBody.healthComponent)
                {
                    dotStack.damageType |= DamageType.NonLethal;
                    dotStack.damage = self.victimBody.healthComponent.fullHealth * 0.01f * 0.4f;

                    if (DotController.HelfireIgniteEffectPrefab == null)
                    {
                        DotController.HelfireIgniteEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HelfireIgniteEffect");
                    }
                    EffectManager.SpawnEffect(DotController.HelfireIgniteEffectPrefab, new EffectData
                    {
                        origin = self.victimBody.corePosition
                    }, transmit: true);
                }
            }
        }

        private void LunarWispChanges()
        {
            var lunarWispBody = LunarWisp.GetComponent<CharacterBody>();
            lunarWispBody.baseMoveSpeed = 20f;
            lunarWispBody.baseAcceleration = 20f;

            foreach (var driver in LunarWispMaster.GetComponents<AISkillDriver>())
                switch (driver.customName)
                {
                    case "Back Up":
                        driver.maxDistance = 15f;
                        break;
                    case "Minigun":
                        driver.minDistance = 15f;
                        driver.maxDistance = 75f;
                        break;
                    case "Chase":
                        driver.minDistance = 30f;
                        driver.shouldSprint = true;
                        break;
                }
        }

        private void LunarGolemChanges()
        {
            LunarShell.TryModifyFieldValue(nameof(Shell.buffDuration), 8f);
            LunarGolemShield.baseRechargeInterval = 30f;
            LunarGolemMaster.GetComponents<AISkillDriver>().Where(driver => driver.customName == "StrafeAndShoot").First().requireSkillReady = true;

            var lunarShellDriver = LunarGolemMaster.AddComponent<AISkillDriver>();
            lunarShellDriver.customName = "LunarShell";
            lunarShellDriver.skillSlot = SkillSlot.Secondary;
            lunarShellDriver.requireSkillReady = true;
            lunarShellDriver.requireEquipmentReady = false;
            lunarShellDriver.driverUpdateTimerOverride = 3f;
            lunarShellDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            lunarShellDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;

            LunarGolemMaster.ReorderSkillDrivers(0);
        }
        #endregion

        #region Hooks
        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.damageInfo?.rejected == false && damageReport.attackerBody && damageReport.victimBody &&
               (damageReport.attackerBody.HasBuff(RoR2Content.Buffs.LunarShell) || damageReport.damageInfo.HasModdedDamageType(helfireDT)) &&
                Util.CheckRoll(0.2f * damageReport.damageInfo.procCoefficient, damageReport.attackerMaster ?? damageReport.attackerOwnerMaster))
            {
                DotController.InflictDot(damageReport.victimBody.gameObject, damageReport.attacker, helfireDotIdx, 6f, damageReport.damageInfo.procCoefficient);
                damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, 6f);
            }
        }
        #endregion
    }
}
