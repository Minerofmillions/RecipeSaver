using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace RecipeSaver {
    public class RecipeSaverConfig : ModConfig {
        public static RecipeSaverConfig Instance;
        
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(10000)]
        [ReloadRequired]
        public int ExtractinatorTests;
    }
}
