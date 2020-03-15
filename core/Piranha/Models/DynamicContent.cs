/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using Piranha.Extend;

namespace Piranha.Models
{
    /// <summary>
    /// Dynamic content model. As this will be the same for all content types it needs to
    /// contain all possible data.
    /// </summary>
    [Serializable]
    public sealed class DynamicContent : RoutedContent, IDynamicContent, IBlockContent, ICategorizedContent, ITaggedContent
    {
        /// <summary>
        /// Gets/sets the category.
        /// </summary>
        public Taxonomy Category { get; set; }

        /// <summary>
        /// Gets/sets the regions.
        /// </summary>
        public dynamic Regions { get; set; } = new ExpandoObject();

        /// <summary>
        /// Gets/sets the available blocks.
        /// </summary>
        public IList<Block> Blocks { get; set; }

        /// <summary>
        /// Gets/sets the availabel tags.
        /// </summary>
        public IList<Taxonomy> Tags { get; set; } = new List<Taxonomy>();
    }
}