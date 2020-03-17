/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System;

namespace Piranha.Data
{
    [Serializable]
    public sealed class ContentGroupType
    {
        /// <summary>
        /// Gets/sets the parent group id.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Gets/sets the child group type id.
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Gets/sets the parent group.
        /// </summary>
        public ContentGroup Group { get; set; }
    }
}
