using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver
{
    public class JsonItem
    {
        public int type;
        public string name;
        public int value;
        public int? createTile;
        public int? createWall;
        public string tooltip;
        public readonly JsonLoot bagItems = new();
        public readonly SortedDictionary<int, int> extractinatorItems = [];
        public readonly SortedDictionary<int, int> chlorophyteExtractinatorItems = [];
        public string mod;
        public int bait;
        public int fishingPower;

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

            tooltip = "";
            for (int i = 0; i < item.ToolTip.Lines; i++)
            {
                tooltip += item.ToolTip.GetLine(i) + "\n";
            }
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
            List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(type);
            rules.ForEach(bagItems.AddRule);
        }
        private void FindExtractinatorInfo(int extractinatorBlockType, IDictionary<int, int> extractinatorItems)
        {
            int extractinatorType = ItemID.Sets.ExtractinatorMode[type];
            if (extractinatorType == -1) return;
            FindExtractinatorInfo(extractinatorType, extractinatorBlockType, extractinatorItems);
        }

        private static void SingleExtraction(int extractType, int extractinatorBlockType, out int resultType, out int resultStack)
        {
            int num = 5000;
            int num2 = 25;
            int num3 = 50;
            int num4 = -1;
            int num5 = -1;
            int num6 = -1;
            int num7 = 1;
            switch (extractType)
            {
                case 3347:
                    num /= 3;
                    num2 *= 2;
                    num3 = 20;
                    num4 = 10;
                    break;
                case 2337:
                    num = -1;
                    num2 = -1;
                    num3 = -1;
                    num4 = -1;
                    num5 = 1;
                    num7 = -1;
                    break;
                case 4354:
                    num = -1;
                    num2 = -1;
                    num3 = -1;
                    num4 = -1;
                    num5 = -1;
                    num7 = -1;
                    num6 = 1;
                    break;
            }
            resultType = -1;
            resultStack = 1;
            if (num4 != -1 && Main.rand.Next(num4) == 0)
            {
                resultType = 3380;
                if (Main.rand.Next(5) == 0)
                {
                    resultStack += Main.rand.Next(2);
                }
                if (Main.rand.Next(10) == 0)
                {
                    resultStack += Main.rand.Next(3);
                }
                if (Main.rand.Next(15) == 0)
                {
                    resultStack += Main.rand.Next(4);
                }
            }
            else if (num7 != -1 && Main.rand.Next(2) == 0)
            {
                if (Main.rand.Next(12000) == 0)
                {
                    resultType = 74;
                    if (Main.rand.Next(14) == 0)
                    {
                        resultStack += Main.rand.Next(0, 2);
                    }
                    if (Main.rand.Next(14) == 0)
                    {
                        resultStack += Main.rand.Next(0, 2);
                    }
                    if (Main.rand.Next(14) == 0)
                    {
                        resultStack += Main.rand.Next(0, 2);
                    }
                }
                else if (Main.rand.Next(800) == 0)
                {
                    resultType = 73;
                    if (Main.rand.Next(6) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(6) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(6) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(6) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(6) == 0)
                    {
                        resultStack += Main.rand.Next(1, 20);
                    }
                }
                else if (Main.rand.Next(60) == 0)
                {
                    resultType = 72;
                    if (Main.rand.Next(4) == 0)
                    {
                        resultStack += Main.rand.Next(5, 26);
                    }
                    if (Main.rand.Next(4) == 0)
                    {
                        resultStack += Main.rand.Next(5, 26);
                    }
                    if (Main.rand.Next(4) == 0)
                    {
                        resultStack += Main.rand.Next(5, 26);
                    }
                    if (Main.rand.Next(4) == 0)
                    {
                        resultStack += Main.rand.Next(5, 25);
                    }
                }
                else
                {
                    resultType = 71;
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(10, 26);
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(10, 26);
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(10, 26);
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(10, 25);
                    }
                }
            }
            else if (num != -1 && Main.rand.Next(num) == 0)
            {
                resultType = 1242;
            }
            else if (num5 != -1)
            {
                resultType = ((Main.rand.Next(4) != 1) ? 2674 : ((Main.rand.Next(3) != 1) ? 2006 : ((Main.rand.Next(3) == 1) ? 2675 : 2002)));
            }
            else if (num6 != -1 && extractinatorBlockType == 642)
            {
                resultType = ((Main.rand.Next(10) == 1) ? (Main.rand.Next(5) switch
                {
                    0 => 4354,
                    1 => 4389,
                    2 => 4377,
                    3 => 5127,
                    _ => 4378,
                }) : (Main.rand.Next(5) switch
                {
                    0 => 4349,
                    1 => 4350,
                    2 => 4351,
                    3 => 4352,
                    _ => 4353,
                }));
            }
            else if (num6 != -1)
            {
                resultType = Main.rand.Next(5) switch
                {
                    0 => 4349,
                    1 => 4350,
                    2 => 4351,
                    3 => 4352,
                    _ => 4353,
                };
            }
            else if (num2 != -1 && Main.rand.Next(num2) == 0)
            {
                resultType = Main.rand.Next(6) switch
                {
                    0 => 181,
                    1 => 180,
                    2 => 177,
                    3 => 179,
                    4 => 178,
                    _ => 182,
                };
                if (Main.rand.Next(20) == 0)
                {
                    resultStack += Main.rand.Next(0, 2);
                }
                if (Main.rand.Next(30) == 0)
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.Next(40) == 0)
                {
                    resultStack += Main.rand.Next(0, 4);
                }
                if (Main.rand.Next(50) == 0)
                {
                    resultStack += Main.rand.Next(0, 5);
                }
                if (Main.rand.Next(60) == 0)
                {
                    resultStack += Main.rand.Next(0, 6);
                }
            }
            else if (num3 != -1 && Main.rand.Next(num3) == 0)
            {
                resultType = 999;
                if (Main.rand.Next(20) == 0)
                {
                    resultStack += Main.rand.Next(0, 2);
                }
                if (Main.rand.Next(30) == 0)
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.Next(40) == 0)
                {
                    resultStack += Main.rand.Next(0, 4);
                }
                if (Main.rand.Next(50) == 0)
                {
                    resultStack += Main.rand.Next(0, 5);
                }
                if (Main.rand.Next(60) == 0)
                {
                    resultStack += Main.rand.Next(0, 6);
                }
            }
            else if (Main.rand.Next(3) == 0)
            {
                if (Main.rand.Next(5000) == 0)
                {
                    resultType = 74;
                    if (Main.rand.Next(10) == 0)
                    {
                        resultStack += Main.rand.Next(0, 3);
                    }
                    if (Main.rand.Next(10) == 0)
                    {
                        resultStack += Main.rand.Next(0, 3);
                    }
                    if (Main.rand.Next(10) == 0)
                    {
                        resultStack += Main.rand.Next(0, 3);
                    }
                    if (Main.rand.Next(10) == 0)
                    {
                        resultStack += Main.rand.Next(0, 3);
                    }
                    if (Main.rand.Next(10) == 0)
                    {
                        resultStack += Main.rand.Next(0, 3);
                    }
                }
                else if (Main.rand.Next(400) == 0)
                {
                    resultType = 73;
                    if (Main.rand.Next(5) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(5) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(5) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(5) == 0)
                    {
                        resultStack += Main.rand.Next(1, 21);
                    }
                    if (Main.rand.Next(5) == 0)
                    {
                        resultStack += Main.rand.Next(1, 20);
                    }
                }
                else if (Main.rand.Next(30) == 0)
                {
                    resultType = 72;
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(5, 26);
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(5, 26);
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(5, 26);
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        resultStack += Main.rand.Next(5, 25);
                    }
                }
                else
                {
                    resultType = 71;
                    if (Main.rand.Next(2) == 0)
                    {
                        resultStack += Main.rand.Next(10, 26);
                    }
                    if (Main.rand.Next(2) == 0)
                    {
                        resultStack += Main.rand.Next(10, 26);
                    }
                    if (Main.rand.Next(2) == 0)
                    {
                        resultStack += Main.rand.Next(10, 26);
                    }
                    if (Main.rand.Next(2) == 0)
                    {
                        resultStack += Main.rand.Next(10, 25);
                    }
                }
            }
            else if (extractinatorBlockType == 642)
            {
                resultType = Main.rand.Next(14) switch
                {
                    0 => 12,
                    1 => 11,
                    2 => 14,
                    3 => 13,
                    4 => 699,
                    5 => 700,
                    6 => 701,
                    7 => 702,
                    8 => 364,
                    9 => 1104,
                    10 => 365,
                    11 => 1105,
                    12 => 366,
                    _ => 1106,
                };
                if (Main.rand.Next(20) == 0)
                {
                    resultStack += Main.rand.Next(0, 2);
                }
                if (Main.rand.Next(30) == 0)
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.Next(40) == 0)
                {
                    resultStack += Main.rand.Next(0, 4);
                }
                if (Main.rand.Next(50) == 0)
                {
                    resultStack += Main.rand.Next(0, 5);
                }
                if (Main.rand.Next(60) == 0)
                {
                    resultStack += Main.rand.Next(0, 6);
                }
            }
            else
            {
                resultType = Main.rand.Next(8) switch
                {
                    0 => 12,
                    1 => 11,
                    2 => 14,
                    3 => 13,
                    4 => 699,
                    5 => 700,
                    6 => 701,
                    _ => 702,
                };
                if (Main.rand.Next(20) == 0)
                {
                    resultStack += Main.rand.Next(0, 2);
                }
                if (Main.rand.Next(30) == 0)
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.Next(40) == 0)
                {
                    resultStack += Main.rand.Next(0, 4);
                }
                if (Main.rand.Next(50) == 0)
                {
                    resultStack += Main.rand.Next(0, 5);
                }
                if (Main.rand.Next(60) == 0)
                {
                    resultStack += Main.rand.Next(0, 6);
                }
            }
            ItemLoader.ExtractinatorUse(ref resultType, ref resultStack, extractType, extractinatorBlockType);
        }

        private static void FindExtractinatorInfo(int extractType, int extractinatorBlockType, IDictionary<int, int> extractinatorItems)
        {
            for (int i = 0; i < RecipeSaverConfig.Instance.ExtractinatorTests; i++)
            {
                SingleExtraction(extractType, extractinatorBlockType, out int resultType, out int resultStack);
                if (resultType > 0)
                {
                    if (!extractinatorItems.TryGetValue(resultType, out int value)) value = 0;
                    extractinatorItems[resultType] = value + resultStack;
                }
            }
        }
    }
}