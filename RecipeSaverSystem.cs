using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RecipeSaver {
    readonly record struct DataFile {
        public DataFile(List<JsonMod> currentMods, int extractinatorTests) {
            CurrentMods = currentMods;
            ExtractinatorTests = extractinatorTests;
        }
        public List<JsonMod> CurrentMods { get; init; }
        public int ExtractinatorTests { get; init; }
    }

    public class RecipeSaverSystem : ModSystem {
        private static readonly string SaverPath = Path.Combine(Main.SavePath, "Saver");
        private static readonly string ItemsPath = Path.Combine(SaverPath, "Items.json");
        private static readonly string RecipesPath = Path.Combine(SaverPath, "Recipes.json");
        private static readonly string GroupsPath = Path.Combine(SaverPath, "Groups.json");
        private static readonly string EnemiesPath = Path.Combine(SaverPath, "Enemies.json");
        private static readonly string ArmorSetsPath = Path.Combine(SaverPath, "ArmorSets.json");
        private static readonly string DataPath = Path.Combine(SaverPath, "_Data.json");

        private static readonly Player player = new();

        public override void PostAddRecipes() {
            Main.player[1] = player;
            List<JsonMod> currentMods = new();
            foreach (Mod mod in ModLoader.Mods) {
                if (mod is not null) currentMods.Add(new(mod));
            }
            bool needsRecalculate;

            if (File.Exists(DataPath)) {
                DataFile oldConfig;
                try {
                    oldConfig = JsonConvert.DeserializeObject<DataFile>(File.ReadAllText(DataPath));
                    needsRecalculate = !ContentsEqualOrderless(currentMods, oldConfig.CurrentMods) ||
                        RecipeSaverConfig.Instance.ExtractinatorTests != oldConfig.ExtractinatorTests;
                } catch (Exception) {
                    needsRecalculate = true;
                }
            } else needsRecalculate = true;

            if (needsRecalculate) {
                int tries = 0;
                while (tries++ < 5) {
                    if (TrySavingData()) {
                        Serialize(DataPath, new DataFile(currentMods, RecipeSaverConfig.Instance.ExtractinatorTests));
                        break;
                    }
                }
                if (tries == 5) {
                    Mod.Logger.Info("Couldn't save data.");
                }
            }
        }

        private static bool savedGroups = false;
        private static bool savedItems = false;
        private static bool savedRecipes = false;
        private static bool savedNPCs = false;
        private static bool savedArmors = false;

        private static bool TrySavingData() {
            if (!savedGroups) try {
                    SaveGroups();
                    savedGroups = true;
                } catch (Exception) { }

            if (!savedItems) try {
                    SaveItems();
                    savedItems = true;
                } catch (Exception) { }

            if (savedItems && !savedArmors) try {
                    HashSet<JsonArmor> armorSets = JsonArmor.GetArmorSets();
                    Serialize(ArmorSetsPath, armorSets);
                    savedArmors = true;
                } catch (Exception) { }

            if (!savedRecipes) try {
                    SaveRecipes();
                    savedRecipes = true;
                } catch (Exception) { }

            if (!savedNPCs) try {
                    SaveNPCs();
                    savedNPCs = true;
                } catch (Exception) { }

            return savedGroups && savedItems && savedArmors && savedRecipes && savedNPCs;
        }

        private static void SaveNPCs() {
            List<JsonEnemy> enemies = new();
            for (int i = -65; i < NPCLoader.NPCCount; i++) {
                enemies.Add(new(i));
            }
            Serialize(EnemiesPath, enemies);
        }

        private static void SaveRecipes() {
            List<JsonRecipe> recipes = new();
            for (int i = 0; i < Recipe.numRecipes; i++) {
                Recipe recipe = Main.recipe[i];
                if (recipe.Disabled) continue;
                recipes.Add(new(recipe));
            }
            Serialize(RecipesPath, recipes);
        }

        private static void SaveItems() {
            List<JsonItem> items = new();
            for (int i = 1; i < ItemLoader.ItemCount; i++) {
                Item item = new(i);
                if (item.type != ItemID.None) {
                    JsonItem jsonItem = new(item);
                    jsonItem.FindDrops();
                    items.Add(jsonItem);

                    if (item.headSlot != -1) JsonArmor.Heads.Add(item);
                    if (item.bodySlot != -1) JsonArmor.Bodies.Add(item);
                    if (item.legSlot != -1) JsonArmor.Legs.Add(item);
                }
            }
            Serialize(ItemsPath, items);
        }

        private static void SaveGroups() {
            Dictionary<int, JsonGroup> groups = new();
            foreach (var (id, group) in RecipeGroup.recipeGroups) {
                groups.Add(id, new(group));
            }
            Serialize(GroupsPath, groups);
        }

        static RecipeSaverSystem() {
            Directory.CreateDirectory(SaverPath);
        }

        private static void Serialize(string path, object value) {
            File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private static bool ContentsEqualOrderless<E>(IEnumerable<E> one, IEnumerable<E> two) => one.All(two.Contains) && two.All(one.Contains);

    }
}
