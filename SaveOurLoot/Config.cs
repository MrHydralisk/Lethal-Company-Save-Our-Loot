using BepInEx.Configuration;

namespace SaveOurLoot
{
    public class Config
    {
        public static ConfigEntry<float> saveAllChance;
        public static ConfigEntry<float> saveEachChance;
        public static ConfigEntry<int> scrapLossMax;

        public static ConfigEntry<bool> valueSaveEnabled;
        public static ConfigEntry<float> valueSavePercent;

        public static ConfigEntry<bool> equipmentLossEnabled;
        public static ConfigEntry<float> equipmentLossChance;
        public static ConfigEntry<int> equipmentLossMax;

        public static ConfigEntry<bool> hoardingBugInfestationEnabled;
        public static ConfigEntry<float> hoardingBugInfestationChance;
        public static ConfigEntry<float> hoardingBugInfestationLossEachChance;
        public static ConfigEntry<int> hoardingBugInfestationLossMax;
        public static ConfigEntry<bool> hoardingBugInfestationValueLossEnabled;
        public static ConfigEntry<float> hoardingBugInfestationValueLossPercent;
        public static ConfigEntry<bool> hoardingBugInfestationEquipmentLossEnabled;
        public static ConfigEntry<float> hoardingBugInfestationEquipmentLossChance;
        public static ConfigEntry<int> hoardingBugInfestationEquipmentLossMax;

        public static void Load()
        {
            saveAllChance = Plugin.config.Bind<float>("LootSaving", "SaveAllChance", 0.25f, "A chance of all item being saved.\nVanilla value 0. Values between 0-1.");
            saveEachChance = Plugin.config.Bind<float>("LootSaving", "SaveEachChance", 0.5f, "A chance of each item being saved.\nApplied after SaveAllChance\nVanilla value 0. Values between 0-1.");
            scrapLossMax = Plugin.config.Bind<int>("LootSaving", "ScrapLossMax", int.MaxValue, $"The maximum amount of items that can be lost.\nApplied after SaveEachChance\nVanilla value {int.MaxValue}. Values between 0-{int.MaxValue}.");

            valueSaveEnabled = Plugin.config.Bind<bool>("LootSaving", "ValueSaveEnabled", false, "Will it try to save item based on it scrap value?\nApplied after SaveAllChance and prevent SaveEachChance\nVanilla value False.");
            valueSavePercent = Plugin.config.Bind<float>("LootSaving", "ValueSavePercent", 0.25f, "What percentage of total scrap value will be saved among loot.\nVanilla value 0. Values between 0-1.");

            equipmentLossEnabled = Plugin.config.Bind<bool>("EquipmentLoss", "EquipmentLossEnabled", false, "Will it allow equipment to be lost?\nVanilla value False.");
            equipmentLossChance = Plugin.config.Bind<float>("EquipmentLoss", "EquipmentLossChance", 0.1f, "A chance of each equipment being lost.\nApplied after SaveAllChance\nVanilla value 0. Values between 0-1.");
            equipmentLossMax = Plugin.config.Bind<int>("EquipmentLoss", "EquipmentLossMax", int.MaxValue, $"The maximum amount of equipment that can be lost.\nApplied after EquipmentLossChance\nVanilla value 0. Values between 0-{int.MaxValue}.");

            hoardingBugInfestationEnabled = Plugin.config.Bind<bool>("HoardingBugInfestation", "HoardingBugInfestationEnabled", false, "Space is a dangerous place. Bug Mafia will protect you for a little part of your loot.\nYour Ship is infested with Hoarding Bugs, which steal your loot while you sleep between missions.\nEnable all features related to this category?\nVanilla value False.");
            hoardingBugInfestationChance = Plugin.config.Bind<float>("HoardingBugInfestation", "HoardingBugInfestationChance", 1f, "A chance of items being stolen by Hoarding Bugs each night on the Ship.\nValues between 0-1.");
            hoardingBugInfestationLossEachChance = Plugin.config.Bind<float>("HoardingBugInfestation", "HoardingBugInfestationLossEachChance", 0.1f, "A chance of each item being stolen by Hoarding Bugs.\nValues between 0-1.");
            hoardingBugInfestationLossMax = Plugin.config.Bind<int>("HoardingBugInfestation", "HoardingBugInfestationLossMax", int.MaxValue, $"The maximum amount of items that can be lost.\nApplied after HoardingBugInfestationLossEachChance\nVanilla value {int.MaxValue}. Values between 0-{int.MaxValue}.");
            hoardingBugInfestationValueLossEnabled = Plugin.config.Bind<bool>("HoardingBugInfestation", "HoardingBugInfestationValueLossEnabled", true, "Will it try to steal items based on it scrap value?\nPrevent HoardingBugInfestationLossEachChance\nVanilla value False.");
            hoardingBugInfestationValueLossPercent = Plugin.config.Bind<float>("HoardingBugInfestation", "HoardingBugInfestationValueLossPercent", 0.1f, "What percentage of total scrap value will be stolen among loot.\nValues between 0-1.");
            hoardingBugInfestationEquipmentLossEnabled = Plugin.config.Bind<bool>("HoardingBugInfestation", "HoardingBugInfestationEquipmentLossEnabled", true, "Will it allow stealing of equipment?");
            hoardingBugInfestationEquipmentLossChance = Plugin.config.Bind<float>("HoardingBugInfestation", "HoardingBugInfestationEquipmentLossChance", 0.05f, "A chance of each equipment being stollen.\nValues between 0-1.");
            hoardingBugInfestationEquipmentLossMax = Plugin.config.Bind<int>("HoardingBugInfestation", "HoardingBugInfestationEquipmentLossMax", int.MaxValue, $"The maximum amount of equipment that can be stollen.\nApplied after EquipmentLossChance\nValues between 0-{int.MaxValue}.");
        }
    }
}
