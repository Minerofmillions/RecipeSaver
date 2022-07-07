using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace RecipeSaver {
    public class JsonMod {
        public string name;
        public string version;

        public JsonMod(Mod mod) {
            name = mod?.Name ?? "Terraria";
            version = mod?.Version.ToString() ?? "0.0";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            if (obj is not JsonMod mod) return false;
            return name == mod.name && version == mod.version;
        }

        public override int GetHashCode() => name.GetHashCode() + 31 * version.GetHashCode();
    }
}
