using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terraria;
using Terraria.Localization;

namespace RecipeSaver;

public class JsonRecipe
{
    [UsedImplicitly] public JsonItemStack createItem;
    [UsedImplicitly] public List<string> conditions;
    [UsedImplicitly] public List<JsonItemStack> requiredItems;
    [UsedImplicitly] public List<int> requiredTiles;
    [UsedImplicitly] public string mod;
    [UsedImplicitly] public List<int> acceptedGroups;

    public JsonRecipe(Recipe recipe)
    {
        createItem = new JsonItemStack(recipe.createItem);

        mod = recipe.Mod?.Name ?? "Terraria";

        conditions = [.. from condition in recipe.Conditions select condition.Description.Value];

        requiredItems = [.. from item in recipe.requiredItem select new JsonItemStack(item)];

        requiredTiles = recipe.requiredTile;
        requiredTiles.Sort();

        acceptedGroups = recipe.acceptedGroups;
        acceptedGroups.Sort();
    }
}