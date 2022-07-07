using System;
using Terraria;

namespace RecipeSaver {
    public class JsonItemStack {
        public int type;
        public int stack;

        public JsonItemStack(Item item) {
            type = item.type;
            stack = item.stack;
        }
    }
}