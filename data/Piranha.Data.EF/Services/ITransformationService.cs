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
using System.Threading.Tasks;
using Piranha.Data;

namespace Piranha.Services
{
    public interface ITransformationService
    {
        /// <summary>
        /// Transforms the given data into a new model.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="content">The content entity</param>
        /// <param name="type">The content type</param>
        /// <param name="languageId">The selected language id</param>
        /// <param name="process">Optional func that should be called after transformation</param>
        /// <returns>The page model</returns>
        Task<T> ToModelAsync<T>(Content content, Models.ContentType type, Guid languageId, Func<Content, T, Task> process = null)
            where T : Models.Content;

        /// <summary>
        /// Transforms the given model into content data.
        /// </summary>
        /// <param name="db">The current db context</param>
        /// <param name="model">The model</param>
        /// <param name="type">The conten type</param>
        /// <param name="languageId">The selected language id</param>
        /// <param name="dest">The optional dest object</param>
        /// <returns>The content data</returns>
        Task<Content> ToContentAsync<T>(IDb db, T model, Models.ContentType type, Guid languageId, Content dest = null)
            where T : Models.Content;
    }
}
