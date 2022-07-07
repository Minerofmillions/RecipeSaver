using System;
using System.Collections.Generic;
using Terraria;

namespace RecipeSaver {
    public class JsonRecipe {
        public JsonItemStack createItem;
        public List<string> conditions;
        public List<JsonItemStack> requiredItems = new();
        public List<int> requiredTiles;
        public string mod;
        public List<int> acceptedGroups;

        public JsonRecipe(Recipe recipe) {
            createItem = new(recipe.createItem);

            mod = recipe.Mod?.Name ?? "Terraria";

            conditions = new();
            foreach (Recipe.Condition condition in recipe.Conditions) {
                conditions.Add(condition.Description);
            }

            foreach (Item item in recipe.requiredItem) {
                requiredItems.Add(new(item));
            }

            requiredTiles = recipe.requiredTile;
            requiredTiles.Sort();

            acceptedGroups = recipe.acceptedGroups;
            acceptedGroups.Sort();
        }
    }
}