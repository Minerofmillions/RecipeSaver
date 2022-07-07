using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Newtonsoft.Json;
using Terraria.Utilities;
using Terraria.ID;
using System.Text.Json.Serialization;
using System.Linq;

namespace RecipeSaver {
    public class RecipeSaver : Mod {
        private static readonly string SaverPath = Path.Combine(Main.SavePath, "Saver");
        private static readonly string ItemsPath = Path.Combine(SaverPath, "Items.json");
        private static readonly string RecipesPath = Path.Combine(SaverPath, "Recipes.json");
        private static readonly string GroupsPath = Path.Combine(SaverPath, "Groups.json");
        private static readonly string EnemiesPath = Path.Combine(SaverPath, "Enemies.json");
        private static readonly string ArmorSetsPath = Path.Combine(SaverPath, "ArmorSets.json");
        private static readonly string DataPath = Path.Combine(SaverPath, "_Data.json");

        static RecipeSaver() {
            Directory.CreateDirectory(SaverPath);
        }
        public override void PostAddRecipes() {
            List<JsonMod> currentMods = new();
            foreach (Mod mod in ModLoader.Mods) {
                if (mod is not null) currentMods.Add(new(mod));
            }
            bool needsRecalculate;

            if (File.Exists(DataPath)) {
                List<JsonMod> oldMods = JsonConvert.DeserializeObject<List<JsonMod>>(File.ReadAllText(DataPath));
                needsRecalculate = !ContentsEqualOrderless(currentMods, oldMods);
            } else needsRecalculate = true;

            //if (needsRecalculate) {
                Logger.Info("Starting recipe saving");
                Logger.InfoFormat("Saving {0} groups", RecipeGroup.recipeGroups.Count);
                Dictionary<int, JsonGroup> groups = new();
                foreach (var (id, group) in RecipeGroup.recipeGroups) {
                    groups.Add(id, new(group));
                }
                Serialize(GroupsPath, groups);
                Logger.Info("Saved groups");

                Logger.InfoFormat("Saving {0} items", ItemLoader.ItemCount);
                List<JsonItem> items = new();
                for (int i = 1; i < ItemLoader.ItemCount; i++) {
                    Item item = new();
                    item.SetDefaults(i);
                    items.Add(new(item));

                    if (item.headSlot != -1) JsonArmor.Heads.Add(item);
                    if (item.bodySlot != -1) JsonArmor.Bodies.Add(item);
                    if (item.legSlot != -1) JsonArmor.Legs.Add(item);
                }
                Serialize(ItemsPath, items);
                Logger.Info("Saved items");

                Logger.InfoFormat("Generating at most {0} armor sets ({1} heads, {2} bodies, {3} legs)", JsonArmor.Heads.Count * JsonArmor.Bodies.Count * JsonArmor.Legs.Count, JsonArmor.Heads.Count, JsonArmor.Bodies.Count, JsonArmor.Legs.Count);
                HashSet<JsonArmor> armorSets = JsonArmor.GetArmorSets();
                Serialize(ArmorSetsPath, armorSets);
                Logger.InfoFormat("Saved {0} armor sets", armorSets.Count);

                Logger.InfoFormat("Saving {0} recipes", Recipe.numRecipes);
                List<JsonRecipe> recipes = new();
                for (int i = 0; i < Recipe.numRecipes; i++) {
                    Recipe recipe = Main.recipe[i];
                    recipes.Add(new(recipe));
                }
                Serialize(RecipesPath, recipes);
                Logger.Info("Saved recipes");

                Logger.InfoFormat("Saving {0} enemies", NPCLoader.NPCCount);
                List<JsonEnemy> enemies = new();
                for (int i = 0; i < NPCLoader.NPCCount; i++) {
                    NPC npc = new();
                    npc.SetDefaults(i);
                    enemies.Add(new(npc));
                }
                Serialize(EnemiesPath, enemies);
                Logger.Info("Saved enemies");

                Serialize(DataPath, currentMods);
            //}
        }

        private static void Serialize(string path, object value) {
            File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private static bool ContentsEqualOrderless<E>(IEnumerable<E> one, IEnumerable<E> two) => one.All(two.Contains) && two.All(one.Contains);
    }
}