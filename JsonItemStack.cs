using System;
using Terraria;

namespace RecipeSaver {
    public class JsonItemStack(Item item)
    {
        public int type = item.type;
        public int stack = item.stack;
    }
}