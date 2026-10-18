
using System.Linq;
using System.Runtime.CompilerServices;
using EntityStates;
using HarmonyLib;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
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

        }

        private static void Chicken(DirectorCard card)
        {
            var chickenCard = RiftTitansMod.RiftTitansPlugin.ChickenCard.Card;
            chickenCard.minimumStageCompletions = 1;
            var master = chickenCard.spawnCard.prefab.GetComponent<CharacterMaster>();

            var ssoh = master.bodyPrefab.GetComponent<SetStateOnHurt>();
            ssoh.hurtState = new(typeof(EntityStates.HurtState));
            ssoh.targetStateMachine = master.bodyPrefab.GetComponent<EntityStateMachine>();

            var shootSkill = master.bodyPrefab.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef;
            shootSkill.beginSkillCooldownOnSkillEnd = true;
            shootSkill.baseRechargeInterval = 0.5f;
            shootSkill.isCombatSkill = true;

            AISkillDriver aISkillDriver2 = master.gameObject.AddComponent<AISkillDriver>();
            aISkillDriver2.customName = "Strafe";
            aISkillDriver2.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            aISkillDriver2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            aISkillDriver2.activationRequiresAimConfirmation = false;
            aISkillDriver2.activationRequiresTargetLoS = false;
            aISkillDriver2.selectionRequiresTargetLoS = true;
            aISkillDriver2.maxDistance = 40f;
            aISkillDriver2.minDistance = 15f;
            aISkillDriver2.requireSkillReady = false;
            aISkillDriver2.aimType = AISkillDriver.AimType.MoveDirection;
            aISkillDriver2.ignoreNodeGraph = false;
            //aISkillDriver2.moveInputScale = 1f;
            //aISkillDriver2.driverUpdateTimerOverride = 0.25f;
            aISkillDriver2.buttonPressType = AISkillDriver.ButtonPressType.Abstain;
            //aISkillDriver2.minTargetHealthFraction = float.NegativeInfinity;
            //aISkillDriver2.maxTargetHealthFraction = float.PositiveInfinity;
            //aISkillDriver2.minUserHealthFraction = float.NegativeInfinity;
            //aISkillDriver2.maxUserHealthFraction = float.PositiveInfinity;
            aISkillDriver2.skillSlot = SkillSlot.None;

            var aiList = master.GetComponents<RoR2.CharacterAI.AISkillDriver>();
            foreach (var ai in aiList)
            {
                switch (ai.customName)
                {
                    case "Chase":
                        break;
                    case "Shoot":
                        ai.requireSkillReady = true;
                        ai.nextHighPriorityOverride = aISkillDriver2;
                        ai.selectionRequiresOnGround = true;
                        break;
                    case "Flee":
                        break;
                    case "Strafe":
                        break;
                }
            }
        }

        private static void Reksai(DirectorCard card)
        {
            RiftTitansMod.RiftTitansPlugin.ReksaiCard.MonsterCategory = R2API.DirectorAPI.MonsterCategory.Champions;
            var reksaiCard = RiftTitansMod.RiftTitansPlugin.ReksaiCard.Card;
            reksaiCard.spawnCard.directorCreditCost = 600;
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

    [HarmonyPatch(typeof(RIFT_SKILLS.Blue.Slam), nameof(RIFT_SKILLS.Blue.Slam.GetMinimumInterruptPriority))]
    public class RiftFlinch
    {
        [HarmonyPostfix]
        private static void Postfix(ref InterruptPriority __result)
        {
            __result = InterruptPriority.Pain;
        }
    }
}
