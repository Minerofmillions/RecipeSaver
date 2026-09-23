using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace RecipeSaver;

internal class JsonEnemy
{
    [UsedImplicitly] public string name;
    [UsedImplicitly] public int type;
    [UsedImplicitly] public string mod;
    [UsedImplicitly] public readonly JsonLoot drops = new();

    [UsedImplicitly] public int banner;
    [UsedImplicitly] public int killsPerBanner;

    public JsonEnemy(int npcID)
    {
        NPC npc = new();
        npc.SetDefaults(npcID);

        name = npc.TypeName;
        type = npc.type;
        mod = npc.ModNPC?.Mod?.Name ?? "Terraria";

        var itemDropRules = Main.ItemDropsDB.GetRulesForNPCID(npcID);
        itemDropRules.ForEach(drops.AddRule);

        var bannerId = Item.NPCtoBanner(npcID);
        banner = bannerId > 0 ? Item.BannerToItem(bannerId) : 0;
        killsPerBanner = banner > 0 ? ItemID.Sets.KillsToBanner[banner] : 0;
    }
}