using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        var startTime = Stopwatch.GetTimestamp();
        var tries = 0;
        while (tries++ < 5)
        {
            if (!TrySavingData()) continue;
            using var sw = new StreamWriter(DataPath);
            Serializer.Serialize(sw, new DataFile(currentMods, RecipeSaverConfig.Instance.extractinatorTests));
            break;
        }

        var elapsed = Stopwatch.GetElapsedTime(startTime);
        Mod.Logger.Info($"Saved data in: {elapsed:mm\\:ss.fff}");
        if (tries != 5) return;
        Mod.Logger.Info("Couldn't save data.");
        File.Delete(GroupsPath);
        File.Delete(ItemsPath);
        File.Delete(RecipesPath);
        File.Delete(EnemiesPath);
        File.Delete(ArmorSetsPath);
        File.Delete(DataPath);
    }

    private static bool _savedGroups;
    private static bool _savedItems;
    private static bool _savedRecipes;
    private static bool _savedEnemies;
    private static bool _savedArmors;

    private static bool TrySavingData()
    {
        var armorsTask = SaveArmors();
        var groupsTask = SaveGroups();
        var itemsTask = SaveItems();
        var recipesTask = SaveRecipes();
        var enemiesTask = SaveEnemies();

        Task.WaitAll(armorsTask, groupsTask, itemsTask, recipesTask, enemiesTask);
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

    private static Task SaveItems() => Task.Run(async () =>
    {
        if (_savedItems) return;
        var item = new Item();
        await using var itemStream = new StreamWriter(ItemsPath);
        await using var itemWriter = new JsonTextWriter(itemStream);
        await itemWriter.WriteStartArrayAsync();
        List<Task> armorTasks = [];
        for (var i = 1; i < ItemLoader.ItemCount; i++)
        {
            item.SetDefaults(i);
            if (item.type == ItemID.None) continue;
            JsonItem jsonItem = new(item);
            jsonItem.FindDrops();
            Serializer.Serialize(itemWriter, jsonItem);

            if (item.headSlot != -1) armorTasks.Add(JsonArmor.AddHead(i));
            if (item.bodySlot != -1) armorTasks.Add(JsonArmor.AddBody(i));
            if (item.legSlot != -1) armorTasks.Add(JsonArmor.AddLegs(i));
        }

        foreach (var task in armorTasks) await task;
        await itemWriter.WriteEndArrayAsync();
        JsonArmor.FinishChannel();
        _savedItems = true;
    });

    private static Task SaveArmors() => Task.Run(async () =>
    {
        if (_savedArmors) return;
        await using var armorStream = new StreamWriter(ArmorSetsPath);
        await using var armorWriter = new JsonTextWriter(armorStream);
        await armorWriter.WriteStartArrayAsync();
        await foreach (var armorSet in JsonArmor.GetArmorSetsAsync())
        {
            Serializer.Serialize(armorWriter, armorSet);
        }

        await armorWriter.WriteEndArrayAsync();
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