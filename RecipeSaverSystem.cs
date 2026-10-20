using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Channels;
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

#if !DEBUG
        bool needsRecalculate;

        if (File.Exists(DataPath))
        {
            try
            {
                var oldConfig = JsonConvert.DeserializeObject<DataFile>(File.ReadAllText(DataPath));
                needsRecalculate = !ContentsEqualOrderless(currentMods, oldConfig.CurrentMods) ||
                                   RecipeSaverConfig.Instance.extractinatorTests != oldConfig.ExtractinatorTests;
            }
            catch (System.Exception)
            {
                needsRecalculate = true;
            }
        }
        else needsRecalculate = true;

        if (!needsRecalculate) return;
#endif

        var startTime = Stopwatch.GetTimestamp();
        var tries = 0;
        while (tries++ < 5)
        {
            var saveTask = TrySavingData();
            saveTask.Wait();
            if (!saveTask.Result) continue;
            using var sw = new StreamWriter(DataPath);
            Serializer.Serialize(sw, new DataFile(currentMods, RecipeSaverConfig.Instance.extractinatorTests));
            break;
        }

        var elapsed = Stopwatch.GetElapsedTime(startTime);
        Mod.Logger.Info($"Saved data in: {elapsed}");
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

    private async Task<bool> TrySavingData()
    {
        var armorsTask = SaveArmors();
        var groupsTask = SaveGroups();
        var itemsTask = SaveItems();
        var recipesTask = SaveRecipes();
        var enemiesTask = SaveEnemies();

        await groupsTask;
        await itemsTask;
        await armorsTask;
        await recipesTask;
        await enemiesTask;
        return _savedGroups && _savedItems && _savedArmors && _savedRecipes && _savedEnemies;
    }

    private async Task SaveEnemies()
    {
        if (_savedEnemies) return;
        await using var sw = new StreamWriter(EnemiesPath);
        await using var writer = new JsonTextWriter(sw);
        await writer.WriteStartArrayAsync();
        for (var i = -65; i < NPCLoader.NPCCount; i++)
        {
            Serializer.Serialize(writer, new JsonEnemy(i));
        }

        await writer.WriteEndArrayAsync();
        _savedEnemies = true;
    }

    private async Task SaveRecipes()
    {
        if (_savedRecipes) return;
        await using var sw = new StreamWriter(RecipesPath);
        await using var writer = new JsonTextWriter(sw);
        await writer.WriteStartArrayAsync();
        for (var i = 0; i < Recipe.numRecipes; i++)
        {
            var recipe = Main.recipe[i];
            if (recipe.Disabled) continue;
            Serializer.Serialize(writer, new JsonRecipe(recipe));
        }

        await writer.WriteEndArrayAsync();
        _savedRecipes = true;
    }

    private async Task SaveItems()
    {
        Mod.Logger.Info($"Saving {ItemLoader.ItemCount} items.");
        if (_savedItems) return;
        var itemChannel = Channel.CreateUnbounded<JsonItem>();
        var itemWriterTask = Task.Run(async () =>
        {
            await using var itemStream = new StreamWriter(ItemsPath);
            await using var itemWriter = new JsonTextWriter(itemStream);
            await itemWriter.WriteStartArrayAsync();
            await foreach (var item in itemChannel.Reader.ReadAllAsync())
            {
                Serializer.Serialize(itemWriter, item);
            }

            await itemWriter.WriteEndArrayAsync();
        });
        Parallel.For(1, ItemLoader.ItemCount, new ParallelOptions { MaxDegreeOfParallelism = 1000 }, () => Tuple.Create(new Item(), new Player()),
            (i, state, tuple) =>
            {
                var item = tuple.Item1;
                var player = tuple.Item2;
                if (state.ShouldExitCurrentIteration) return tuple;
                item.SetDefaults(i);
                if (item.type == ItemID.None) return tuple;

                JsonItem jsonItem = new(item);
                jsonItem.FindDrops();
                itemChannel.Writer.TryWrite(jsonItem);

                if (item.vanity) return tuple;
                if (item.headSlot != -1) JsonArmor.AddHead(i);
                if (item.bodySlot != -1) JsonArmor.AddBody(i);
                if (item.legSlot != -1) JsonArmor.AddLegs(i);
                return tuple;
            }, _ => { });

        itemChannel.Writer.Complete();
        await itemWriterTask;
        JsonArmor.FinishChannel();
        _savedItems = true;
    }

    private async Task SaveArmors()
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
    }

    private async Task SaveGroups()
    {
        if (_savedGroups) return;
        await using var sw = new StreamWriter(GroupsPath);
        await using var writer = new JsonTextWriter(sw);
        await writer.WriteStartObjectAsync();
        foreach (var (id, group) in RecipeGroup.recipeGroups)
        {
            await writer.WritePropertyNameAsync(id.ToString());
            Serializer.Serialize(writer, new JsonGroup(group));
        }

        await writer.WriteEndObjectAsync();
        _savedGroups = true;
    }

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