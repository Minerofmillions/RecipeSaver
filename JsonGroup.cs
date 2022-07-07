using Terraria;
using System;
using System.Collections.Generic;

namespace RecipeSaver {
    public class JsonGroup {
        public List<int> validItems = new();
        public int iconicItem;
        public JsonGroup(RecipeGroup group) {
            iconicItem = group.IconicItemId;
            foreach (int item in group.ValidItems) {
                validItems.Add(item);
            }
            validItems.Sort();
        }
    }
}