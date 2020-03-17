/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Collections.Generic;

namespace Piranha.Runtime
{
    /// <summary>
    /// Base class for lists that cache data from a service.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public abstract class CachedList<T> : List<T>
    {
        /// <summary>
        /// Initializes the model from the given list
        /// </summary>
        /// <param name="items">The itemss</param>
        public void Init(IEnumerable<T> items)
        {
            // Add the items
            foreach (var item in items)
            {
                Add(item);
            }

            // Register runtime hooks to update the collection
            App.Hooks.RegisterOnAfterSave<T>((model) =>
            {
                var old = GetItem(model);

                if (old != null)
                {
                    Remove(old);
                }
                Add(model);
            });
        }

        /// <summary>
        /// Gets the item from the list that matches the given item.
        /// </summary>
        /// <param name="item">The item to search for</param>
        /// <returns>The matching item in the list</returns>
        protected abstract T GetItem(T item);
    }
}
