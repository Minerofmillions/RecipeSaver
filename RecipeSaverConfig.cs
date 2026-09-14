using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace RecipeSaver
{
    public class RecipeSaverConfig : ModConfig
    {
        public static RecipeSaverConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(10000)]
        [ReloadRequired]
        public int ExtractinatorTests;
    }
}
