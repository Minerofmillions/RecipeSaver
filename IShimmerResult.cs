using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria;

namespace RecipeSaver;

public interface IShimmerResult
{
    [UsedImplicitly] public string Type { get; }
}

public readonly struct NoShimmerResult : IShimmerResult
{
    public string Type => "None";
}

public readonly struct CoinLuckShimmerResult(int luckValue) : IShimmerResult
{
    public string Type => "CoinLuck";
    [UsedImplicitly] public readonly int luckValue = luckValue;
}

public readonly struct EntitySpawnShimmerResult(int npcResult) : IShimmerResult
{
    public string Type => "NPCSpawn";
    [UsedImplicitly] public readonly int npcResult = npcResult;
}

public readonly struct TransmutedItemShimmerResult(int resultItem) : IShimmerResult
{
    public string Type => "TransmutedItem";
    [UsedImplicitly] public readonly int resultItem  = resultItem;
}

public readonly struct DecraftShimmerResult(int decraftingRecipeIndex, List<string> conditions) : IShimmerResult
{
    public string Type => "Decraft";
    [UsedImplicitly] public readonly int decraftingRecipeIndex = decraftingRecipeIndex;
    [UsedImplicitly] public readonly List<string> conditions = conditions;
}