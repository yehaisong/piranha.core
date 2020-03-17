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
    public sealed class Content
    {
        /// <summary>
        /// Gets/sets the unique id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets/sets the id of the content type.
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Gets/sets if comments should be enabled.
        /// </summary>
        public bool EnableComments { get; set; }

        /// <summary>
        /// Gets/sets after how many days after publish date comments
        /// should be closed. A value of 0 means never.
        /// </summary>
        public int CloseCommentsAfterDays { get; set; }

        /// <summary>
        /// Gets/sets the optional route.
        /// </summary>
	    public string Route { get; set; }

        /// <summary>
        /// Gets/sets the optional redirect url.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets/sets the redirect type.
        /// </summary>
        public Models.RedirectType RedirectType { get; set; } = Models.RedirectType.Temporary;

        /// <summary>
        /// Gets/sets the created date.
        /// </summary>
	    public DateTime Created { get; set; }

        /// <summary>
        /// Gets/sets the last modification date.
        /// </summary>
	    public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets/sets the publishe date.
        /// </summary>
	    public DateTime? Published { get; set; }

        /// <summary>
        /// Gets/sets the available fields.
        /// </summary>
        public IList<ContentField> Fields { get; set; } = new List<ContentField>();

        /// <summary>
        /// Gets/sets the available translations.
        /// </summary>
        public IList<ContentTranslation> Translations { get; set; } = new List<ContentTranslation>();
    }
}
