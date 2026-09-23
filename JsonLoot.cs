using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Terraria.GameContent.ItemDropRules;

namespace RecipeSaver;

public class JsonLoot
{
    [UsedImplicitly] public readonly IDictionary<int, IDictionary<string, float>> globalLoot = new SortedDictionary<int, IDictionary<string, float>>();
    [UsedImplicitly] public readonly IDictionary<int, IDictionary<string, float>> normalLoot = new SortedDictionary<int, IDictionary<string, float>>();
    [UsedImplicitly] public readonly IDictionary<int, IDictionary<string, float>> expertLoot = new SortedDictionary<int, IDictionary<string, float>>();
    [UsedImplicitly] public readonly IDictionary<int, IDictionary<string, float>> masterLoot = new SortedDictionary<int, IDictionary<string, float>>();

    private bool GlobalEmpty => DictIsEmpty(globalLoot);
    private bool NormalEmpty => DictIsEmpty(normalLoot);
    private bool ExpertEmpty => DictIsEmpty(expertLoot);
    private bool MasterEmpty => DictIsEmpty(masterLoot);
    private bool IsEmpty => GlobalEmpty && NormalEmpty && ExpertEmpty && MasterEmpty;

    private static void AddItemToDict(IDictionary<int, IDictionary<string, float>> dict, string condition, int itemId, int numerator, int denominator, int min, int max, float inheritedChance = 1f) =>
        AddItemToDict(dict, condition, itemId, inheritedChance * numerator / denominator, min, max);
    private static void AddItemToDict(IDictionary<int, IDictionary<string, float>> dict, string condition, int itemId, float chance, int min, int max)
    {
        if (!dict.TryGetValue(itemId, out var conditionalLoot))
        {
            conditionalLoot = new SortedDictionary<string, float>();
            dict[itemId] = conditionalLoot;
        }
        if (!conditionalLoot.TryGetValue(condition, out var currentAverage)) currentAverage = 0f;
        conditionalLoot[condition] = currentAverage + chance * (min + max) / 2f;
    }

