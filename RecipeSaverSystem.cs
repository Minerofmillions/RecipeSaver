using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeSaver;

internal readonly record struct DataFile(
    [UsedImplicitly] List<JsonMod> CurrentMods,
    [UsedImplicitly] int ExtractinatorTests);

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
            using var sw = new StreamWriter(DataPath);
            Serializer.Serialize(sw, new DataFile(currentMods, RecipeSaverConfig.Instance.extractinatorTests));
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
        var groupsTask = SaveGroups();
        var itemsTask = SaveItemsAndArmor();
        var recipesTask = SaveRecipes();
        var enemiesTask = SaveEnemies();

        Task.WaitAll(groupsTask, itemsTask, recipesTask, enemiesTask);

        return _savedGroups && _savedItems && _savedArmors && _savedRecipes && _savedEnemies;
    }

    private static Task SaveEnemies() =>
        Task.Run(() =>
        {
            if (_savedEnemies) return;
            using var sw = new StreamWriter(EnemiesPath);
            using var writer = new JsonTextWriter(sw);
            writer.WriteStartArray();
            for (var i = -65; i < NPCLoader.NPCCount; i++)
            {
                Serializer.Serialize(writer, new JsonEnemy(i));
            }

            writer.WriteEndArray();
            _savedEnemies = true;
        });

    private static Task SaveRecipes() =>
        Task.Run(() =>
        {
            if (_savedRecipes) return;
            using var sw = new StreamWriter(RecipesPath);
            using var writer = new JsonTextWriter(sw);
            writer.WriteStartArray();
            for (var i = 0; i < Recipe.numRecipes; i++)
            {
                var recipe = Main.recipe[i];
                if (recipe.Disabled) continue;
                Serializer.Serialize(writer, new JsonRecipe(recipe));
            }

            writer.WriteEndArray();
            _savedRecipes = true;
        });

    private static async Task SaveItemsAndArmor()
    {
        await SaveItems();
        await SaveArmors();
    }

    private static Task SaveItems() => Task.Run(() =>
    {
        if (_savedItems) return;
        var item = new Item();
        using var itemStream = new StreamWriter(ItemsPath);
        using var itemWriter = new JsonTextWriter(itemStream);
        itemWriter.WriteStartArray();
        for (var i = 1; i < ItemLoader.ItemCount; i++)
        {
            item.SetDefaults(i);
            if (item.type == ItemID.None) continue;
            JsonItem jsonItem = new(item);
            jsonItem.FindDrops();
            Serializer.Serialize(itemWriter, jsonItem);

            if (item.headSlot != -1) JsonArmor.Heads.Add(i);
            if (item.bodySlot != -1) JsonArmor.Bodies.Add(i);
            if (item.legSlot != -1) JsonArmor.Legs.Add(i);
        }

        itemWriter.WriteEndArray();
        _savedItems = true;
    });

    private static Task SaveArmors() => Task.Run(() =>
    {
        if (_savedArmors) return;
        using var armorStream = new StreamWriter(ArmorSetsPath);
        using var armorWriter = new JsonTextWriter(armorStream);
        armorWriter.WriteStartArray();
        foreach (var armorSet in JsonArmor.GetArmorSetsEnumerator())
        {
            Serializer.Serialize(armorWriter, armorSet);
        }

        armorWriter.WriteEndArray();
        _savedArmors = true;
    });

    private static Task SaveGroups() => Task.Run(() =>
    {
        if (_savedGroups) return;
        using var sw = new StreamWriter(GroupsPath);
        using var writer = new JsonTextWriter(sw);
        writer.WriteStartObject();
        foreach (var (id, group) in RecipeGroup.recipeGroups)
        {
            writer.WritePropertyName(id.ToString());
            Serializer.Serialize(writer, new JsonGroup(group));
        }

        writer.WriteEndObject();
        _savedGroups = true;
    });

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

    private static readonly JsonSerializer Serializer = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Converters = { new JsonLoot.LootConverter() },
        Formatting = Formatting.Indented,
    };

    private static bool ContentsEqualOrderless<TE>(ICollection<TE> one, ICollection<TE> two) =>
        one.All(two.Contains) && two.All(one.Contains);
}