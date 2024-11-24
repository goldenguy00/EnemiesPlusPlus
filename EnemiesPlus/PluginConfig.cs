using System.Runtime.CompilerServices;
using BepInEx.Configuration;

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

        private static ConfigFile PluginConfigFile { get; set; }

        public static void Init(ConfigFile cfg)
        {
            PluginConfigFile = cfg;

            var section = "Skills";
            enableSkills = PluginConfigFile.BindOption(section,
                "Enable New Skills",
                true,
                "Allows any of the skills within this section to be toggled on and off.", true);

            bellSkills = PluginConfigFile.BindOption(section,
                "Enable Brass Contraptions Buff Beam Skill",
                true,
                "Adds a new skill that gives an ally increased armor.", true);

            impSkills = PluginConfigFile.BindOption(section,
                "Enable Imps Void Spike Skill",
                true,
                "Adds a new skill for Imps to throw void spikes at range, similarly to the Imp OverLord.", true);

            lunarGolemSkills = PluginConfigFile.BindOption(section,
                "Enable Lunar Golems Lunar Shell Skill",
                true,
                "Adds a new ability that gives them some stuff idk its vanilla but unused", true);

            wormTracking = PluginConfigFile.BindOption(section,
                "Enable Worm Tracking Changes",
                true,
                "Changes Worms to have better targeting.", true);
            wormLeap = PluginConfigFile.BindOption(section,
                "Enable Worms Leap Skill",
                true,
                "Adds a new leap skill.", true);


            section = "Tweaks";
            enableTweaks = PluginConfigFile.BindOption(section,
                "Enable Enemy Tweaks",
                true,
                "Allows any of the skills within this section to be toggled on and off.", true);

            helfireChanges = PluginConfigFile.BindOption(section,
                "Enable Lunar Helfire Debuff",
                true,
                "Enables Lunar enemies to apply the new Helfire debuff.", true);

            lunarWispChanges = PluginConfigFile.BindOption(section,
                "Enable Lunar Wisp Changes",
                true,
                "Increases Lunar Wisp movement speed and acceleration, orb applies helfire", true);

            wispChanges = PluginConfigFile.BindOption(section,
                "Enable Wisp Changes",
                true,
                "Makes the wisp attack a fast projectile and increases Wisp bullet count", true);

            greaterWispChanges = PluginConfigFile.BindOption(section,
                "Enable Greater Wisp Changes",
                true,
                "Decreases Greater Wisp credit cost", true);

            lemChanges = PluginConfigFile.BindOption(section,
                "Enable Lemurian Bite Changes",
                true,
                "Adds slight leap to Lemurian bite", true);


            section = "Beetles";
            enableBeetleFamilyChanges = PluginConfigFile.BindOption(section,
                "Enable Beetle Family Changes",
                true,
                "Enables all beetle related changes. Yes, they needed their own section. Unaffected by other config sections.", true);

            beetleBurrow = PluginConfigFile.BindOption(section,
                "Enable Lil Beetle Burrow Skill",
                true,
                "Adds a new projectile attack to beetles and adds a new mobility skill that allows beetles to burrow into the ground and reappear near the player.", true);
            beetleSpit = PluginConfigFile.BindOption(section,
                "Enable Lil Beetles Spit Skill",
                true,
                "Adds a new projectile attack to beetles and adds a new mobility skill that allows beetles to burrow into the ground and reappear near the player.", true);

            bgChanges = PluginConfigFile.BindOption(section,
                "Enable Beetle Guards Rally Cry Skill",
                true,
                "Adds a new skill that gives them and nearby allies increased attack speed and movement speed", true);

            queenChanges = PluginConfigFile.BindOption(section,
                "Enable Beetle Queens Debuff",
                true,
                "Adds a new debuff, Beetle Juice, to Beetle Queen and ally beetles attacks and makes spit explode mid air", true);
        }

        #region Config Binding
        public static ConfigEntry<T> BindOption<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, description);

            if (EnemiesPlusPlugin.RooInstalled)
                TryRegisterOption(configEntry, restartRequired);

            return configEntry;
        }

        public static ConfigEntry<T> BindOptionSlider<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", float min = 0, float max = 20, bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += " (Default: " + defaultValue + ")";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, description);

            if (EnemiesPlusPlugin.RooInstalled)
                TryRegisterOptionSlider(configEntry, min, max, restartRequired);

            return configEntry;
        }
        #endregion

        #region RoO
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOption<T>(ConfigEntry<T> entry, bool restartRequired)
        {
            if (entry is ConfigEntry<string> stringEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(stringEntry, restartRequired));
                return;
            }
            if (entry is ConfigEntry<float> floatEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(floatEntry, new RiskOfOptions.OptionConfigs.SliderConfig()
                {
                    min = 0,
                    max = 20,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }));
                return;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.IntSliderOption(intEntry, restartRequired));
                return;
            }
            if (entry is ConfigEntry<bool> boolEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(boolEntry, restartRequired));
                return;
            }
            if (entry is ConfigEntry<KeyboardShortcut> shortCutEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(shortCutEntry, restartRequired));
                return;
            }
            if (typeof(T).IsEnum)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(entry, restartRequired));
                return;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOptionSlider<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<int> intEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.IntSliderOption(intEntry, new RiskOfOptions.OptionConfigs.IntSliderConfig()
                {
                    min = (int)min,
                    max = (int)max,
                    formatString = "{0:0.00}",
                    restartRequired = restartRequired
                }));
                return;
            }

            if (entry is ConfigEntry<float> floatEntry)
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(floatEntry, new RiskOfOptions.OptionConfigs.SliderConfig()
                {
                    min = min,
                    max = max,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }));
        }
        #endregion
    }
}
