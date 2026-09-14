using System.Collections.Generic;
using Terraria;
using Terraria.Localization;

namespace RecipeSaver
{
    public class JsonRecipe
    {
        public JsonItemStack createItem;
        public List<LocalizedText> conditions;
        public List<JsonItemStack> requiredItems = [];
        public List<int> requiredTiles;
        public string mod;
        public List<int> acceptedGroups;

        public JsonRecipe(Recipe recipe)
        {
            createItem = new(recipe.createItem);

            mod = recipe.Mod?.Name ?? "Terraria";

            conditions = [];
            foreach (var condition in recipe.Conditions)
            {
                conditions.Add(condition.Description);
            }

            foreach (Item item in recipe.requiredItem)
            {
                requiredItems.Add(new(item));
            }

            requiredTiles = recipe.requiredTile;
            requiredTiles.Sort();

            acceptedGroups = recipe.acceptedGroups;
            acceptedGroups.Sort();
        }
    }
}