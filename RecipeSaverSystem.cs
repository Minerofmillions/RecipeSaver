using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver;

internal readonly record struct DataFile(List<JsonMod> CurrentMods, int ExtractinatorTests);

public class RecipeSaverSystem : ModSystem
{
    private static readonly string SaverPath = Path.Combine(Main.SavePath, "Saver");
    private static readonly string ItemsPath = Path.Combine(SaverPath, "Items.json");
    private static readonly string RecipesPath = Path.Combine(SaverPath, "Recipes.json");
    private static readonly string GroupsPath = Path.Combine(SaverPath, "Groups.json");
    private static readonly string EnemiesPath = Path.Combine(SaverPath, "Enemies.json");
    private static readonly string ArmorSetsPath = Path.Combine(SaverPath, "ArmorSets.json");
    private static readonly string DataPath = Path.Combine(SaverPath, "_Data.json");

    public override void PostAddRecipes()
    {
        List<JsonMod> currentMods =
        [
            .. from mod in ModLoader.Mods where mod is not null select new JsonMod(mod)
        ];

        bool needsRecalculate;

#if DEBUG
        needsRecalculate = true;
#else
        if (File.Exists(DataPath))
        {
            try
            {
                var oldConfig = JsonConvert.DeserializeObject<DataFile>(File.ReadAllText(DataPath));
                needsRecalculate = !ContentsEqualOrderless(currentMods, oldConfig.CurrentMods) ||
                                   RecipeSaverConfig.Instance.extractinatorTests != oldConfig.ExtractinatorTests;
            }
            catch (Exception)
            {
                needsRecalculate = true;
            }
        }
        else needsRecalculate = true;
#endif

        if (!needsRecalculate) return;
        var tries = 0;
        while (tries++ < 5)
        {
            if (!TrySavingData()) continue;
            Serialize(DataPath, new DataFile(currentMods, RecipeSaverConfig.Instance.extractinatorTests));
            break;
        }

        if (tries == 5)
        {
            Mod.Logger.Info("Couldn't save data.");
        }
    }

    private static bool _savedGroups;
    private static bool _savedItems;
    private static bool _savedRecipes;
    private static bool _savedEnemies;
    private static bool _savedArmors;

    private static bool TrySavingData()
    {
        if (!_savedGroups)
            try
            {
                SaveGroups();
                _savedGroups = true;
            }
            catch (Exception)
            {
            }

        if (!_savedItems)
            try
            {
                SaveItems();
                _savedItems = true;
            }
            catch (Exception)
            {
            }

        if (_savedItems && !_savedArmors)
            try
            {
                var armorSets = JsonArmor.GetArmorSets();
                Serialize(ArmorSetsPath, armorSets);
                _savedArmors = true;
            }
            catch (Exception)
            {
            }

        if (!_savedRecipes)
            try
            {
                SaveRecipes();
                _savedRecipes = true;
            }
            catch (Exception)
            {
            }

        if (!_savedEnemies)
            try
            {
                SaveEnemies();
                _savedEnemies = true;
            }
            catch (Exception)
            {
            }

        return _savedGroups && _savedItems && _savedArmors && _savedRecipes && _savedEnemies;
    }

    private static void SaveEnemies()
    {
        List<JsonEnemy> enemies = [];
        for (var i = -65; i < NPCLoader.NPCCount; i++)
        {
            enemies.Add(new JsonEnemy(i));
        }

        Serialize(EnemiesPath, enemies);
    }

    private static void SaveRecipes()
    {
        List<JsonRecipe> recipes = [];
        for (var i = 0; i < Recipe.numRecipes; i++)
        {
            var recipe = Main.recipe[i];
            if (recipe.Disabled) continue;
            recipes.Add(new JsonRecipe(recipe));
        }

        Serialize(RecipesPath, recipes);
    }

    private static void SaveItems()
    {
        List<JsonItem> items = [];
        for (var i = 1; i < ItemLoader.ItemCount; i++)
        {
            Item item = new(i);
            if (item.type == ItemID.None) continue;
            JsonItem jsonItem = new(item);
            jsonItem.FindDrops();
            items.Add(jsonItem);

            if (item.headSlot != -1) JsonArmor.Heads.Add(item);
            if (item.bodySlot != -1) JsonArmor.Bodies.Add(item);
            if (item.legSlot != -1) JsonArmor.Legs.Add(item);
        }

        Serialize(ItemsPath, items);
    }

    private static void SaveGroups()
    {
        Dictionary<int, JsonGroup> groups = [];
        foreach (var (id, group) in RecipeGroup.recipeGroups)
        {
            groups.Add(id, new JsonGroup(group));
        }

        Serialize(GroupsPath, groups);
    }

    static RecipeSaverSystem()
    {
        Directory.CreateDirectory(SaverPath);
    }

    private static readonly JsonSerializerSettings Settings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Converters = [new JsonLoot.LootConverter()],
    };

    private static void Serialize(string path, object value)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented, Settings));
    }

    private static bool ContentsEqualOrderless<TE>(ICollection<TE> one, ICollection<TE> two) =>
        one.All(two.Contains) && two.All(one.Contains);
}