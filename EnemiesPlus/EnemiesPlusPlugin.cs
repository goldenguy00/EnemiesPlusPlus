using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using EnemiesPlus.Content.Beetle;
using EnemiesPlus.Content.Donger;
using EnemiesPlus.Content.Imp;
using EnemiesPlus.Content.Lemur;
using EnemiesPlus.Content.Lunar;
using EnemiesPlus.Content.Wisp;
using EnemiesPlus.Content.Worm;
using EnemiesPlus.Prediction;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace EnemiesPlus
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class EnemiesPlusPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = $"com.{PluginAuthor}.{PluginName}";
        public const string PluginAuthor = "score";
        public const string PluginName = "EnemiesPlusPlus";
        public const string PluginVersion = "1.0.0";

        public static EnemiesPlusPlugin Instance { get; private set; }

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public void Awake()
        {
            Instance = this;

            Log.Init(Logger);
            EnemiesPlusConfig.Init(Config);
            PredictionHooks.Init();

            if (EnemiesPlusConfig.enableSkills.Value)
            {
                if (EnemiesPlusConfig.bellSkills.Value) BellChanges.Init();
                if (EnemiesPlusConfig.impSkills.Value) ImpChanges.Init();
                if (EnemiesPlusConfig.wormSkills.Value) WormChanges.Init();
                if (EnemiesPlusConfig.lunarGolemSkills.Value) LunarChanges.Init();
            }

            if (EnemiesPlusConfig.enableTweaks.Value)
            {
                if (EnemiesPlusConfig.lemChanges.Value) LemurChanges.Init();
                if (EnemiesPlusConfig.wispChanges.Value || EnemiesPlusConfig.greaterWispChanges.Value) WispChanges.Init();
                if (EnemiesPlusConfig.helfireChanges.Value || EnemiesPlusConfig.lunarWispChanges.Value) LunarChanges.Init();
            }

            if (EnemiesPlusConfig.enableBeetleFamilyChanges.Value)
                BeetleChanges.Init();
        }
    }
}
