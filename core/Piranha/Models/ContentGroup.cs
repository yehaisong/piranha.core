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
using System.ComponentModel.DataAnnotations;

namespace Piranha.Models
{
    /// <summary>
    /// Class for defining a content group.
    /// </summary>
    [Serializable]
    public class ContentGroup
    {
        /// <summary>
        /// Gets/sets the unique id.
        /// </summary>
        [Required]
        [StringLength(64)]
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the type name of the content group.
        /// </summary>
        [StringLength(255)]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets/sets the short assembly name of the content group.
        /// </summary>
        [StringLength(255)]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets/sets the display title.
        /// </summary>
        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        /// <summary>
        /// Gets/sets if this group can be routed to directly.
        /// </summary>
        public bool IsRoutedContent { get; set; }

        /// <summary>
        /// Gets/sets if this group is primary content that
        /// should be positioned in the sitemap.
        /// </summary>
        public bool IsPrimaryContent { get; set; }

        /// <summary>
        /// Gets/sets the available child group types.
        /// </summary>
        public IList<string> ChildGroups { get; set; } = new List<string>();
    }
}