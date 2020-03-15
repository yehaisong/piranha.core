/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System.Collections.Generic;

namespace Piranha.Models
{
    public interface ITaggedContent
    {
        /// <summary>
        /// Gets/sets the available tags.
        /// </summary>
        IList<Taxonomy> Tags { get; set; }
    }
}