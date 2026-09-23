using Terraria;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RecipeSaver;

public class JsonGroup(RecipeGroup group)
{
    [UsedImplicitly] public readonly SortedSet<int> validItems = [..group.ValidItems];
    [UsedImplicitly] public readonly int iconicItem = group.IconicItemId;
}