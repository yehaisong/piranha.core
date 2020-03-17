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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Piranha.Data;
using Piranha.Services;

namespace Piranha.Repositories
{
    public class ContentRepository : IContentRepository
    {
        private readonly IDb _db;
        private readonly ITransformationService _service;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="db">The current db connection</param>
        /// <param name="service">The current transformation service</param>
        public ContentRepository(IDb db, ITransformationService service)
        {
            _db = db;
            _service = service;
        }

        /// <summary>
        /// Gets the content model with the specified id.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="id">The unique id</param>
        /// <param name="languageId">The selected language id</param>
        /// <returns>The content model</returns>
        public async Task<T> GetByIdAsync<T>(Guid id, Guid languageId) where T : Models.Content
        {
            var content = await GetQuery()
                .FirstOrDefaultAsync(c => c.Id == id)
                .ConfigureAwait(false);

            if (content != null)
            {
                return await _service.ToModelAsync<T>(content, App.ContentTypes.GetById(content.TypeId), languageId)
                    .ConfigureAwait(false);
            }
            return null;
        }

        /// <summary>
        /// Saves the given content model
        /// </summary>
        /// <param name="model">The content model</param>
        /// <param name="languageId">The selected language id</param>
        public async Task SaveAsync<T>(T model, Guid languageId) where T : Models.Content
        {
            var type = App.ContentTypes.GetById(model.TypeId);

            // Make sure we have a valid content type
            if (type == null) return;

            var content = await _db.Content
                .Include(c => c.Translations)
                .Include(c => c.Fields).ThenInclude(f => f.Translations)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            // This is a new model
            if (content == null)
            {
                content = new Content
                {
                    Id = model.Id != Guid.Empty ? model.Id : Guid.NewGuid(),
                    Created = DateTime.Now,
                };
                model.Id = content.Id;

                await _db.Content.AddAsync(content).ConfigureAwait(false);

                // TODO: Check if the content should be positioned in the sitemap
            }
            else
            {
                // TODO: Check if the content should be moved in the sitemap
            }
            content.LastModified = DateTime.Now;

            // Transform model data
            await _service.ToContentAsync(_db, model, type, languageId, content);

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the content model with the specified id.
        /// </summary>
        /// <param name="id">The unique id</param>
        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the base query for content.
        /// </summary>
        /// <returns>The queryable</returns>
        private IQueryable<Content> GetQuery()
        {
            return (IQueryable<Content>)_db.Content
                .AsNoTracking()
                .Include(c => c.Translations)
                .Include(c => c.Fields).ThenInclude(f => f.Translations)
                .AsQueryable();
        }
    }
}
