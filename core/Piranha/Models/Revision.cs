/*
 * Copyright (c) 2019 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System;

namespace Piranha.Models
{
    /// <summary>
    /// Model for a content revision.
    /// </summary>
    public class Revision
    {
        /// <summary>
        /// Gets/sets the unique revision id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets/sets the content id this is a revision of.
        /// </summary>
        public Guid ContentId { get; set; }

        /// <summary>
        /// Gets/sets the created date.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
