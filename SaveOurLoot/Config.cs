using BepInEx.Configuration;

namespace SaveOurLoot
{
    public class Config
    {
        public static ConfigEntry<float> saveAllChance;
        public static ConfigEntry<float> saveEachChance;

        public static ConfigEntry<bool> valueSaveEnabled;
        public static ConfigEntry<float> valueSavePercent;

        public static void Load()
        {
            saveAllChance = Plugin.config.Bind<float>("LootSaving", "SaveAllChance", 0.25f, "A chance of all item being saved.\nVanilla value 0. Values between 0-1.");
            saveEachChance = Plugin.config.Bind<float>("LootSaving", "SaveEachChance", 0.5f, "A chance of each item being saved.\nApplied after SaveAllChance\nVanilla value 0. Values between 0-1.");

            valueSaveEnabled = Plugin.config.Bind<bool>("LootSaving", "ValueSaveEnabled", false, "Will it try to save item based on it scrap value.\nApplied after SaveAllChance and prevent SaveEachChance\nVanilla value False.");
            valueSavePercent = Plugin.config.Bind<float>("LootSaving", "ValueSavePercent", 0.25f, "What percentage of total scrap value will be saved among loot.\nVanilla value 0. Values between 0-1.");
        }
    }
}
