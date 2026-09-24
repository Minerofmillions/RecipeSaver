using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver;

public class JsonItem
{
    [UsedImplicitly] public readonly int type;
    [UsedImplicitly] public readonly string name;
    [UsedImplicitly] public readonly int value;
    [UsedImplicitly] public readonly int? createTile;
    [UsedImplicitly] public readonly int? createWall;
    [UsedImplicitly] public readonly string tooltip;
    [UsedImplicitly] public readonly JsonLoot bagItems = new();
    [UsedImplicitly] public readonly SortedDictionary<int, int> extractinatorItems = [];
    [UsedImplicitly] public readonly SortedDictionary<int, int> chlorophyteExtractinatorItems = [];
    [UsedImplicitly] public readonly string mod;
    [UsedImplicitly] public readonly int bait;
    [UsedImplicitly] public readonly int fishingPower;

    public JsonItem(Item item)
    {
        type = item.type;
        name = item.Name;
        value = item.value;
        createTile = item.createTile == -1 ? null : item.createTile;
        createWall = item.createWall == -1 ? null : item.createWall;
        bait = item.bait;
        fishingPower = item.fishingPole;

        mod = item.ModItem?.Mod?.Name ?? "Terraria";

        tooltip = item.ToolTip.Lines > 0 ? Enumerable.Range(0, item.ToolTip.Lines).Select(item.ToolTip.GetLine)
            .Aggregate((a, b) => a + "\n" + b) : "";
    }

    public override string ToString() => name;

    public override int GetHashCode() => type.GetHashCode();

    public void FindDrops()
    {
        OpenBag();
        FindExtractinatorInfo(TileID.Extractinator, extractinatorItems);
        FindExtractinatorInfo(TileID.ChlorophyteExtractinator, chlorophyteExtractinatorItems);
    }

    private void OpenBag()
    {
        var rules = Main.ItemDropsDB.GetRulesForItemID(type);
        rules.ForEach(bagItems.AddRule);
    }
    private void FindExtractinatorInfo(int extractinatorBlockType, IDictionary<int, int> outputItems)
    {
        var extractinatorType = ItemID.Sets.ExtractinatorMode[type];
        if (extractinatorType == -1) return;
        ExtractinatorInfo.FindExtractinatorInfo(extractinatorType, extractinatorBlockType, outputItems);
    }
}