using Terraria;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RecipeSaver;

public class JsonArmor
{
    public static readonly List<Item> Heads = [];
    public static readonly List<Item> Bodies = [];
    public static readonly List<Item> Legs = [];

    public static HashSet<JsonArmor> GetArmorSets()
    {
        Main.player[0] = new Player();
        var player = Main.player[0];
        HashSet<JsonArmor> armorSets = [];
        foreach (var head in Heads)
        {
            SetHead(player, head);
            foreach (var body in Bodies)
            {
                SetBody(player, body);
                foreach (var legs in Legs)
                {
                    SetLegs(player, legs);
                    player.statDefense = Player.DefenseStat.Default;

                    var (fullSetBonus, fullDefenseBonus) = EvaluateArmorSet(player);

                    if (fullSetBonus == "") continue;
                    SetLegs(player, new Item());
                    var (noLegsSetBonus, noLegsDefenseBonus) = EvaluateArmorSet(player);
                    SetLegs(player, legs);

                    SetBody(player, new Item());
                    var (noBodySetBonus, noBodyDefenseBonus) = EvaluateArmorSet(player);
                    SetBody(player, body);

                    SetHead(player, new Item());
                    var (noHeadSetBonus, noHeadDefenseBonus) = EvaluateArmorSet(player);
                    SetHead(player, head);

                    if (noLegsSetBonus != "")
                        armorSets.Add(new JsonArmor(head, body, null, noLegsSetBonus,
                            head.defense + body.defense + noLegsDefenseBonus));
                    else if (noBodySetBonus != "")
                        armorSets.Add(new JsonArmor(head, null, legs, noBodySetBonus,
                            head.defense + legs.defense + noBodyDefenseBonus));
                    else if (noHeadSetBonus != "")
                        armorSets.Add(new JsonArmor(null, body, legs, noHeadSetBonus,
                            body.defense + legs.defense + noHeadDefenseBonus));
                    else
                        armorSets.Add(new JsonArmor(head, body, legs, fullSetBonus,
                            head.defense + body.defense + legs.defense + fullDefenseBonus));
                }
            }
        }

        return armorSets;
    }

    private static void SetHead(Player player, Item head)
    {
        player.head = head.headSlot;
        player.armor[0] = head;
    }

    private static void SetBody(Player player, Item body)
    {
        player.body = body.bodySlot;
        player.armor[1] = body;
    }

    private static void SetLegs(Player player, Item legs)
    {
        player.legs = legs.legSlot;
        player.armor[2] = legs;
    }

    private static Tuple<string, int> EvaluateArmorSet(Player player)
    {
        player.UpdateArmorSets(255);
        var fullSetBonus = player.setBonus;
        int fullDefenceBonus = player.statDefense;

        return new Tuple<string, int>(fullSetBonus, fullDefenceBonus);
    }

    [UsedImplicitly] public readonly JsonItem head;
    [UsedImplicitly] public readonly JsonItem body;
    [UsedImplicitly] public readonly JsonItem legs;

    [UsedImplicitly] public string setBonus;
    [UsedImplicitly] public int setDefense;

    private JsonArmor(Item head, Item body, Item legs, string setBonus, int setDefense)
    {
        if (head is not null) this.head = new JsonItem(head);
        if (body is not null) this.body = new JsonItem(body);
        if (legs is not null) this.legs = new JsonItem(legs);
        this.setBonus = setBonus;
        this.setDefense = setDefense;
    }

    public override bool Equals(object obj)
    {
        if (obj is not JsonArmor armor) return false;
        return armor.head?.type == head?.type && armor.body?.type == body?.type && armor.legs?.type == legs?.type;
    }

    public override int GetHashCode() =>
        (head?.GetHashCode() ?? 0) ^ (body?.GetHashCode() ?? 0) ^ (legs?.GetHashCode() ?? 0);
}