using Terraria;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RecipeSaver;

public class JsonArmor
{
    private static readonly ConcurrentBag<int> Heads = [];
    private static readonly ConcurrentBag<int> Bodies = [];
    private static readonly ConcurrentBag<int> Legs = [];
    private static readonly ConcurrentBag<Tuple<int, int>> ShortSets = [];

    private static readonly Player Player = new();
    private static readonly Item HeadItem = new();
    private static readonly Item BodyItem = new();
    private static readonly Item LegsItem = new();

    static JsonArmor()
    {
        Main.player[0] = Player;
        Player.armor[0] = HeadItem;
        Player.armor[1] = BodyItem;
        Player.armor[2] = LegsItem;
    }

    private static int TotalDefense =>
        (HeadItem.active ? HeadItem.defense : 0) + (BodyItem.active ? BodyItem.defense : 0) +
        (LegsItem.active ? LegsItem.defense : 0) + Player.statDefense;

    private static readonly Channel<JsonArmor> ArmorChannel = Channel.CreateUnbounded<JsonArmor>();
    private static readonly Channel<ArmorItem> InputChannel = Channel.CreateUnbounded<ArmorItem>();

    private static readonly Task InputChannelReader = Task.Run(async () =>
    {
        await foreach (var armorItem in InputChannel.Reader.ReadAllAsync())
        {
            switch (armorItem.itemType)
            {
                case 0:
                {
                    var head = armorItem.itemId;
                    Heads.Add(head);
                    foreach (var body in Bodies)
                    {
                        SetArmor(head, body, 0);
                        if (Player.setBonus != "")
                        {
                            await ArmorChannel.Writer.WriteAsync(new JsonArmor(head, body, 0, Player.setBonus, TotalDefense));
                            ShortSets.Add(new Tuple<int, int>(head, body));
                            continue;
                        }

                        foreach (var legs in Legs)
                        {
                            SetLegs(legs);
                            if (Player.setBonus == "") continue;
                            await ArmorChannel.Writer.WriteAsync(new JsonArmor(head, body, legs, Player.setBonus, TotalDefense));
                        }
                    }

                    break;
                }
                case 1:
                {
                    var body = armorItem.itemId;
                    Bodies.Add(body);
                    foreach (var head in Heads)
                    {
                        SetArmor(head, body, 0);
                        if (Player.setBonus != "")
                        {
                            await ArmorChannel.Writer.WriteAsync(new JsonArmor(head, body, 0, Player.setBonus, TotalDefense));
                            ShortSets.Add(new Tuple<int, int>(head, body));
                            continue;
                        }

                        foreach (var legs in Legs)
                        {
                            SetLegs(legs);
                            if (Player.setBonus == "") continue;
                            await ArmorChannel.Writer.WriteAsync(new JsonArmor(head, body, legs, Player.setBonus, TotalDefense));
                        }
                    }

                    break;
                }
                default:
                {
                    var legs = armorItem.itemId;
                    Legs.Add(legs);
                    foreach (var head in Heads)
                    {
                        foreach (var body in Bodies.Where(body => !ShortSets.Any(tup => head == tup.Item1 && body == tup.Item2)))
                        {
                            SetArmor(head, body, legs);
                            if (Player.setBonus == "") continue;
                            await ArmorChannel.Writer.WriteAsync(new JsonArmor(head, body, legs, Player.setBonus, TotalDefense));
                        }
                    }

                    break;
                }
            }
        }
    });

    public static IAsyncEnumerable<JsonArmor> GetArmorSetsAsync() => ArmorChannel.Reader.ReadAllAsync();

    public static void FinishChannel()
    {
        InputChannel.Writer.Complete();
        InputChannelReader.Wait();
        ArmorChannel.Writer.Complete();
    }

    public static void AddHead(int head)
    {
        InputChannel.Writer.TryWrite(new ArmorItem(head, 0));
    }

    public static void AddBody(int body)
    {
        InputChannel.Writer.TryWrite(new ArmorItem(body, 1));
    }

    public static void AddLegs(int legs)
    {
        InputChannel.Writer.TryWrite(new ArmorItem(legs, 2));
    }

    private static void SetArmor(int head, int body, int legs)
    {
        HeadItem.SetDefaults(head);
        Player.head = HeadItem.headSlot;
        BodyItem.SetDefaults(body);
        Player.body = BodyItem.bodySlot;
        LegsItem.SetDefaults(legs);
        Player.legs = LegsItem.legSlot;
        Player.UpdateArmorSets(0);
    }

    private static void SetLegs(int legs)
    {
        LegsItem.SetDefaults(legs);
        Player.legs = LegsItem.legSlot;
        Player.UpdateArmorSets(0);
    }

    [UsedImplicitly] public readonly int head;
    [UsedImplicitly] public readonly int body;
    [UsedImplicitly] public readonly int legs;

    [UsedImplicitly] public string setBonus;
    [UsedImplicitly] public int setDefense;

    private JsonArmor(int head, int body, int legs, string setBonus, int setDefense)
    {
        this.head = head;
        this.body = body;
        this.legs = legs;
        this.setBonus = setBonus;
        this.setDefense = setDefense;
    }

    public override bool Equals(object obj)
    {
        if (obj is not JsonArmor armor) return false;
        return armor.head == head && armor.body == body && armor.legs == legs;
    }

    public override int GetHashCode() =>
        head.GetHashCode() ^ body.GetHashCode() ^ legs.GetHashCode();

    public class ArmorItem(int itemId, int itemType)
    {
        public readonly int itemId = itemId;
        public readonly int itemType = itemType;
    }
}