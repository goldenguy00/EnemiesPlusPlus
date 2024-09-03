using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using EntityStates;
using System.Linq;
using RoR2.CharacterAI;
using EntityStates.LunarWisp;
using RoR2.Skills;

namespace EnemiesPlus.Content.Lunar
{
    internal class LunarChanges
    {
        public static GameObject lunarWispTrackingBomb;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().InstantiateClone("LunarWispOrbScore");
        public static BuffDef helfireBuff;
        public static DamageAPI.ModdedDamageType helfireDT;
        public static DotController.DotIndex helfireDotIdx;

        internal Sprite FireBuffSprite => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffOnFireIcon.tif").WaitForCompletion();
        internal GameObject LunarGolemMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemMaster.prefab").WaitForCompletion();
        internal SkillDef LunarGolemShield => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/LunarGolem/LunarGolemBodyShield.asset").WaitForCompletion();
        internal GameObject LunarWisp => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab").WaitForCompletion();
        internal GameObject LunarWispMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab").WaitForCompletion();
        internal GameObject HelfireIgniteEffect => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BurnNearby/HelfireIgniteEffect.prefab").WaitForCompletion();
        internal EntityStateConfiguration LunarSeekingBomb => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LunarWisp/EntityStates.LunarWisp.SeekingBomb.asset").WaitForCompletion();

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
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.HealthComponent.Heal += PreventHelfireHeal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            helfireBuff = ScriptableObject.CreateInstance<BuffDef>();
            (helfireBuff as ScriptableObject).name = "HelfireDebuff";
            helfireBuff.canStack = true;
            helfireBuff.isCooldown = false;
            helfireBuff.isDebuff = true;
            helfireBuff.buffColor = Color.cyan;
            helfireBuff.iconSprite = FireBuffSprite;
            ContentAddition.AddBuffDef(helfireBuff);

            helfireDT = DamageAPI.ReserveDamageType();
            helfireDotIdx = DotAPI.RegisterDotDef(0.2f, 0f, DamageColorIndex.DeathMark, helfireBuff, AddPercentHelfireDamage);

            lunarWispTrackingBomb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().InstantiateClone("LunarWispOrbScore");
            lunarWispTrackingBomb.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(helfireDT);
            ContentAddition.AddProjectile(lunarWispTrackingBomb);

            LunarSeekingBomb.TryModifyFieldValue(nameof(SeekingBomb.projectilePrefab), lunarWispTrackingBomb);
        }

        private void AddPercentHelfireDamage(DotController self, DotController.DotStack dotStack)
        {
            if (dotStack?.dotIndex == helfireDotIdx)
            {
                if (self.victimBody && self.victimBody.healthComponent)
                {
                    dotStack.damageType |= DamageType.NonLethal;
                    dotStack.damage = self.victimBody.healthComponent.fullHealth * 0.1f / 10f * 0.2f; ;
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
            LunarGolemShield.baseRechargeInterval = 60f;
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
        private float PreventHelfireHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if (self.body && self.body.HasBuff(helfireBuff))
                return 0f;

            return orig(self, amount, procChainMask, nonRegen);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(RoR2Content.Buffs.LunarShell))
                args.armorAdd += 200f;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (!damageReport.damageInfo?.rejected == false && damageReport.attackerBody && damageReport.victimBody &&
                (damageReport.attackerBody.HasBuff(RoR2Content.Buffs.LunarShell) || damageReport.damageInfo.HasModdedDamageType(helfireDT)))
                    DotController.InflictDot(damageReport.victimBody.gameObject, damageReport.attacker, helfireDotIdx, 10f, 1f);
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == helfireBuff)
                self.gameObject.AddComponent<NuxHelfireEffectController>();
        }

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == helfireBuff && self.TryGetComponent<NuxHelfireEffectController>(out var ctrl))
                Object.Destroy(ctrl);
        }
        #endregion
    }
}
