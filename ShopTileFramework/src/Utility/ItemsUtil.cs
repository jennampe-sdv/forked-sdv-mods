﻿using ShopTileFramework.API;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework.Utility
{
    /// <summary>
    /// This class contains static utility methods used to handle items
    /// </summary>
    class ItemsUtil
    {
        public static Dictionary<string, IDictionary<int, string>> ObjectInfoSource { get; set; }
        public static List<string> RecipesList;
        private static Dictionary<int, string> _fruitTreeData;
        private static Dictionary<int, string> _cropData;

        private static List<string> _packsToRemove = new List<string>();
        private static List<string> _recipePacksToRemove = new List<string>();
        private static List<string> _itemsToRemove = new List<string>();

        /// <summary>
        /// Loads up the onject information for all types, 
        /// done at the start of each save loaded so that JA info is up to date
        /// </summary>
        public static void UpdateObjectInfoSource()
        {
            //load up all the object information into a static dictionary
            ObjectInfoSource = new Dictionary<string, IDictionary<int, string>>
            {
                { "Object", Game1.objectInformation },
                { "BigCraftable", Game1.bigCraftablesInformation },
                { "Clothing", Game1.clothingInformation },
                { "Ring", Game1.objectInformation },
                {
                    "Hat",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                        (@"Data/hats", ContentSource.GameContent)
                },
                {
                    "Boot",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                            (@"Data/Boots", ContentSource.GameContent)
                },
                {
                    "Furniture",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                            (@"Data/Furniture", ContentSource.GameContent)
                },
                {
                    "Weapon",
                    ModEntry.helper.Content.Load<Dictionary<int, string>>
                            (@"Data/weapons", ContentSource.GameContent)
                }
            };

            //load up recipe information
            RecipesList = ModEntry.helper.Content.Load<Dictionary<string, string>>(@"Data/CraftingRecipes", ContentSource.GameContent).Keys.ToList();
            RecipesList.AddRange(ModEntry.helper.Content.Load<Dictionary<string, string>>(@"Data/CookingRecipes", ContentSource.GameContent).Keys.ToList());

            //add "recipe" to the end of every element
            RecipesList = RecipesList.Select(s => s + " Recipe").ToList();

            //load up tree and crop data
            _fruitTreeData = ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/fruitTrees", ContentSource.GameContent);
            _cropData = ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/Crops", ContentSource.GameContent);
        }

        /// <summary>
        /// Given and ItemInventoryAndStock, and a maximum number, randomly reduce the stock until it hits that number
        /// </summary>
        /// <param name="inventory">the ItemPriceAndStock</param>
        /// <param name="maxNum">The maximum number of items we want for this stock</param>
        public static void RandomizeStock(Dictionary<ISalable, int[]> inventory, int maxNum)
        {
            while (inventory.Count > maxNum)
            {
                inventory.Remove(inventory.Keys.ElementAt(Game1.random.Next(inventory.Count)));
            }

        }

        /// <summary>
        /// Get the itemID given a name and the object information that item belongs to
        /// </summary>
        /// <param name="name">name of the item</param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public static int GetIndexByName(string name, string itemType= "Object")
        {
            foreach (KeyValuePair<int, string> kvp in ObjectInfoSource[itemType])
            {
                if (kvp.Value.Split('/')[0] == name)
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if an itemtype is valid
        /// </summary>
        /// <param name="itemType">The name of the itemtype</param>
        /// <returns>True if it's a valid type, false if not</returns>
        public static bool CheckItemType(string itemType)
        {
            return (itemType == "Seed" || ObjectInfoSource.ContainsKey(itemType));
        }

        /// <summary>
        /// Given the name of a crop, return the ID of its seed object
        /// </summary>
        /// <param name="cropName">The name of the crop object</param>
        /// <returns>The ID of the seed object if found, -1 if not</returns>
        public static int GetSeedId(string cropName)
        {
            //int cropID = ModEntry.JsonAssets.GetCropId(cropName);
            int cropId = GetIndexByName(cropName);
            foreach (KeyValuePair<int, string> kvp in _cropData)
            {
                //find the tree id in crops information to get seed id
                Int32.TryParse(kvp.Value.Split('/')[3], out int id);
                if (cropId == id)
                    return kvp.Key;
            }

            return -1;
        }

        /// <summary>
        /// Given the name of a tree crop, return the ID of its sapling object
        /// </summary>
        /// <returns>The ID of the sapling object if found, -1 if not</returns>
        public static int GetSaplingId(string treeName)
        {
            int treeId = GetIndexByName(treeName);
            foreach (KeyValuePair<int, string> kvp in _fruitTreeData)
            {
                //find the tree id in fruitTrees information to get sapling id
                Int32.TryParse(kvp.Value.Split('/')[2], out int id);
                if (treeId == id)
                    return kvp.Key;
            }

            return -1;
        }

        public static void RegisterPacksToRemove(string[] JApacks,string[] recipePacks)
        {
            _packsToRemove = _packsToRemove.Union(JApacks).ToList();
            _recipePacksToRemove = _recipePacksToRemove.Union(recipePacks).ToList();
        }

        public static void RegisterItemsToRemove()
        {
            if (APIs.JsonAssets == null)
                return;

            foreach (string pack in _packsToRemove)
            {

                var items = APIs.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    _itemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllClothingFromContentPack(pack);
                if (items != null)
                    _itemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllHatsFromContentPack(pack);
                if (items != null)
                    _itemsToRemove.AddRange(items);

                items = APIs.JsonAssets.GetAllObjectsFromContentPack(pack);
                if (items != null)
                {
                    _itemsToRemove.AddRange(items);
                }

                items = APIs.JsonAssets.GetAllWeaponsFromContentPack(pack);
                if (items != null)
                    _itemsToRemove.AddRange(items);
            }

            foreach (string pack in _recipePacksToRemove)
            {
                var items = APIs.JsonAssets.GetAllBigCraftablesFromContentPack(pack);
                if (items != null)
                    _itemsToRemove.AddRange(items.Select(i => (i + " Recipe")));

                items = APIs.JsonAssets.GetAllObjectsFromContentPack(pack);
                if (items != null)
                {
                    _itemsToRemove.AddRange(items.Select(i => (i + " Recipe")));
                }

            }
        }

        public static Dictionary<ISalable, int[]> RemoveSpecifiedJAPacks(Dictionary<ISalable, int[]> stock)
        {
            List<ISalable> removeItems = (stock.Keys.Where(item => _itemsToRemove.Contains(item.Name))).ToList();
            
            foreach (var item in removeItems)
            {
                stock.Remove(item);
            }

            return stock;
        }

        public static void RemoveSoldOutItems(Dictionary<ISalable, int[]> stock)
        {
            List<ISalable> keysToRemove = (stock.Where(kvp => kvp.Value[1] == 0).Select(kvp => kvp.Key)).ToList();
            foreach (ISalable item in keysToRemove)
                stock.Remove(item);
        }
    }
}
