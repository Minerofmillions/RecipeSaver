using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using Mono.Cecil.Cil;

namespace RecipeSaver {
    public class JsonArmor {
        public static readonly List<Item> Heads = new();
        public static readonly List<Item> Bodies = new();
        public static readonly List<Item> Legs = new();

        public static HashSet<JsonArmor> GetArmorSets() {
            Main.player[0] = new();
            Player player = Main.player[0];
            HashSet<JsonArmor> armorSets = new();
            foreach (Item head in Heads) {
                SetHead(player, head);
                foreach (Item body in Bodies) {
                    SetBody(player, body);
                    foreach (Item legs in Legs) {
                        SetLegs(player, legs);
                        player.statDefense = 0;

                        var (fullSetBonus, fullDefenseBonus) = EvaluateArmorSet(player);

                        if (fullSetBonus != "") {
                            SetLegs(player, new());
                            var (noLegsSetBonus, noLegsDefenseBonus) = EvaluateArmorSet(player);
                            SetLegs(player, legs);

                            SetBody(player, new());
                            var (noBodySetBonus, noBodyDefenseBonus) = EvaluateArmorSet(player);
                            SetBody(player, body);

                            SetHead(player, new());
                            var (noHeadSetBonus, noHeadDefenseBonus) = EvaluateArmorSet(player);
                            SetHead(player, head);

                            if (noLegsSetBonus != "")
                                armorSets.Add(new(head, body, null, noLegsSetBonus, head.defense + body.defense + noLegsDefenseBonus));
                            else if (noBodySetBonus != "")
                                armorSets.Add(new(head, null, legs, noBodySetBonus, head.defense + legs.defense + noBodyDefenseBonus));
                            else if (noHeadSetBonus != "")
                                armorSets.Add(new(null, body, legs, noHeadSetBonus, body.defense + legs.defense + noHeadDefenseBonus));
                            else
                                armorSets.Add(new(head, body, legs, fullSetBonus, head.defense + body.defense + legs.defense + fullDefenseBonus));
                        }
                    }
                }
            }
            return armorSets;
        }

        private static void SetHead(Player player, Item head) {
            player.head = head.headSlot;
            player.armor[0] = head;
        }

        private static void SetBody(Player player, Item body) {
            player.body = body.bodySlot;
            player.armor[1] = body;
        }

        private static void SetLegs(Player player, Item legs) {
            player.legs = legs.legSlot;
            player.armor[2] = legs;
        }

        private static Tuple<string, int> EvaluateArmorSet(Player player) {
            player.UpdateArmorSets(255);
            string fullSetBonus = player.setBonus;
            int fullDefenceBonus = player.statDefense;

            return new(fullSetBonus, fullDefenceBonus);
        }

        public JsonItem head;
        public JsonItem body;
        public JsonItem legs;

        public string setBonus;
        public int setDefense;

        private JsonArmor(Item head, Item body, Item legs, string setBonus, int setDefense) {
            if (head is not null) this.head = new(head);
            if (body is not null) this.body = new(body);
            if (legs is not null) this.legs = new(legs);
            this.setBonus = setBonus;
            this.setDefense = setDefense;
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (obj is not JsonArmor armor) return false;
            return armor.head?.type == head?.type && armor.body?.type == body?.type && armor.legs?.type == legs?.type;
        }

        public override int GetHashCode() => (head?.GetHashCode() ?? 0) ^ (body?.GetHashCode() ?? 0) ^ (legs?.GetHashCode() ?? 0);
    }
}
