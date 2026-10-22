
using System.Linq;
using System.Runtime.CompilerServices;
using EntityStates;
using HarmonyLib;
using HG;
using RiftTitansMod;
using RoR2;
using RoR2.CharacterAI;
using RIFT_SKILLS = RiftTitansMod.SkillStates;

namespace EnemiesPlus
{
    public static class HarmonyHooks
    {
        public static Harmony Patcher;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init()
        {
            Patcher = new Harmony(EnemiesPlusPlugin.PluginGUID);
            if (EnemiesPlusPlugin.LeagueOfLiteralGays)
                League();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void League()
        {
            Patcher.CreateClassProcessor(typeof(RiftFlinch)).Patch();

            Blue();
            Chicken();
            Reksai();
        }
        private static void Blue()
        {
            var blueCard = RiftTitansPlugin.BlueCard.Card;
            var master = blueCard.spawnCard.prefab.GetComponent<CharacterMaster>();

            var ssoh = master.bodyPrefab.EnsureComponent<SetStateOnHurt>();
            ssoh.hurtState = new(typeof(EntityStates.HurtState));
            ssoh.targetStateMachine = master.bodyPrefab.GetComponent<EntityStateMachine>();
        }
        private static void Chicken()
        {
            var chickenCard = RiftTitansMod.RiftTitansPlugin.ChickenCard.Card;
            // chickenCard.minimumStageCompletions = 1;
            var master = chickenCard.spawnCard.prefab.GetComponent<CharacterMaster>();

            var ssoh = master.bodyPrefab.EnsureComponent<SetStateOnHurt>();
            ssoh.hurtState = new(typeof(EntityStates.HurtState));
            ssoh.targetStateMachine = master.bodyPrefab.GetComponent<EntityStateMachine>();

            var shootSkill = master.bodyPrefab.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef;
            shootSkill.beginSkillCooldownOnSkillEnd = true;
            shootSkill.mustKeyPress = true;

            AISkillDriver aISkillDriver2 = master.gameObject.AddComponent<AISkillDriver>();
            aISkillDriver2.customName = "Strafe";
            aISkillDriver2.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            aISkillDriver2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            aISkillDriver2.activationRequiresAimConfirmation = false;
            aISkillDriver2.maxDistance = 40f;
            aISkillDriver2.minDistance = 0f;
            aISkillDriver2.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            aISkillDriver2.ignoreNodeGraph = false;
            //aISkillDriver2.driverUpdateTimerOverride = 0.5f;
            aISkillDriver2.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            aISkillDriver2.skillSlot = SkillSlot.Primary;
            aISkillDriver2.selectionRequiresOnGround = true;
            aISkillDriver2.selectionRequiresTargetLoS = true;

            var aiList = master.GetComponents<RoR2.CharacterAI.AISkillDriver>();
            foreach (var ai in aiList)
            {
                switch (ai.customName)
                {
                    case "Chase":
                        break;
                    case "Shoot":
                        ai.requireSkillReady = true;
                        ai.requiredSkill = shootSkill;
                        ai.skillSlot = SkillSlot.Primary;
                        ai.selectionRequiresSkillFullyStocked = true;
                        //ai.nextHighPriorityOverride = aISkillDriver2;
                        ai.selectionRequiresOnGround = true;
                        ai.selectionRequiresTargetLoS = true;
                        //ai.driverUpdateTimerOverride = 0.5f;
                        break;
                    case "Flee":
                        break;
                    case "Strafe":
                        break;
                }
            }
        }

        private static void Reksai()
        {
            RiftTitansMod.RiftTitansPlugin.ReksaiCard.MonsterCategory = R2API.DirectorAPI.MonsterCategory.Champions;
            var reksaiCard = RiftTitansMod.RiftTitansPlugin.ReksaiCard.Card;
            reksaiCard.spawnCard.directorCreditCost = 800;
            reksaiCard.minimumStageCompletions = 2;
            var master = reksaiCard.spawnCard.prefab.GetComponent<CharacterMaster>();
            var aiList = master.GetComponents<RoR2.CharacterAI.AISkillDriver>();

            foreach (var ai in aiList)
            {
                switch (ai.customName)
                {
                    case "Special":
                        break;
                    case "Seeker":
                        break;
                    case "ChaseHard":
                        ai.shouldSprint = false;
                        break;
                    case "Attack":
                        ai.driverUpdateTimerOverride = 0.5f;
                        ai.nextHighPriorityOverride = aiList.Last();
                        break;
                    case "Chase":
                        break;
                }
            }
            master.bodyPrefab.GetComponent<CharacterBody>().baseMoveSpeed = 10;
        }
    }

    [HarmonyPatch]
    public class RiftFlinch
    {
        [HarmonyPatch(typeof(RIFT_SKILLS.Blue.Slam), nameof(RIFT_SKILLS.Blue.Slam.GetMinimumInterruptPriority))]
        [HarmonyPostfix]
        private static void FixSlam(ref InterruptPriority __result)
        {
            __result = InterruptPriority.Pain;
        }

    }
}
