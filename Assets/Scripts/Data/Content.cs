/*
 * Tabletop Theatre
 * Copyright (C) 2020-2022 Robert van Kooten
 * Original source code: https://github.com/ThatRobVK/Tabletop-Theatre
 * License: https://github.com/ThatRobVK/Tabletop-Theatre/blob/main/LICENSE
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditorInternal.VersionControl;
using UnityEngine;

namespace TT.Data
{
    /// <summary>
    /// The root class through which game content is accessed. Use the Current static singleton property to interact
    /// with the loaded content. Note this property may be null before the content is loaded. The OnContentChanged
    /// event can be used to be notified of changes to the loaded content (including the initial load).
    /// </summary>
    public class Content
    {
        #region Events

        /// <summary>
        /// Raised when the content has been changed, e.g. by re-loading or by changing the selected content packs.
        /// </summary>
        public static Action OnContentChanged;

        #endregion


        #region Public properties

        /// <summary>
        /// All packs that were loaded from the content file. This may include packs that were not selected for the
        /// editor.
        /// </summary>
        public ContentPack[] Packs = { };

        /// <summary>
        /// A pack that represents the combination of all selected packs. This can be used to access the content that
        /// should be displayed to the user.
        /// </summary>
        public ContentPack Combined = new ContentPack();

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static Content Current { get; private set; }

        /// <summary>
        /// Indicates whether content has been loaded yet or not.
        /// </summary>
        public static bool ContentLoaded { get; private set; }

        #endregion


        #region Public methods

        /// <summary>
        /// Loads content available to the current user.
        /// </summary>
        public static async Task Load()
        {
            if (!Helpers.Comms.User.IsLoggedIn)
            {
                Current = null;
                ContentLoaded = false;
            }

            try
            {
                var json = await CommsLib.GameContent.GetContentJsonAsync();
                Current = JsonConvert.DeserializeObject<Content>(json);
                SetDerivedValues();
                CombinePacks();

                ContentLoaded = true;
                OnContentChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Content :: Load :: Exception {0}: {1}", ex.GetType().FullName, ex.Message);
                Current = null;
                ContentLoaded = false;
            }
        }

        /// <summary>
        /// Finds the ContentItem with the specified ID of the specified type, and returns it. If no item is found,
        /// null is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns>A ContentItem if found, otherwise null.</returns>
        public static ContentItem GetContentItemById(WorldObjectType type, string id)
        {
            var categories = GetCategoryByType(type);

            if (categories.Length > 0)
            {
                foreach (var category in categories)
                {
                    var item = SearchItemRecursively(category, id);
                    if (item != null) return item;
                }
            }
            else
            {
                var items = GetItemsByType(type);
                foreach (var item in items)
                foreach (var itemId in item.IDs)
                    if (itemId.Equals(id))
                        return item;
            }

            return null;
        }

        /// <summary>
        /// Returns the combined content category for the specified type.
        /// </summary>
        /// <param name="type">The type to return the categories for.</param>
        /// <returns>An array of content categories for the specified type.</returns>
        public static ContentItemCategory[] GetCategoryByType(WorldObjectType type)
        {
            if (type == WorldObjectType.NatureObject) return Current.Combined.Nature;
            if (type == WorldObjectType.Building) return Current.Combined.Construction.Buildings;
            if (type == WorldObjectType.ConstructionProp) return Current.Combined.Construction.Props;
            if (type == WorldObjectType.Lightsource) return Current.Combined.Lightsources;
            if (type == WorldObjectType.Item) return Current.Combined.Items;

            return new ContentItemCategory[] { };
        }

        /// <summary>
        /// Returns an array of content items based on the specified type.
        /// </summary>
        /// <param name="type">The WorldObjectType to return ContentItems for.</param>
        /// <returns>
        /// An array of ContentItems representing the type. If the type has categories, an empty list is returned
        /// instead.
        /// </returns>
        public static ContentItem[] GetItemsByType(WorldObjectType type)
        {
            string[] items = null;
            if (type == WorldObjectType.River) items = Current.Combined.RiversRoads.Rivers;
            if (type == WorldObjectType.Road) items = Current.Combined.RiversRoads.Roads;
            if (type == WorldObjectType.Bridge) items = Current.Combined.RiversRoads.Bridges;

            var contentItems = new List<ContentItem>();

            if (items != null)
            {
                var contentItemTemplate = TypeToContentItemMap[type];
                foreach (var item in items)
                    contentItems.Add(new ContentItem()
                    {
                        Name = contentItemTemplate.Name,
                        Traversable = contentItemTemplate.Traversable,
                        Type = type,
                        Scale = contentItemTemplate.Scale,
                        Lights = contentItemTemplate.Lights,
                        IDs = new[] {item}
                    });
            }

            return contentItems.ToArray();
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Iterates through the content packs and sets the appropriate WorldObjectType for each item, based on the category it is in.
        /// </summary>
        private static void SetDerivedValues()
        {
            foreach (var pack in Current.Packs)
            {
                SetDerivedValues(pack.Construction.Buildings, WorldObjectType.Building);
                SetDerivedValues(pack.Construction.Props, WorldObjectType.ConstructionProp);
                SetDerivedValues(pack.Items, WorldObjectType.Item);
                SetDerivedValues(pack.Lightsources, WorldObjectType.Lightsource);
                SetDerivedValues(pack.Nature, WorldObjectType.NatureObject);
            }
        }

        /// <summary>
        /// Iterates through an array of categories recursively, setting all items' WorldObjectType to that specified.
        /// </summary>
        /// <param name="categories">An array of categories to recursively loop over.</param>
        /// <param name="type">The type to set all items inside this category to.</param>
        private static void SetDerivedValues(ContentItemCategory[] categories, WorldObjectType type)
        {
            foreach (var category in categories)
            {
                foreach (var item in category.Items)
                {
                    // Set each item's type, and cascade the category name down to the item
                    item.Type = type;
                    item.Name = category.Name;
                    item.Category = category;
                }

                if (category.Categories.Length > 0)
                    // Recurse over subcategories
                    SetDerivedValues(category.Categories, type);
            }
        }

        /// <summary>
        /// Loops through all Items in the specified category, searching for an Item with the specified id. This method
        /// is recursive and will call itself on all sub-Categories in the specified Category.
        /// </summary>
        /// <param name="category">The category to search.</param>
        /// <param name="id">The ID to search for.</param>
        /// <returns>A content item with the specified ID, or null if none is found.</returns>
        private static ContentItem SearchItemRecursively(ContentItemCategory category, string id)
        {
            // Go through all items and their ID's, return the item if the search ID is found
            foreach (var item in category.Items)
            foreach (var itemId in item.IDs)
                if (itemId.Equals(id))
                    return item;

            // Go through all sub-categories and recurse over them
            foreach (var subcategory in category.Categories)
            {
                var item = SearchItemRecursively(subcategory, id);
                if (item != null) return item;
            }

            return null;
        }

        /// <summary>
        /// Populates the Combined property with a union of all selected content packs.
        /// </summary>
        private static void CombinePacks()
        {
            var constructionBuildings = new List<ContentItemCategory>();
            var constructionProps = new List<ContentItemCategory>();
            var constructionCeilings = new List<string>();
            var constructionFloors = new List<string>();
            var constructionWalls = new List<string>();
            var items = new List<ContentItemCategory>();
            var lightsources = new List<ContentItemCategory>();
            var nature = new List<ContentItemCategory>();
            var rivers = new List<string>();
            var roads = new List<string>();
            var bridges = new List<string>();
            var terrainLayers = new List<ContentTerrainLayer>();

            foreach (var pack in Current.Packs)
                if (pack.Selected)
                {
                    CombineCategories(ref constructionBuildings, pack.Construction.Buildings);
                    CombineCategories(ref constructionProps, pack.Construction.Props);
                    constructionCeilings.AddRange(pack.Construction.Ceilings);
                    constructionFloors.AddRange(pack.Construction.Floors);
                    constructionWalls.AddRange(pack.Construction.Walls);
                    CombineCategories(ref items, pack.Items);
                    CombineCategories(ref lightsources, pack.Lightsources);
                    nature.AddRange(pack.Nature);
                    rivers.AddRange(pack.RiversRoads.Rivers);
                    roads.AddRange(pack.RiversRoads.Roads);
                    bridges.AddRange(pack.RiversRoads.Bridges);
                    terrainLayers.AddRange(pack.TerrainLayers);
                }

            Current.Combined.Construction.Buildings = constructionBuildings.ToArray();
            Current.Combined.Construction.Props = constructionProps.ToArray();
            Current.Combined.Construction.Ceilings = constructionCeilings.ToArray();
            Current.Combined.Construction.Floors = constructionFloors.ToArray();
            Current.Combined.Construction.Walls = constructionWalls.ToArray();
            Current.Combined.Items = items.ToArray();
            Current.Combined.Lightsources = lightsources.ToArray();
            Current.Combined.Nature = nature.ToArray();
            Current.Combined.RiversRoads.Rivers = rivers.ToArray();
            Current.Combined.RiversRoads.Roads = roads.ToArray();
            Current.Combined.RiversRoads.Bridges = bridges.ToArray();
            Current.Combined.TerrainLayers = terrainLayers.ToArray();
        }

        /// <summary>
        /// Adds items to the specified list, combining any categories of the same name into one and adding new
        /// categories to the end of the list. This method is recursive and will combine all subcategories as well.
        /// </summary>
        /// <param name="listToAddTo">The list of categories to merge into. This is a ref parameter and will be changed
        ///     by this method.</param>
        /// <param name="itemsToAdd">The list of items to add. This parameter will not be changed.</param>
        private static void CombineCategories(ref List<ContentItemCategory> listToAddTo, IEnumerable<ContentItemCategory> itemsToAdd)
        {
            foreach (var itemToAdd in itemsToAdd)
            {
                bool categoryFound = false;
                foreach (var listItem in listToAddTo)
                {
                    if (listItem.Name.Equals(itemToAdd.Name))
                    {
                        // Same category
                        categoryFound = true;
                        
                        if (itemToAdd.Categories.Length > 0)
                        {
                            // If there are further subcategories, combine them
                            var listCategories = listItem.Categories.ToList();
                            CombineCategories(ref listCategories, itemToAdd.Categories);
                            listItem.Categories = listCategories.ToArray();
                        }

                        if (itemToAdd.Items.Length > 0)
                        {
                            // If there are items, combine them
                            var itemList = listItem.Items.ToList();
                            itemList.AddRange(itemToAdd.Items);
                            listItem.Items = itemList.ToArray();
                        }

                        // End loop if found
                        break;
                    }
                }

                if (!categoryFound)
                {
                    // If not found, add it
                    listToAddTo.Add(itemToAdd);
                }
            }
        }

        /// <summary>
        /// A map of WorldObjectType to ContentItem templates.
        /// </summary>
        private static readonly Dictionary<WorldObjectType, ContentItem> TypeToContentItemMap =
            new Dictionary<WorldObjectType, ContentItem>()
            {
                {WorldObjectType.Bridge, new ContentItem() {Name = "Bridge", Traversable = true}},
                {WorldObjectType.River, new ContentItem() {Name = "River", Traversable = false}},
                {WorldObjectType.Road, new ContentItem() {Name = "Road", Traversable = true}}
            };

        #endregion
    }
}