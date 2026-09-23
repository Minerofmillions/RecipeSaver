using JetBrains.Annotations;
using Terraria;

namespace RecipeSaver;

public class JsonItemStack(Item item)
{
    [UsedImplicitly] public int type = item.type;
    [UsedImplicitly] public int stack = item.stack;
}