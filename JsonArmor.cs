using Terraria;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RecipeSaver;

public class JsonArmor
{
    public static readonly HashSet<int> Heads = [];
    public static readonly HashSet<int> Bodies = [];
    public static readonly HashSet<int> Legs = [];

    private static readonly Item HeadItem = new();
    private static readonly Item BodyItem = new();
    private static readonly Item LegsItem = new();

    public static IEnumerable<JsonArmor> GetArmorSetsEnumerator()
    {
        var player = new Player
        {
            armor = [HeadItem, BodyItem, LegsItem]
        };
        Main.player[0] = player;
        foreach (var head in Heads)
        {
            SetHead(player, head);
            var (headSetBonus, headDefenseBonus) = EvaluateArmorSet(player);
            if (headSetBonus != "")
            {
                yield return new JsonArmor(head, 0, 0, headSetBonus, HeadItem.defense + headDefenseBonus);
                continue;
            }
            foreach (var body in Bodies)
            {
                SetBody(player, body);
                var (noLegsSetBonus, noLegsDefenseBonus) = EvaluateArmorSet(player);
                if (noLegsSetBonus != "")
                {
                    yield return new JsonArmor(head, body, 0, noLegsSetBonus, HeadItem.defense + BodyItem.defense + noLegsDefenseBonus);
                    continue;
                }
                foreach (var legs in Legs)
                {
                    SetLegs(player, legs);
                    player.statDefense = Player.DefenseStat.Default;

                    var (fullSetBonus, fullDefenseBonus) = EvaluateArmorSet(player);

                    if (fullSetBonus == "") continue;

                    SetBody(player, 0);
                    var (noBodySetBonus, noBodyDefenseBonus) = EvaluateArmorSet(player);
                    SetBody(player, body);

                    SetHead(player, 0);
                    var (noHeadSetBonus, noHeadDefenseBonus) = EvaluateArmorSet(player);
                    SetHead(player, head);

                    if (noBodySetBonus != "")
                        yield return new JsonArmor(head, 0, legs, noBodySetBonus,
                            HeadItem.defense + LegsItem.defense + noBodyDefenseBonus);
                    else if (noHeadSetBonus != "")
                        yield return new JsonArmor(0, body, legs, noHeadSetBonus,
                            BodyItem.defense + LegsItem.defense + noHeadDefenseBonus);
                    else
                        yield return new JsonArmor(head, body, legs, fullSetBonus,
                            HeadItem.defense + BodyItem.defense + LegsItem.defense + fullDefenseBonus);
                }
            }
        }
    }

    private static void SetHead(Player player, int head)
    {
        HeadItem.SetDefaults(head);
        player.head = HeadItem.headSlot;
    }

    private static void SetBody(Player player, int body)
    {
        BodyItem.SetDefaults(body);
        player.body = BodyItem.bodySlot;
    }

    private static void SetLegs(Player player, int legs)
    {
        LegsItem.SetDefaults(legs);
        player.legs = LegsItem.legSlot;
    }

    private static Tuple<string, Player.DefenseStat> EvaluateArmorSet(Player player)
    {
        player.UpdateArmorSets(255);
        var fullSetBonus = player.setBonus;
        var fullDefenceBonus = player.statDefense;

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