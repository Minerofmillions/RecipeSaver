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
        public readonly Dictionary<int, int> extractinatorItems = [];
        public readonly Dictionary<int, int> chlorophyteExtractinatorItems = [];
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
            FindExtractinatorInfo(TileID.Extractinator, extractinatorItems);
            FindExtractinatorInfo(TileID.ChlorophyteExtractinator, chlorophyteExtractinatorItems);
        }

        private void OpenBag() {
            List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(type);
            rules.ForEach(bagItems.AddRule);
        }
        private void FindExtractinatorInfo(int extractinatorBlockType, Dictionary<int, int> extractinatorItems) {
            int extractinatorType = ItemID.Sets.ExtractinatorMode[type];
            if (extractinatorType == -1) return;
            FindExtractinatorInfo(extractinatorType, extractinatorBlockType, extractinatorItems);
        }

        private static void FindExtractinatorInfo(int type, int extractinatorBlockType, Dictionary<int, int> extractinatorItems) {
            int resultType = 0;
            int resultStack = 0;

            int consecutiveEmpty = 0;
            for (int i = 0; i < RecipeSaverConfig.Instance.ExtractinatorTests; i++) {
                ItemLoader.ExtractinatorUse(ref resultType, ref resultStack, type, extractinatorBlockType);
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