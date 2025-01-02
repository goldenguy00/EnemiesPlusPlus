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
using RoR2.Projectile;

namespace EnemiesPlus.Content.Lunar
{
    internal class LunarChanges
    {
        public static GameObject lunarWispTrackingBomb;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().InstantiateClone("LunarWispOrbScore");
        public static BuffDef helfireDebuff;
        public static DamageAPI.ModdedDamageType helfireDT;
        public static DotController.DotIndex helfireDotIdx;
        public static BurnEffectController.EffectParams lunarHelfireEffect;
        public static GameObject lunarHelfireIgniteEffectPrefab;

        internal Sprite FireBuffSprite => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffOnFireIcon.tif").WaitForCompletion();
        internal GameObject LunarGolemMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemMaster.prefab").WaitForCompletion();
        internal SkillDef LunarGolemShield => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/LunarGolem/LunarGolemBodyShield.asset").WaitForCompletion();
        internal GameObject LunarWisp => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab").WaitForCompletion();
        internal GameObject LunarWispMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab").WaitForCompletion();
        internal EntityStateConfiguration LunarSeekingBomb => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LunarWisp/EntityStates.LunarWisp.SeekingBomb.asset").WaitForCompletion();
        internal EntityStateConfiguration LunarShell => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LunarGolem/EntityStates.LunarGolem.Shell.asset").WaitForCompletion();
        internal EntityStateConfiguration LunarWispGat => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LunarWisp/EntityStates.LunarWisp.FireLunarGuns.asset").WaitForCompletion();

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
            (helfireDebuff as ScriptableObject).name = "LunarHelfireDebuff";
            helfireDebuff.canStack = true;
            helfireDebuff.isDOT = true;
            helfireDebuff.isCooldown = false;
            helfireDebuff.isDebuff = true;
            helfireDebuff.buffColor = Color.cyan;
            helfireDebuff.iconSprite = FireBuffSprite;
            ContentAddition.AddBuffDef(helfireDebuff);

            helfireDT = DamageAPI.ReserveDamageType();
            helfireDotIdx = DotAPI.RegisterDotDef(0.2f, 0f, DamageColorIndex.DeathMark, helfireDebuff, AddPercentHelfireDamage, AddHelfireDotVisuals);

            lunarHelfireIgniteEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HelfireIgniteEffect");
            lunarHelfireEffect = new BurnEffectController.EffectParams
            {
                startSound = "Play_item_proc_igniteOnKill_Loop",
                stopSound = "Stop_item_proc_igniteOnKill_Loop"
            };
            LegacyResourcesAPI.LoadAsyncCallback("Materials/matOnHelfire", delegate (Material val)
            {
                lunarHelfireEffect.overlayMaterial = val;
            });
            LegacyResourcesAPI.LoadAsyncCallback("Prefabs/HelfireEffect", delegate (GameObject val)
            {
                lunarHelfireEffect.fireEffectPrefab = val;
            });

            lunarWispTrackingBomb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().InstantiateClone("LunarWispOrbScore");
            lunarWispTrackingBomb.GetComponent<ProjectileDamage>().damageType.AddModdedDamageType(helfireDT);

            ContentAddition.AddProjectile(lunarWispTrackingBomb);

            LunarSeekingBomb.TryModifyFieldValue(nameof(SeekingBomb.projectilePrefab), lunarWispTrackingBomb);

            if (LunarWispGat.TryGetFieldValueString<float>(nameof(FireLunarGuns.baseDamagePerSecondCoefficient), out var baseDmg))
                LunarWispGat.TryModifyFieldValue<float>(nameof(FireLunarGuns.baseDamagePerSecondCoefficient), baseDmg * 0.5f);
        }

        private static void AddHelfireDotVisuals(DotController self)
        {
            if (self.victimBody && self.HasDotActive(helfireDotIdx))
            {
                var modelTransform = self.victimBody.modelLocator ? self.victimBody.modelLocator.modelTransform : null;
                if (modelTransform && !self.GetComponent<LunarHelfireController>())
                {
                    var ctrl = self.gameObject.AddComponent<LunarHelfireController>();
                    ctrl.target = modelTransform.gameObject;
                    ctrl.effectType = lunarHelfireEffect;
                    ctrl.dotController = self;
                }
            }
        }

        private static void AddPercentHelfireDamage(DotController self, DotController.DotStack dotStack)
        {
            if (dotStack?.dotIndex == helfireDotIdx)
            {
                if (self.victimBody && self.victimBody.healthComponent)
                {
                    dotStack.damageType |= DamageType.NonLethal;
                    // 6s/0.2 = 30t total
                    // 30 * 0.4 = 12%
                    // % Hp no shield
                    dotStack.damage = self.victimBody.healthComponent.fullHealth * 0.01f * 0.4f;

                    EffectManager.SpawnEffect(lunarHelfireIgniteEffectPrefab, new EffectData
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
            LunarShell.TryModifyFieldValue(nameof(Shell.buffDuration), 12f);
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
            if (damageReport.damageInfo.rejected || !damageReport.attackerBody || !damageReport.victimBody)
                return;

            if (damageReport.attackerBody.HasBuff(RoR2Content.Buffs.LunarShell) || damageReport.damageInfo.HasModdedDamageType(helfireDT))
            {
                if (Util.CheckRoll(25f * damageReport.damageInfo.procCoefficient, damageReport.attackerMaster))
                {
                    DotController.InflictDot(damageReport.victim.gameObject, damageReport.attacker, helfireDotIdx, 6f, 1f, 3);
                    damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, 6f);
                }
            }
        }
        #endregion
    }
}
