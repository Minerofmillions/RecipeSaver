using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace RecipeSaver
{
    internal class JsonEnemy
    {
        public string name;
        public int type;
        public string mod;
        public readonly JsonLoot drops = new();

        public int banner;
        public int killsPerBanner;

        internal static readonly HashSet<Type> ruleTypes = [];

        public JsonEnemy(int npcID)
        {
            NPC npc = new();
            npc.SetDefaults(npcID);

            name = npc.TypeName;
            type = npc.type;
            mod = npc.ModNPC?.Mod?.Name ?? "Terraria";

            List<IItemDropRule> itemDropRules = Main.ItemDropsDB.GetRulesForNPCID(npcID);
            itemDropRules.ForEach(drops.AddRule);

            int bannerId = Item.NPCtoBanner(npcID);
            banner = bannerId > 0 ? Item.BannerToItem(bannerId) : 0;
            killsPerBanner = banner > 0 ? ItemID.Sets.KillsToBanner[banner] : 0;
        }
    }
}
