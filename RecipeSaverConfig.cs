using System.ComponentModel;
using JetBrains.Annotations;
using Terraria.ModLoader.Config;

namespace RecipeSaver;

public class RecipeSaverConfig : ModConfig
{
    [UsedImplicitly] public static RecipeSaverConfig Instance;

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(10000)] [ReloadRequired] [UsedImplicitly]
    public int extractinatorTests;
}