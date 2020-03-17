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
    public sealed class ContentTypeAttribute : Attribute
    {
        private string _title;

        /// <summary>
        /// Gets/sets the unique id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the optional title.
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
        /// Gets/sets if blocks should be used. This is only applicable
        /// if the content type itself implements IBlockContent.
        /// </summary>
        public bool UseBlocks { get; set; } = true;
    }
}