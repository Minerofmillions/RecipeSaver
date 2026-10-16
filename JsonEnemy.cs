using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver;

internal class JsonEnemy
{
    private static readonly NPC Npc = new();
    [UsedImplicitly] public string name;
    [UsedImplicitly] public int type;
    [UsedImplicitly] public string mod;
    [UsedImplicitly] public readonly JsonLoot drops = new();
    [UsedImplicitly] public readonly SortedDictionary<int, List<List<string>>> shop = new();
    [UsedImplicitly] public int banner;
    [UsedImplicitly] public int killsPerBanner;

    public JsonEnemy(int npcId)
    {
        Npc.SetDefaults(npcId);

        name = Npc.TypeName;
        type = Npc.type;
        mod = Npc.ModNPC?.Mod?.Name ?? "Terraria";

        var itemDropRules = Main.ItemDropsDB.GetRulesForNPCID(npcId);
        itemDropRules.ForEach(drops.AddRule);

        var bannerId = Item.NPCtoBanner(npcId);
        banner = bannerId > 0 ? Item.BannerToItem(bannerId) : 0;
        killsPerBanner = banner > 0 ? ItemID.Sets.KillsToBanner[banner] : 0;

        if (!NPCShopDatabase.TryGetNPCShop(NPCShopDatabase.GetShopName(npcId), out var npcShop)) return;

        foreach (var entry in npcShop.ActiveEntries)
        {
            if (!shop.TryGetValue(entry.Item.type, out var conditionCollection))
            {
                conditionCollection = [];
                shop[entry.Item.type] = conditionCollection;
            }

            conditionCollection.Add([.. from condition in entry.Conditions select condition.Description.Value]);
        }
    }
}