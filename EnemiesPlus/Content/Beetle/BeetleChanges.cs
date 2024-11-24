using System.Linq;
using EntityStates.BeetleMonster;
using Inferno.Stat_AI;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EnemiesPlus.Content.Beetle
{
    public class BeetleChanges
    {
        public static GameObject beetleSpit;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpit.prefab").WaitForCompletion().InstantiateClone("BeetleSpitProjectileScore");
        public static GameObject beetleSpitGhost;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpitGhost.prefab").WaitForCompletion().InstantiateClone("BeetleSpitProjectileGhostScore");
        public static GameObject beetleSpitExplosion;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleSpitExplosion.prefab").WaitForCompletion().InstantiateClone("BeetleSpitExplosionScore");
        public static GameObject burrowFX;// = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardSunderPop.prefab").WaitForCompletion().InstantiateClone("BeetleBurrowEffectScore");
        public static DamageAPI.ModdedDamageType beetleJuiceDT;

        internal GameObject BeetleMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleMaster.prefab").WaitForCompletion();
        internal GameObject BeetleBody => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleBody.prefab").WaitForCompletion();
        internal GameObject BeetleQueenSpit => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpit.prefab").WaitForCompletion();
        internal GameObject BeetleQueenAcid => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenAcid.prefab").WaitForCompletion();
        internal BuffDef BeetleJuice => Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Beetle/bdBeetleJuice.asset").WaitForCompletion();
        internal GameObject BeetleGuardMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardMaster.prefab").WaitForCompletion();
        internal GameObject BeetleGuardBody => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardBody.prefab").WaitForCompletion();
        internal SkillDef BGUtilitySkillDef => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Beetle/BeetleGuardBodyDefenseUp.asset").WaitForCompletion();
        internal SkillDef BeetlePrimary => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Beetle/BeetleBodyHeadbutt.asset").WaitForCompletion();
        internal SkillDef BeetleSecondary => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Beetle/BeetleBodySleep.asset").WaitForCompletion();
        internal EntityStateConfiguration BeetleSpawn => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Beetle/EntityStates.BeetleMonster.SpawnState.asset").WaitForCompletion();

        public static BeetleChanges Instance { get; private set; }

        public static void Init() => Instance ??= new BeetleChanges();

        private BeetleChanges()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;

            MakeJuice();

            if (EnemiesPlusConfig.beetleBurrow.Value || EnemiesPlusConfig.beetleSpit.Value)
                LilGuyChanges();

            if (EnemiesPlusConfig.bgChanges.Value)
                GuardChanges();

            if (EnemiesPlusConfig.queenChanges.Value)
                QueenChanges();
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport?.damageInfo?.rejected == false && damageReport.victimBody && damageReport.damageInfo.HasModdedDamageType(beetleJuiceDT))
                damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.BeetleJuice, 5f);
        }

        private void MakeJuice()
        {
            BeetleJuice.canStack = true;
            beetleJuiceDT = DamageAPI.ReserveDamageType();

            beetleSpit = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpit.prefab").WaitForCompletion().InstantiateClone("BeetleSpitProjectileScore"); 
            beetleSpit.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(beetleJuiceDT);
            beetleSpit.GetComponent<ProjectileSimple>().desiredForwardSpeed = 60f;
            beetleSpit.transform.localScale /= 2;
            beetleSpit.GetComponent<Rigidbody>().useGravity = false;

            beetleSpitExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleSpitExplosion.prefab").WaitForCompletion().InstantiateClone("BeetleSpitExplosionScore", false);
            beetleSpitExplosion.transform.GetChild(0).localScale /= 2;
            foreach (Transform child in beetleSpitExplosion.transform.GetChild(0))
                child.localScale /= 2;

            var pie = beetleSpit.GetComponent<ProjectileImpactExplosion>();
            pie.impactEffect = beetleSpitExplosion;
            pie.childrenProjectilePrefab = null;
            pie.destroyOnEnemy = true;
            pie.fireChildren = false;
            pie.blastRadius = 2f;

            beetleSpitGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpitGhost.prefab").WaitForCompletion().InstantiateClone("BeetleSpitProjectileGhostScore", false);
            beetleSpitGhost.transform.localScale /= 2;
            beetleSpitGhost.transform.GetChild(1).localScale /= 2;
            beetleSpitGhost.transform.GetChild(0).GetChild(0).localScale /= 2;
            beetleSpitGhost.transform.GetChild(1).GetChild(0).localScale /= 2;
            beetleSpitGhost.transform.GetChild(1).GetChild(1).localScale /= 2;
            beetleSpit.GetComponent<ProjectileController>().ghostPrefab = beetleSpitGhost;


            ContentAddition.AddEffect(beetleSpitExplosion);
            ContentAddition.AddProjectile(beetleSpit);
            BeetleSpit.projectilePrefab = beetleSpit;
        }
        private void LilGuyChanges()
        {
            BeetleSpawn.TryModifyFieldValue(nameof(SpawnState.duration), 3.5f);
            BeetleBody.GetComponent<CharacterBody>().baseMoveSpeed = 7f;

            if (EnemiesPlusConfig.beetleSpit.Value)
                MakeSpit();
            if (EnemiesPlusConfig.beetleBurrow.Value)
                MakeBurrow();

            var master = BeetleMaster;
            var ai = master.GetComponent<BaseAI>();
            ai.aimVectorDampTime = 0.1f;
            ai.aimVectorMaxSpeed = 180f;

            foreach (var driver in master.GetComponents<AISkillDriver>())
            {
                switch (driver.customName)
                {
                    case "HeadbuttOffNodegraph":
                        driver.minDistance = 0f;
                        driver.maxDistance = 5f;
                        break;
                    case "SpitOffNodeGraph":
                    case "BurrowTowardsTarget":
                        break;
                    default:
                        driver.shouldSprint = true;
                        break;
                }
            }
        }
        private void MakeSpit()
        {
            var spit = BeetleSecondary.GetCopyOf(BeetlePrimary);
            (spit as ScriptableObject).name = "BeetleBodySpit";
            spit.activationState = ContentAddition.AddEntityState<BeetleSpit>(out _);
            spit.skillName = "BeetleSpit";
            spit.baseRechargeInterval = 3f;
            spit.baseMaxStock = 1;

            var loc = BeetleBody.GetComponent<SkillLocator>();
            loc.secondary.skillName = "BeetleSpit";

            var spitDriver = BeetleMaster.AddComponent<AISkillDriver>();
            spitDriver.customName = "SpitOffNodeGraph";
            spitDriver.skillSlot = SkillSlot.Secondary;
            spitDriver.selectionRequiresAimTarget = true;
            spitDriver.selectionRequiresTargetLoS = true;
            spitDriver.minDistance = 5f;
            spitDriver.maxDistance = 25f;
            spitDriver.ignoreNodeGraph = true;
            spitDriver.shouldSprint = true;
            spitDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            spitDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            spitDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;

            BeetleMaster.ReorderSkillDrivers(spitDriver, 1);
        }

        private void MakeBurrow()
        {
            ContentAddition.AddEntityState<BeetleBurrow>(out _);
            ContentAddition.AddEntityState<ExitBurrow>(out _);

            var burrowSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            (burrowSkillDef as ScriptableObject).name = "BeetleBodyBurrow";
            burrowSkillDef.skillName = "BeetleBurrow";
            burrowSkillDef.activationStateMachineName = "Body";
            burrowSkillDef.activationState = ContentAddition.AddEntityState<EnterBurrow>(out _);
            burrowSkillDef.baseRechargeInterval = 12f;
            burrowSkillDef.cancelSprintingOnActivation = false;
            burrowSkillDef.isCombatSkill = false;
            ContentAddition.AddSkillDef(burrowSkillDef);

            var utilFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (utilFamily as ScriptableObject).name = "BeetleBodyUtilityFamily";
            utilFamily.variants = [new SkillFamily.Variant() { skillDef = burrowSkillDef }];

            var skill = BeetleBody.AddComponent<GenericSkill>();
            skill.skillName = "BeetleBurrow";
            skill._skillFamily = utilFamily;

            var loc = BeetleBody.GetComponent<SkillLocator>();
            loc.utility = skill;

            ContentAddition.AddSkillFamily(utilFamily);

            var oldfollow = BeetleMaster.GetComponents<AISkillDriver>().Last();

            var burrowTowardsTarget = BeetleMaster.AddComponent<AISkillDriver>();
            burrowTowardsTarget.customName = "BurrowTowardsTarget";
            burrowTowardsTarget.skillSlot = SkillSlot.Utility;
            burrowTowardsTarget.requireSkillReady = true;
            burrowTowardsTarget.minDistance = 25f;
            burrowTowardsTarget.maxDistance = 60f;
            burrowTowardsTarget.selectionRequiresOnGround = true;
            burrowTowardsTarget.ignoreNodeGraph = true;
            burrowTowardsTarget.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            burrowTowardsTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            burrowTowardsTarget.aimType = AISkillDriver.AimType.AtMoveTarget;
            burrowTowardsTarget.resetCurrentEnemyOnNextDriverSelection = true;

            BeetleMaster.AddComponentCopy(oldfollow);
            Component.DestroyImmediate(oldfollow);

            burrowFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardSunderPop.prefab").WaitForCompletion().InstantiateClone("BeetleBurrowEffectScore", false);
            burrowFX.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            var dust = burrowFX.transform.Find("Particles/ParticleInitial/Dust");
            if (dust && dust.TryGetComponent(out ParticleSystemRenderer dustRenderer))
            {
                dustRenderer.sharedMaterial = new Material(dustRenderer.sharedMaterial);
                dustRenderer.sharedMaterial.SetColor("_TintColor", new Color32(201, 126, 44, 255));
            }

            var decal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion().transform.Find("ParticleInitial/Decal");
            if (decal)
            {
                var burrowDecal = Object.Instantiate(decal.gameObject, burrowFX.transform);
                burrowDecal.transform.localScale = Vector3.one * 5f;
            }
            ContentAddition.AddEffect(burrowFX);
        }

        private void QueenChanges()
        {
            BeetleQueenSpit.GetComponent<ProjectileImpactExplosion>().destroyOnEnemy = true;
            BeetleQueenSpit.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(beetleJuiceDT);
            BeetleQueenAcid.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(beetleJuiceDT);
        }

        private void GuardChanges()
        {
            var rallyCryDriver = BeetleGuardMaster.AddComponent<AISkillDriver>();
            rallyCryDriver.customName = "RallyCry";
            rallyCryDriver.skillSlot = SkillSlot.Utility;
            rallyCryDriver.requireSkillReady = true;
            rallyCryDriver.maxUserHealthFraction = 0.8f;
            rallyCryDriver.movementType = AISkillDriver.MovementType.Stop;

            BeetleGuardMaster.ReorderSkillDrivers(rallyCryDriver, 2);
            BGUtilitySkillDef.activationState = ContentAddition.AddEntityState<RallyCry>(out _);
        }
    }
}
