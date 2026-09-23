using JetBrains.Annotations;
using Terraria.ModLoader;

namespace RecipeSaver;

public class JsonMod(Mod mod)
{
    [UsedImplicitly] public readonly string name = mod?.Name ?? "Terraria";
    [UsedImplicitly] public readonly string version = mod?.Version?.ToString() ?? "0.0";
    [UsedImplicitly] public readonly string displayName = mod?.DisplayName ?? "Terraria";

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not JsonMod mod) return false;
        return name == mod.name && version == mod.version;
    }

    public override int GetHashCode() => name.GetHashCode() + 31 * version.GetHashCode();
}