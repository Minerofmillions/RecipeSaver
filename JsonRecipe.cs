using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria;
using Terraria.Localization;

namespace RecipeSaver;

public class JsonRecipe
{
    [UsedImplicitly] public JsonItemStack createItem;
    [UsedImplicitly] public List<LocalizedText> conditions;
    [UsedImplicitly] public List<JsonItemStack> requiredItems = [];
    [UsedImplicitly] public List<int> requiredTiles;
    [UsedImplicitly] public string mod;
    [UsedImplicitly] public List<int> acceptedGroups;

    public JsonRecipe(Recipe recipe)
    {
        createItem = new JsonItemStack(recipe.createItem);

        mod = recipe.Mod?.Name ?? "Terraria";

        conditions = [];
        foreach (var condition in recipe.Conditions)
        {
            conditions.Add(condition.Description);
        }

        foreach (var item in recipe.requiredItem)
        {
            requiredItems.Add(new JsonItemStack(item));
        }

        requiredTiles = recipe.requiredTile;
        requiredTiles.Sort();

        acceptedGroups = recipe.acceptedGroups;
        acceptedGroups.Sort();
    }
}