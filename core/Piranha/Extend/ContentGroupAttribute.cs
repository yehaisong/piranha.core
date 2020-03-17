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

namespace Piranha.Extend
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContentGroupAttribute : Attribute
    {
        private string _title;

        /// <summary>
        /// Gets/sets the unique id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the title.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;

                if (string.IsNullOrWhiteSpace(Id))
                {
                    Id = Utils.GenerateInteralId(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the default route for the content group.
        /// </summary>
        public string DefaultRoute { get; set; }

        /// <summary>
        /// Gets/sets if instances of this group can be positioned
        /// directly in the sitemap.
        /// </summary>
        public bool IsPrimaryContent { get; set; } = true;
    }
}