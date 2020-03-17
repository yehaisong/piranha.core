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
using Newtonsoft.Json;

namespace Piranha.Data
{
    [Serializable]
    public sealed class ContentRevision
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
        /// Gets/sets the data of the revision serialized
        /// as JSON.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets/sets the created date.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets the revision data deserialized as the
        /// specified type.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>The deserialized revision data</returns>
        public T GetData<T>()
        {
            if (!string.IsNullOrEmpty(Body))
                return JsonConvert.DeserializeObject<T>(Body);
            return default(T);
        }

        /// <summary>
        /// Gets/sets the content.
        /// </summary>
        public Content Content { get; set; }
    }
}
