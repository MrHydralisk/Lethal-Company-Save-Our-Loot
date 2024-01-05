using BepInEx.Configuration;

namespace SaveOurLoot
{
    public class Config
    {
        public static ConfigEntry<float> saveEachChance;
        public static ConfigEntry<float> saveAllChance;

        public static void Load()
        {
            saveEachChance = Plugin.config.Bind<float>("LootSaving", "SaveEachChance", 0.5f, "A chance of each item being saved.\nApplied after SaveAllChance\nVanilla value 0. Values between 0-1.");
            saveAllChance = Plugin.config.Bind<float>("LootSaving", "SaveAllChance", 0.25f, "A chance of all item being saved.\nVanilla value 0. Values between 0-1.");
        }
    }
}