    public void AddRule(IItemDropRule rule) => AddRule(rule, globalLoot, 1f, "");
    private void AddRule(IItemDropRule rule, IDictionary<int, IDictionary<string, float>> dict, float inheritedChance, string condition)
    {
        string addedCondition;
        string newCondition;
        switch (rule)
        {
            case CommonDropWithRerolls c:
                AddItemToDict(dict, condition, c.itemId, inheritedChance * (1 - MathF.Pow(1 - (float)c.chanceNumerator / c.chanceDenominator, c.timesToRoll)), c.amountDroppedMinimum, c.amountDroppedMaximum);
                c.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case ItemDropWithConditionRule i:
                addedCondition = i.condition.GetConditionDescription() ?? "";
                if (condition == "") newCondition = addedCondition;
                else if (addedCondition == "") newCondition = condition;
                else newCondition = condition + "&&" + addedCondition;
                AddItemToDict(dict, newCondition, i.itemId, i.chanceNumerator, i.chanceDenominator, i.amountDroppedMinimum, i.amountDroppedMaximum, inheritedChance);
                i.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, newCondition));
                break;
            case OneFromOptionsDropRule o:
                foreach (var dropId in o.dropIds)
                {
                    AddItemToDict(dict, condition, dropId, o.chanceNumerator, o.chanceDenominator * o.dropIds.Length, 1, 1, inheritedChance);
                }
                o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case OneFromOptionsNotScaledWithLuckDropRule o:
                foreach (var dropId in o.dropIds)
                {
                    AddItemToDict(dict, condition, dropId, o.chanceNumerator, o.chanceDenominator * o.dropIds.Length, 1, 1, inheritedChance);
                }
                o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case OneFromRulesRule o:
                foreach (var r in o.options)
                {
                    AddRule(r, dict, inheritedChance / (o.chanceDenominator * o.options.Length), condition);
                }
                o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case SequentialRulesRule s:
                foreach (var r in s.rules)
                {
                    AddRule(r, dict, inheritedChance, condition);
                }
                s.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case SequentialRulesNotScalingWithLuckRule s:
                foreach (var r in s.rules)
                {
                    AddRule(r, dict, inheritedChance, condition);
                }
                s.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case FewFromOptionsDropRule f:
                foreach (var dropId in f.dropIds)
                {
                    AddItemToDict(dict, condition, dropId, f.chanceNumerator * f.amount, f.chanceDenominator * f.dropIds.Length, 1, 1, inheritedChance);
                }
                f.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case FewFromOptionsNotScaledWithLuckDropRule f:
                foreach (var dropId in f.dropIds)
                {
                    AddItemToDict(dict, condition, dropId, f.chanceNumerator * f.amount, f.chanceDenominator * f.dropIds.Length, 1, 1, inheritedChance);
                }
                f.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case DropBasedOnExpertMode d:
                AddRule(d.ruleForExpertMode, expertLoot, inheritedChance, condition);
                AddRule(d.ruleForNormalMode, normalLoot, inheritedChance, condition);
                d.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case DropBasedOnMasterMode d:
                AddRule(d.ruleForMasterMode, masterLoot, inheritedChance, condition);
                AddRule(d.ruleForDefault, dict, inheritedChance, condition);
                d.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case LeadingConditionRule l:
                addedCondition = l.condition.GetConditionDescription() ?? "";
                if (condition == "") newCondition = addedCondition;
                else if (addedCondition == "") newCondition = condition;
                else newCondition = condition + "&&" + addedCondition;
                l.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, newCondition));
                break;
            case DropOneByOne o:
                AddItemToDict(dict, condition, o.itemId, o.parameters.ChanceNumerator, o.parameters.ChanceDenominator, o.parameters.MinimumItemDropsCount, o.parameters.MaximumItemDropsCount, inheritedChance);
                o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case AlwaysAtleastOneSuccessDropRule a:
                foreach (var r in a.rules)
                {
                    AddRule(r, dict, inheritedChance, condition);
                }
                a.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case HerbBagDropsItemDropRule h:
                foreach (var id in h.dropIds)
                {
                    AddItemToDict(dict, condition, id, 115, 9, 1, 1, inheritedChance);
                }
                h.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case CoinsRule c:
                var value = c.value;
                if (value % 100 != 0L)
                {
                    AddItemToDict(dict, condition, 71, inheritedChance, (int)value % 100, (int)value % 100);
                }
                if ((value / 100) % 100 != 0L)
                {
                    AddItemToDict(dict, condition, 72, inheritedChance, (int)(value / 100) % 100, (int)(value / 100) % 100);
                }
                if ((value / 10000) % 100 != 0L)
                {
                    AddItemToDict(dict, condition, 72, inheritedChance, (int)(value / 10000) % 100, (int)(value / 10000) % 100);
                }
                if ((value / 1000000) % 100 != 0L)
                {
                    AddItemToDict(dict, condition, 72, inheritedChance, (int)(value / 1000000) % 100, (int)(value / 1000000) % 100);
                }
                c.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
            case CommonDrop c:
                AddItemToDict(dict, condition, c.itemId, c.chanceNumerator, c.chanceDenominator, c.amountDroppedMinimum, c.amountDroppedMaximum, inheritedChance);
                c.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                break;
        }
    }

    private static bool DictIsEmpty<TK, TV>(IDictionary<TK, TV> source) => source.Keys.Count == 0;

    public class LootConverter : JsonConverter<JsonLoot>
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override JsonLoot ReadJson(JsonReader reader, Type objectType, JsonLoot existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, JsonLoot value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (!value.GlobalEmpty)
            {
                writer.WritePropertyName("globalLoot");
                serializer.Serialize(writer, value.globalLoot);
            }
            if (!value.NormalEmpty)
            {
                writer.WritePropertyName("normalLoot");
                serializer.Serialize(writer, value.normalLoot);
            }
            if (!value.ExpertEmpty)
            {
                writer.WritePropertyName("expertLoot");
                serializer.Serialize(writer, value.expertLoot);
            }
            if (!value.MasterEmpty)
            {
                writer.WritePropertyName("masterLoot");
                serializer.Serialize(writer, value.masterLoot);
            }

            writer.WriteEndObject();
        }
    }
}