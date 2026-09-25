using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver;

public static class ExtractinatorInfo
{
    private static void SingleExtraction(int extractType, int extractinatorBlockType, out int resultType, out int resultStack)
    {
        var num = 5000;
        var num2 = 25;
        var num3 = 50;
        var num4 = -1;
        var num5 = -1;
        var num6 = -1;
        var num7 = 1;
        switch (extractType)
        {
            case ItemID.DesertFossil:
                num /= 3;
                num2 *= 2;
                num3 = 20;
                num4 = 10;
                break;
            case ItemID.OldShoe:
                num = -1;
                num2 = -1;
                num3 = -1;
                num4 = -1;
                num5 = 1;
                num7 = -1;
                break;
            case ItemID.LavaMoss:
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
        if (num4 != -1 && Main.rand.NextBool(num4))
        {
            resultType = ItemID.FossilOre;
            if (Main.rand.NextBool(5))
            {
                resultStack += Main.rand.Next(2);
            }
            if (Main.rand.NextBool(10))
            {
                resultStack += Main.rand.Next(3);
            }
            if (Main.rand.NextBool(15))
            {
                resultStack += Main.rand.Next(4);
            }
        }
        else if (num7 != -1 && Main.rand.NextBool(2))
        {
            if (Main.rand.NextBool(12000))
            {
                resultType = ItemID.PlatinumCoin;
                if (Main.rand.NextBool(14))
                {
                    resultStack += Main.rand.Next(0, 2);
                }
                if (Main.rand.NextBool(14))
                {
                    resultStack += Main.rand.Next(0, 2);
                }
                if (Main.rand.NextBool(14))
                {
                    resultStack += Main.rand.Next(0, 2);
                }
            }
            else if (Main.rand.NextBool(800))
            {
                resultType = ItemID.GoldCoin;
                if (Main.rand.NextBool(6))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(6))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(6))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(6))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(6))
                {
                    resultStack += Main.rand.Next(1, 20);
                }
            }
            else if (Main.rand.NextBool(60))
            {
                resultType = ItemID.SilverCoin;
                if (Main.rand.NextBool(4))
                {
                    resultStack += Main.rand.Next(5, 26);
                }
                if (Main.rand.NextBool(4))
                {
                    resultStack += Main.rand.Next(5, 26);
                }
                if (Main.rand.NextBool(4))
                {
                    resultStack += Main.rand.Next(5, 26);
                }
                if (Main.rand.NextBool(4))
                {
                    resultStack += Main.rand.Next(5, 25);
                }
            }
            else
            {
                resultType = ItemID.CopperCoin;
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(10, 26);
                }
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(10, 26);
                }
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(10, 26);
                }
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(10, 25);
                }
            }
        }
        else if (num != -1 && Main.rand.NextBool(num))
        {
            resultType = ItemID.AmberMosquito;
        }
        else if (num5 != -1)
        {
            resultType = Main.rand.NextBool(4) ? ItemID.ApprenticeBait : Main.rand.NextBool(3) ? ItemID.Snail : Main.rand.NextBool(3) ? ItemID.JourneymanBait : ItemID.Worm;
        }
        else if (num6 != -1 && extractinatorBlockType == 642)
        {
            resultType = Main.rand.NextBool(10) ? Main.rand.Next(5) switch
            {
                0 => ItemID.LavaMoss,
                1 => ItemID.ArgonMoss,
                2 => ItemID.KryptonMoss,
                3 => ItemID.VioletMoss,
                _ => ItemID.XenonMoss,
            } : Main.rand.Next(5) switch
            {
                0 => ItemID.GreenMoss,
                1 => ItemID.BrownMoss,
                2 => ItemID.RedMoss,
                3 => ItemID.BlueMoss,
                _ => ItemID.PurpleMoss,
            };
        }
        else if (num6 != -1)
        {
            resultType = Main.rand.Next(5) switch
            {
                0 => ItemID.GreenMoss,
                1 => ItemID.BrownMoss,
                2 => ItemID.RedMoss,
                3 => ItemID.BlueMoss,
                _ => ItemID.PurpleMoss,
            };
        }
        else if (num2 != -1 && Main.rand.Next(num2) == 0)
        {
            resultType = Main.rand.Next(6) switch
            {
                0 => ItemID.Amethyst,
                1 => ItemID.Topaz,
                2 => ItemID.Sapphire,
                3 => ItemID.Emerald,
                4 => ItemID.Ruby,
                _ => ItemID.Diamond,
            };
            if (Main.rand.NextBool(20))
            {
                resultStack += Main.rand.Next(0, 2);
            }
            if (Main.rand.NextBool(30))
            {
                resultStack += Main.rand.Next(0, 3);
            }
            if (Main.rand.NextBool(40))
            {
                resultStack += Main.rand.Next(0, 4);
            }
            if (Main.rand.NextBool(50))
            {
                resultStack += Main.rand.Next(0, 5);
            }
            if (Main.rand.NextBool(60))
            {
                resultStack += Main.rand.Next(0, 6);
            }
        }
        else if (num3 != -1 && Main.rand.Next(num3) == 0)
        {
            resultType = ItemID.Amber;
            if (Main.rand.NextBool(20))
            {
                resultStack += Main.rand.Next(0, 2);
            }
            if (Main.rand.NextBool(30))
            {
                resultStack += Main.rand.Next(0, 3);
            }
            if (Main.rand.NextBool(40))
            {
                resultStack += Main.rand.Next(0, 4);
            }
            if (Main.rand.NextBool(50))
            {
                resultStack += Main.rand.Next(0, 5);
            }
            if (Main.rand.NextBool(60))
            {
                resultStack += Main.rand.Next(0, 6);
            }
        }
        else if (Main.rand.NextBool(3))
        {
            if (Main.rand.NextBool(5000))
            {
                resultType = ItemID.PlatinumCoin;
                if (Main.rand.NextBool(10))
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.NextBool(10))
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.NextBool(10))
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.NextBool(10))
                {
                    resultStack += Main.rand.Next(0, 3);
                }
                if (Main.rand.NextBool(10))
                {
                    resultStack += Main.rand.Next(0, 3);
                }
            }
            else if (Main.rand.NextBool(400))
            {
                resultType = ItemID.GoldCoin;
                if (Main.rand.NextBool(5))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(5))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(5))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(5))
                {
                    resultStack += Main.rand.Next(1, 21);
                }
                if (Main.rand.NextBool(5))
                {
                    resultStack += Main.rand.Next(1, 20);
                }
            }
            else if (Main.rand.NextBool(30))
            {
                resultType = ItemID.SilverCoin;
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(5, 26);
                }
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(5, 26);
                }
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(5, 26);
                }
                if (Main.rand.NextBool(3))
                {
                    resultStack += Main.rand.Next(5, 25);
                }
            }
            else
            {
                resultType = ItemID.CopperCoin;
                if (Main.rand.NextBool(2))
                {
                    resultStack += Main.rand.Next(10, 26);
                }
                if (Main.rand.NextBool(2))
                {
                    resultStack += Main.rand.Next(10, 26);
                }
                if (Main.rand.NextBool(2))
                {
                    resultStack += Main.rand.Next(10, 26);
                }
                if (Main.rand.NextBool(2))
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
            if (Main.rand.NextBool(20))
            {
                resultStack += Main.rand.Next(0, 2);
            }
            if (Main.rand.NextBool(30))
            {
                resultStack += Main.rand.Next(0, 3);
            }
            if (Main.rand.NextBool(40))
            {
                resultStack += Main.rand.Next(0, 4);
            }
            if (Main.rand.NextBool(50))
            {
                resultStack += Main.rand.Next(0, 5);
            }
            if (Main.rand.NextBool(60))
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
            if (Main.rand.NextBool(20))
            {
                resultStack += Main.rand.Next(0, 2);
            }
            if (Main.rand.NextBool(30))
            {
                resultStack += Main.rand.Next(0, 3);
            }
            if (Main.rand.NextBool(40))
            {
                resultStack += Main.rand.Next(0, 4);
            }
            if (Main.rand.NextBool(50))
            {
                resultStack += Main.rand.Next(0, 5);
            }
            if (Main.rand.NextBool(60))
            {
                resultStack += Main.rand.Next(0, 6);
            }
        }
        ItemLoader.ExtractinatorUse(ref resultType, ref resultStack, extractType, extractinatorBlockType);
    }

    public static void FindExtractinatorInfo(int extractType, int extractinatorBlockType, IDictionary<int, int> outputItems)
    {
        var failedTests = 0;
        for (var i = 0; i < RecipeSaverConfig.Instance.extractinatorTests; i++)
        {
            SingleExtraction(extractType, extractinatorBlockType, out var resultType, out var resultStack);
            if (resultType <= 0)
            {
                if (++failedTests >= 100)
                {
                    outputItems.Clear();
                    break;
                }
                continue;
            }
            if (!outputItems.TryGetValue(resultType, out var value)) value = 0;
            outputItems[resultType] = value + resultStack;
        }
    }
}