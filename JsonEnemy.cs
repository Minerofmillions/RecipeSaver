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
        public readonly Dictionary<int, float> globalDrops = new();
        public readonly Dictionary<int, float> normalDrops = new();
        public readonly Dictionary<int, float> expertDrops = new();
        public readonly Dictionary<int, float> masterDrops = new();

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
        private void AddRule(IItemDropRule rule, Dictionary<int, float> dict) {
            if (rule is CommonDrop commonDrop) {
                dict[commonDrop.itemId] = GetAverageItemCount(commonDrop.amountDroppedMinimum, commonDrop.amountDroppedMaximum, commonDrop.chanceNumerator, commonDrop.chanceDenominator);
            } else if (rule is DropBasedOnExpertMode expertModeDrop) {
                AddRule(expertModeDrop.ruleForNormalMode, normalDrops);
                AddRule(expertModeDrop.ruleForExpertMode, expertDrops);
            } else if (rule is DropBasedOnMasterMode masterModeDrop) {
                AddRule(masterModeDrop.ruleForMasterMode, masterDrops);
                AddRule(masterModeDrop.ruleForDefault, normalDrops);
            } else if (rule is DropOneByOne oneByOne) {
                DropOneByOne.Parameters oboParameters = oneByOne.parameters;
                dict[oneByOne.itemId] = GetAverageItemCount(oboParameters.MinimumItemDropsCount, oboParameters.MaximumItemDropsCount, oboParameters.ChanceNumerator, oboParameters.ChanceDenominator);
            } else if (rule is OneFromOptionsDropRule oneFromOptions) {
                int[] drops = oneFromOptions.dropIds;
                foreach (int drop in drops) {
                    dict[drop] = GetItemChance(oneFromOptions.chanceNumerator, oneFromOptions.chanceDenominator, drops.Length);
                }
            } else if (rule is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsNoLuck) {
                int[] drops = oneFromOptionsNoLuck.dropIds;
                foreach (int drop in drops) {
                    dict[drop] = GetItemChance(oneFromOptionsNoLuck.chanceNumerator, oneFromOptionsNoLuck.chanceDenominator, drops.Length);
                }
            } else {
                ruleTypes.Add(rule.GetType());
            }
            rule.ChainedRules.ForEach(r => AddRule(r.RuleToChain, dict));
        }

        private static float GetAverageItemCount(int min, int max, int p, int q) => (min + max) * p / (2f * q);
        private static float GetItemChance(int p, int q, int numItems) => (float) p / (q * numItems);
    }
}
