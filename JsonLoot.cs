using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;

namespace RecipeSaver {
    public class JsonLoot {
        public IDictionary<int, IDictionary<string, float>> globalLoot = new SortedDictionary<int, IDictionary<string, float>>();
        public IDictionary<int, IDictionary<string, float>> normalLoot = new SortedDictionary<int, IDictionary<string, float>>();
        public IDictionary<int, IDictionary<string, float>> expertLoot = new SortedDictionary<int, IDictionary<string, float>>();
        public IDictionary<int, IDictionary<string, float>> masterLoot = new SortedDictionary<int, IDictionary<string, float>>();

        private static void AddItemToDict(IDictionary<int, IDictionary<string, float>> dict, string condition, int itemId, int numerator, int denominator, int min, int max, float inheritedChance = 1f) =>
            AddItemToDict(dict, condition, itemId, inheritedChance * numerator / denominator, min, max);
        private static void AddItemToDict(IDictionary<int, IDictionary<string, float>> dict, string condition, int itemId, float chance, int min, int max) {
            if (!dict.TryGetValue(itemId, out var conditionalLoot)) {
                conditionalLoot = new SortedDictionary<string, float>();
                dict[itemId] = conditionalLoot;
            }
            if (!conditionalLoot.TryGetValue(condition, out float currentAverage)) currentAverage = 0f;
            conditionalLoot[condition] = currentAverage + chance * (min + max) / 2f;
        }

        public void AddRule(IItemDropRule rule) => AddRule(rule, globalLoot, 1f, "");
        private void AddRule(IItemDropRule rule, IDictionary<int, IDictionary<string, float>> dict, float inheritedChance, string condition) {
            string addedCondition;
            string newCondition;
            switch (rule) {
                case CommonDropWithRerolls c:
                    AddItemToDict(dict, condition, c.itemId, inheritedChance * (1 - MathF.Pow(1 - (float) c.chanceNumerator / c.chanceDenominator, c.timesToRoll)), c.amountDroppedMinimum, c.amountDroppedMaximum);
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
                    foreach (int dropId in o.dropIds) {
                        AddItemToDict(dict, condition, dropId, o.chanceNumerator, o.chanceDenominator * o.dropIds.Length, 1, 1, inheritedChance);
                    }
                    o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case OneFromOptionsNotScaledWithLuckDropRule o:
                    foreach (int dropId in o.dropIds) {
                        AddItemToDict(dict, condition, dropId, o.chanceNumerator, o.chanceDenominator * o.dropIds.Length, 1, 1, inheritedChance);
                    }
                    o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case OneFromRulesRule o:
                    foreach (var r in o.options) {
                        AddRule(r, dict, inheritedChance / (o.chanceDenominator * o.options.Length), condition);
                    }
                    o.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case SequentialRulesRule s:
                    foreach (var r in s.rules) {
                        AddRule(r, dict, inheritedChance, condition);
                    }
                    s.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case SequentialRulesNotScalingWithLuckRule s:
                    foreach (var r in s.rules) {
                        AddRule(r, dict, inheritedChance, condition);
                    }
                    s.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case FewFromOptionsDropRule f:
                    foreach (int dropId in f.dropIds) {
                        AddItemToDict(dict, condition, dropId, f.chanceNumerator * f.amount, f.chanceDenominator * f.dropIds.Length, 1, 1, inheritedChance);
                    }
                    f.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case FewFromOptionsNotScaledWithLuckDropRule f:
                    foreach (int dropId in f.dropIds) {
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
                    foreach (var r in a.rules) {
                        AddRule(r, dict, inheritedChance, condition);
                    }
                    a.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case HerbBagDropsItemDropRule h:
                    foreach (var id in h.dropIds) {
                        AddItemToDict(dict, condition, id, 115, 9, 1, 1, inheritedChance);
                    }
                    h.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case CoinsRule c:
                    long value = c.value;
                    if (value % 100 != 0L) {
                        AddItemToDict(dict, condition, 71, inheritedChance, (int) value % 100, (int) value % 100);
                    }
                    if ((value / 100) % 100 != 0L) {
                        AddItemToDict(dict, condition, 72, inheritedChance, (int) (value / 100) % 100, (int) (value / 100) % 100);
                    }
                    if ((value / 10000) % 100 != 0L) {
                        AddItemToDict(dict, condition, 72, inheritedChance, (int) (value / 10000) % 100, (int) (value / 10000) % 100);
                    }
                    if ((value / 1000000) % 100 != 0L) {
                        AddItemToDict(dict, condition, 72, inheritedChance, (int) (value / 1000000) % 100, (int) (value / 1000000) % 100);
                    }
                    c.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case CommonDrop c:
                    AddItemToDict(dict, condition, c.itemId, c.chanceNumerator, c.chanceDenominator, c.amountDroppedMinimum, c.amountDroppedMaximum, inheritedChance);
                    c.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, inheritedChance, condition));
                    break;
                case MechBossSpawnersDropRule:
                case SlimeBodyItemDropRule:
                case DropNothing:
                    break;
                default:
                    break;
            }
        }
    }
}
