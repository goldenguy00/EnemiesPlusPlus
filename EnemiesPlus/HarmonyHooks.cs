
using System.Linq;
using System.Runtime.CompilerServices;
using EntityStates;
using HarmonyLib;
using RoR2;
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
