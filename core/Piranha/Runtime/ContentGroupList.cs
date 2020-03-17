/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Linq;
using Piranha.Models;

namespace Piranha.Runtime
{
    public sealed class ContentGroupList : CachedList<ContentGroup>
    {
        /// <summary>
        /// Gets the content type with the given id.
        /// </summary>
        /// <param name="id">The unique id</param>
        /// <returns>The content type</returns>
        public ContentGroup GetById(string id)
        {
            return this.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Gets the item from the list that matches the given item.
        /// </summary>
        /// <param name="item">The item to search for</param>
        /// <returns>The matching item in the list</returns>
        protected override ContentGroup GetItem(ContentGroup item)
        {
            return this.FirstOrDefault(g => g.Id == item.Id);
        }
    }
}
