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
using Piranha.Extend;

namespace Piranha.Models
{
    /// <summary>
    /// Base class for all archives.
    /// </summary>
    [Serializable]
    [ContentGroup(Title = "Archive", DefaultRoute = "/archive")]
    [ContentGroupChild(typeof(Post))]
    [ContentTypeEditor(Title = "Archive", Icon = "fas fa-book", Component = "post-archive")]
    public abstract class Archive : RoutedContent, IBlockContent
    {
        /// <summary>
        /// Gets/sets the available blocks.
        /// </summary>
        public IList<Block> Blocks { get; set; }
    }
}