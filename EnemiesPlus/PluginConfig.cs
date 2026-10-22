using System.Runtime.CompilerServices;
using System;
using BepInEx.Configuration;
using MiscFixes.Modules;
using UnityEngine;

namespace EnemiesPlus
{
    public static class EnemiesPlusConfig
    {
        public static ConfigEntry<bool> enableSkills;
        public static ConfigEntry<bool> bellSkills;
        public static ConfigEntry<bool> impSkills;
        public static ConfigEntry<bool> wormTracking;
        public static ConfigEntry<bool> wormLeap;
        public static ConfigEntry<bool> lunarGolemSkills;

        public static ConfigEntry<bool> enableTweaks;
        public static ConfigEntry<bool> gupChanges;
        public static ConfigEntry<bool> lemChanges;
        public static ConfigEntry<bool> wispChanges;
        public static ConfigEntry<bool> greaterWispChanges;
        public static ConfigEntry<bool> helfireChanges;
        public static ConfigEntry<bool> lunarWispChanges;

        public static ConfigEntry<bool> enableBeetleFamilyChanges;
        public static ConfigEntry<bool> beetleBurrow;
        public static ConfigEntry<bool> beetleSpit;
        public static ConfigEntry<bool> bgChanges;
        public static ConfigEntry<bool> queenChanges;

        public static ConfigEntry<bool> compatChanges;

        private static ConfigFile PluginConfigFile { get; set; }

        public static void Init(ConfigFile cfg)
        {
            PluginConfigFile = cfg;
            if (EnemiesPlusPlugin.RooInstalled)
                InitRoO();

            BindSkills("Skills");
            BindTweaks("Tweaks");
            BindBeetles("Beetles");
            BindCompat("Compat");
        }

        private static void BindCompat(string section)
        {
            compatChanges = PluginConfigFile.BindOption(section,
                "Enable RiftTitans Compatibility",
                "Adjusts the enemies from this mod to feel more fair",
                true,
                Extensions.ConfigFlags.RestartRequired);
        }

        private static void BindSkills(string section)
        {
            enableSkills = PluginConfigFile.BindOption(section,
                "Enable New Skills",
                "Allows any of the skills within this section to be toggled on and off.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            bellSkills = PluginConfigFile.BindOption(section,
                "Enable Brass Contraptions Buff Beam Skill",
                "Adds a new skill that gives an ally increased armor.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            impSkills = PluginConfigFile.BindOption(section,
                "Enable Imps Void Spike Skill",
                "Adds a new skill for Imps to throw void spikes at range, similarly to the Imp OverLord.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            lunarGolemSkills = PluginConfigFile.BindOption(section,
                "Enable Lunar Golems Lunar Shell Skill",
                "Adds a new ability that gives them some stuff idk its vanilla but unused",
                true,
                Extensions.ConfigFlags.RestartRequired);

            wormLeap = PluginConfigFile.BindOption(section,
                "Enable Worms Leap Skill",
                "Adds a new leap skill.",
                true,
                Extensions.ConfigFlags.RestartRequired);

        }

        private static void BindTweaks(string section)
        {
            enableTweaks = PluginConfigFile.BindOption(section,
                "Enable Enemy Tweaks",
                "Allows any of the skills within this section to be toggled on and off.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            gupChanges = PluginConfigFile.BindOption(section,
                "Enable Gup Changes",
                "Makes gups have more accurate visual queues",
                true,
                Extensions.ConfigFlags.RestartRequired);

            helfireChanges = PluginConfigFile.BindOption(section,
                "Enable Lunar Helfire Debuff",
                "Enables Lunar enemies to apply the new Helfire debuff.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            lunarWispChanges = PluginConfigFile.BindOption(section,
                "Enable Lunar Wisp Changes",
                "Increases Lunar Wisp movement speed and acceleration, orb applies helfire",
                true,
                Extensions.ConfigFlags.RestartRequired);

            wispChanges = PluginConfigFile.BindOption(section,
                "Enable Wisp Changes",
                "Makes the wisp attack a fast projectile and increases Wisp bullet count",
                true,
                Extensions.ConfigFlags.RestartRequired);

            greaterWispChanges = PluginConfigFile.BindOption(section,
                "Enable Greater Wisp Changes",
                "Decreases Greater Wisp credit cost",
                true,
                Extensions.ConfigFlags.RestartRequired);

            lemChanges = PluginConfigFile.BindOption(section,
                "Enable Lemurian Bite Changes",
                "Adds slight leap to Lemurian bite",
                true,
                Extensions.ConfigFlags.RestartRequired);

            wormTracking = PluginConfigFile.BindOption(section,
                "Enable Worm Tracking Changes",
                "Changes Worms to have better targeting.",
                true,
                Extensions.ConfigFlags.RestartRequired);

        }
        private static void BindBeetles(string section)
        {

            enableBeetleFamilyChanges = PluginConfigFile.BindOption(section,
                "Enable Beetle Family Changes",
                "Enables all beetle related changes. Yes, they needed their own section. Unaffected by other config sections.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            beetleBurrow = PluginConfigFile.BindOption(section,
                "Enable Lil Beetle Burrow Skill",
                "Adds a new projectile attack to beetles and adds a new mobility skill that allows beetles to burrow into the ground and reappear near the player.",
                true,
                Extensions.ConfigFlags.RestartRequired);
            beetleSpit = PluginConfigFile.BindOption(section,
                "Enable Lil Beetles Spit Skill",
                "Adds a new projectile attack to beetles and adds a new mobility skill that allows beetles to burrow into the ground and reappear near the player.",
                true,
                Extensions.ConfigFlags.RestartRequired);

            bgChanges = PluginConfigFile.BindOption(section,
                "Enable Beetle Guards Rally Cry Skill",
                "Adds a new skill that gives them and nearby allies increased attack speed and movement speed",
                true,
                Extensions.ConfigFlags.RestartRequired);

            queenChanges = PluginConfigFile.BindOption(section,
                "Enable Beetle Queens Debuff",
                "Adds a new debuff, Beetle Juice, to Beetle Queen and ally beetles attacks and makes spit explode mid air",
                true,
                Extensions.ConfigFlags.RestartRequired);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitRoO()
        {
            try
            {
                RiskOfOptions.ModSettingsManager.SetModDescription("Enemies but good.", EnemiesPlusPlugin.PluginGUID, EnemiesPlusPlugin.PluginName);

                var iconStream = System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(EnemiesPlusPlugin.Instance.Info.Location), "icon.png"));
                var tex = new Texture2D(256, 256);
                tex.LoadImage(iconStream);
                var icon = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));

                RiskOfOptions.ModSettingsManager.SetModIcon(icon);
            }
            catch (Exception e)
            {
                Log.Debug(e.ToString());
            }
        }
    }
}
