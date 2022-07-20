using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace RecipeSaver {
    internal class JsonEnemy {
        public string name;
        public int type;
        public readonly Dictionary<string, Dictionary<int, float>> globalDrops = new();
        public readonly Dictionary<string, Dictionary<int, float>> normalDrops = new();
        public readonly Dictionary<string, Dictionary<int, float>> expertDrops = new();
        public readonly Dictionary<string, Dictionary<int, float>> masterDrops = new();

        internal static readonly HashSet<Type> ruleTypes = new();

        public JsonEnemy(NPC npc) {
            name = npc.TypeName;
            type = npc.type;

            List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForNPCID(type);
            foreach (IItemDropRule rule in rules) {
                AddRule(rule);
            }
        }

        private void AddRule(IItemDropRule rule) {
            AddRule(rule, globalDrops);
        }
        private void AddRule(IItemDropRule rule, Dictionary<string, Dictionary<int, float>> dict, string condition = "", int inheritedDenominator = 1) {
            if (rule is ItemDropWithConditionRule conditionRule) {
                AddItemToDict(dict, conditionRule.itemId, GetAverageItemCount(conditionRule.amountDroppedMinimum, conditionRule.amountDroppedMaximum, conditionRule.chanceNumerator, conditionRule.chanceDenominator * inheritedDenominator), conditionRule.condition.GetConditionDescription());
            } else if (rule is CommonDrop commonDrop) {
                AddItemToDict(dict, commonDrop.itemId, GetAverageItemCount(commonDrop.amountDroppedMinimum, commonDrop.amountDroppedMaximum, commonDrop.chanceNumerator, commonDrop.chanceDenominator * inheritedDenominator), condition);
            } else if (rule is DropBasedOnExpertMode expertModeDrop) {
                AddRule(expertModeDrop.ruleForNormalMode, normalDrops, condition);
                AddRule(expertModeDrop.ruleForExpertMode, expertDrops, condition);
            } else if (rule is DropBasedOnMasterMode masterModeDrop) {
                AddRule(masterModeDrop.ruleForMasterMode, masterDrops, condition);
                AddRule(masterModeDrop.ruleForDefault, normalDrops, condition);
            } else if (rule is DropOneByOne oneByOne) {
                DropOneByOne.Parameters oboParameters = oneByOne.parameters;
                AddItemToDict(dict, oneByOne.itemId, GetAverageItemCount(oboParameters.MinimumItemDropsCount, oboParameters.MaximumItemDropsCount, oboParameters.ChanceNumerator, oboParameters.ChanceDenominator * inheritedDenominator), condition);
            } else if (rule is OneFromOptionsDropRule oneFromOptions) {
                int[] drops = oneFromOptions.dropIds;
                foreach (int drop in drops) {
                    AddItemToDict(dict, drop, GetItemChance(oneFromOptions.chanceNumerator, oneFromOptions.chanceDenominator * inheritedDenominator, drops.Length), condition);
                }
            } else if (rule is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsNoLuck) {
                int[] drops = oneFromOptionsNoLuck.dropIds;
                foreach (int drop in drops) {
                    AddItemToDict(dict, drop, GetItemChance(oneFromOptionsNoLuck.chanceNumerator, oneFromOptionsNoLuck.chanceDenominator * inheritedDenominator, drops.Length));
                }
            } else if (rule is OneFromRulesRule oneFromRules) {
                foreach (IItemDropRule r in oneFromRules.options) {
                    AddRule(r, dict, condition, oneFromRules.chanceDenominator);
                }
            } else {
                ruleTypes.Add(rule.GetType());
            }
            rule.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict, condition));
        }

        private static float GetAverageItemCount(int min, int max, int p, int q) => (min + max) * p / (2f * q);
        private static float GetItemChance(int p, int q, int numItems) => (float) p / (q * numItems);
        private static void AddItemToDict(Dictionary<string, Dictionary<int, float>> dict, int itemId, float chance, string condition = "") {
            if (condition is null) condition = "";
            if (!dict.TryGetValue(condition, out Dictionary<int, float> itemDict)) {
                itemDict = new();
                dict[condition] = itemDict;
            }
            if (!itemDict.TryGetValue(itemId, out float c)) {
                c = 0f;
            }
            itemDict[itemId] = c + chance;
        }
    }
}
