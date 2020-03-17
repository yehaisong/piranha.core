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
using System.Collections.Generic;

namespace Piranha.Data
{
    [Serializable]
    public sealed class ContentField
    {
        /// <summary>
        /// Gets/sets the unique id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets/sets the content id.
        /// </summary>
        public Guid ContentId { get; set; }

        /// <summary>
        /// Gets/sets the type id of the field.
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Gets/sets the region id.
        /// </summary>
        public string RegionId { get; set; }

        /// <summary>
        /// Gets/sets the field id.
        /// </summary>
        public string FieldId { get; set; }

        /// <summary>
        /// Gets/sets the sort order.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets/sets the serialized value for non
        /// translatable fields.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets/sets the available translations.
        /// </summary>
        public IList<ContentFieldTranslation> Translations { get; set; } = new List<ContentFieldTranslation>();
    }
}
