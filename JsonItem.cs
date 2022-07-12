using System;
using System.Collections.Generic;
using Terraria;
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
        public readonly Dictionary<int, int> bagItems = new();
        public string mod;

        public JsonItem(Item item) {
            type = item.type;
            name = item.Name;
            value = item.value;
            createTile = item.createTile == -1 ? null : item.createTile;
            createWall = item.createWall == -1 ? null : item.createWall;

            mod = item.ModItem?.Mod?.Name ?? "Terraria";

            tooltip = "";
            for (int i = 0; i < item.ToolTip.Lines; i++) {
                tooltip += item.ToolTip.GetLine(i) + "\n";
            }
        }

        public override string ToString() => name;

        public override int GetHashCode() => type.GetHashCode();
    }
}