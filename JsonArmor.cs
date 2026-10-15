using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RecipeSaver;

public class JsonArmor
{
    private static readonly HashSet<int> Heads = [];
    private static readonly HashSet<int> Bodies = [];
    private static readonly HashSet<int> Legs = [];
    private static readonly List<Tuple<int, int>> ShortSets = [];

    private static readonly Item HeadItem = new();
    private static readonly Item BodyItem = new();
    private static readonly Item LegsItem = new();
    private static readonly Player Player = new();

    private static int TotalDefense => (HeadItem.active ? HeadItem.defense : 0) + (BodyItem.active ? BodyItem.defense : 0) +
                                       (LegsItem.active ? LegsItem.defense : 0) + Player.statDefense;

    static JsonArmor()
    {
        Main.player[0] = Player;
        Player.armor[0] = HeadItem;
        Player.armor[1] = BodyItem;
        Player.armor[2] = LegsItem;
    }

    private static readonly Channel<JsonArmor> ArmorChannel = Channel.CreateUnbounded<JsonArmor>();

    public static IAsyncEnumerable<JsonArmor> GetArmorSetsAsync() => ArmorChannel.Reader.ReadAllAsync();

    public static void FinishChannel() => ArmorChannel.Writer.Complete();

    public static async Task AddHead(int head)
    {
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

        Heads.Add(head);
    }

    public static async Task AddBody(int body)
    {
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
        Bodies.Add(body);
    }

    public static async Task AddLegs(int legs)
    {
        foreach (var head in Heads)
        {
            foreach (var body in Bodies.Where(body => !ShortSets.Any(tup => head == tup.Item1 && body == tup.Item2)))
            {
                SetArmor(head, body, legs);
                if (Player.setBonus == "") continue;
                await ArmorChannel.Writer.WriteAsync(new JsonArmor(head, body, legs, Player.setBonus, TotalDefense));
            }
        }
        Legs.Add(legs);
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

    private static void SetHead(int head)
    {
        HeadItem.SetDefaults(head);
        Player.head = HeadItem.headSlot;
        Player.UpdateArmorSets(0);
    }

    private static void SetBody(int body)
    {
        BodyItem.SetDefaults(body);
        Player.body = BodyItem.bodySlot;
        Player.UpdateArmorSets(0);
    }

    private static void SetLegs(int legs)
    {
        LegsItem.SetDefaults(legs);
        Player.legs = LegsItem.legSlot;
        Player.UpdateArmorSets(0);
    }

    private static Tuple<string, Player.DefenseStat> EvaluateArmorSet()
    {
        Player.UpdateArmorSets(255);
        var fullSetBonus = Player.setBonus;
        var fullDefenceBonus = Player.statDefense;

        return new Tuple<string, Player.DefenseStat>(fullSetBonus, fullDefenceBonus);
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
}