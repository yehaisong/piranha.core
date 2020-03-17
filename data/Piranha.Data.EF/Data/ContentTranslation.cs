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
    public sealed class ContentTranslation
    {
        /// <summary>
        /// Gets/sets the content id.
        /// </summary>
        public Guid ContentId { get; set; }

        /// <summary>
        /// Gets/sets the language id.
        /// </summary>
        public Guid LanguageId { get; set; }

        /// <summary>
        /// Gets/sets the main title.
        /// </summary>
	    public string Title { get; set; }

        /// <summary>
        /// Gets/sets the optional navigation title.
        /// </summary>
	    public string NavigationTitle { get; set; }

        /// <summary>
        /// Gets/sets the unique slug.
        /// </summary>
	    public string Slug { get; set; }

        /// <summary>
        /// Gets/sets the optional meta title.
        /// </summary>
	    public string MetaTitle { get; set; }

        /// <summary>
        /// Gets/sets the optional meta keywords.
        /// </summary>
	    public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets/sets the optional meta description.
        /// </summary>
	    public string MetaDescription { get; set; }

        /// <summary>
        /// Gets/sets the content.
        /// </summary>
        public Content Content { get; set; }

        /// <summary>
        /// Gets/sets the language.
        /// </summary>
        public Language Language { get; set; }
    }
}
