using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver {
    public class JsonItem {
        public int type;
        public string name;
        public int value;
        public int? createTile;
        public int? createWall;
        public string tooltip;
        public readonly JsonLoot bagItems = new();
        public readonly Dictionary<int, int> extractinatorItems = new();
        public string mod;
        public int bait;
        public int fishingPower;

        public JsonItem(Item item) {
            type = item.type;
            name = item.Name;
            value = item.value;
            createTile = item.createTile == -1 ? null : item.createTile;
            createWall = item.createWall == -1 ? null : item.createWall;
            bait = item.bait;
            fishingPower = item.fishingPole;

            mod = item.ModItem?.Mod?.Name ?? "Terraria";

            tooltip = "";
            for (int i = 0; i < item.ToolTip.Lines; i++) {
                tooltip += item.ToolTip.GetLine(i) + "\n";
            }
        }

        public override string ToString() => name;

        public override int GetHashCode() => type.GetHashCode();

        public void FindDrops() {
            OpenBag();
            FindExtractinatorInfo();
        }

        private void OpenBag() {
            List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(type);
            rules.ForEach(rule => bagItems.AddRule(rule));
        }
        private void FindExtractinatorInfo() {
            int extractinatorMode = ItemID.Sets.ExtractinatorMode[type];
            
            int resultType = 0;
            int resultStack = 0;

            int consecutiveEmpty = 0;
            for (int i = 0; i < RecipeSaverConfig.Instance.ExtractinatorTests; i++) {
                ItemLoader.ExtractinatorUse(ref resultType, ref resultStack, type);
                if (resultType == 0) {
                    if (++consecutiveEmpty == 100) {
                        extractinatorItems.Clear();
                        return;
                    } else continue;
                }
                if (!extractinatorItems.TryGetValue(resultType, out int current)) current = 0;
                extractinatorItems[resultType] = current + resultStack;
            }
        }
    }
}