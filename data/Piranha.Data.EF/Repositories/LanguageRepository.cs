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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Piranha.Models;

namespace Piranha.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly IDb _db;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="db">The current db context</param>
        public LanguageRepository(IDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets all available models.
        /// </summary>
        /// <returns>The available models</returns>
        public async Task<IEnumerable<Language>> GetAllAsync()
        {
            return await _db.Languages
                .AsNoTracking()
                .OrderBy(l => l.Title)
                .Select(l => new Language
                {
                    Id = l.Id,
                    Culture = l.Culture,
                    IsDefault = l.IsDefault,
                    Slug = l.Slug,
                    Title = l.Title
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the model with the specified id.
        /// </summary>
        /// <param name="id">The unique id</param>
        /// <returns>The model, or null if it doesn't exist</returns>
        public async Task<Language> GetByIdAsync(Guid id)
        {
            return await _db.Languages
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new Language
                {
                    Id = l.Id,
                    Culture = l.Culture,
                    IsDefault = l.IsDefault,
                    Slug = l.Slug,
                    Title = l.Title
                })
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the model with the given slug id.
        /// </summary>
        /// <param name="slug">The unique slug</param>
        /// <returns>The model</returns>
        public async Task<Language> GetBySlugdAsync(string slug)
        {
            return await _db.Languages
                .AsNoTracking()
                .Where(l => l.Slug == slug)
                .Select(l => new Language
                {
                    Id = l.Id,
                    Culture = l.Culture,
                    IsDefault = l.IsDefault,
                    Slug = l.Slug,
                    Title = l.Title
                })
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the default side.
        /// </summary>
        /// <returns>The modell</returns>
        public async Task<Language> GetDefaultAsync()
        {
            return await _db.Languages
                .AsNoTracking()
                .Where(l => l.IsDefault)
                .Select(l => new Language
                {
                    Id = l.Id,
                    Culture = l.Culture,
                    IsDefault = l.IsDefault,
                    Slug = l.Slug,
                    Title = l.Title
                })
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Adds or updates the given model in the database
        /// depending on its state.
        /// </summary>
        /// <param name="model">The model</param>
        public async Task SaveAsync(Language model)
        {
            var language = await _db.Languages
                .FirstOrDefaultAsync(l => l.Id == model.Id)
                .ConfigureAwait(false);

            if (language == null)
            {
                language = new Data.Language
                {
                    Id = model.Id != Guid.Empty ? model.Id : Guid.NewGuid()
                };
                await _db.Languages.AddAsync(language).ConfigureAwait(false);
            }
            language.Culture = model.Culture;
            language.IsDefault = model.IsDefault;
            language.Slug = model.Slug;
            language.Title = model.Title;

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the model with the specified id.
        /// </summary>
        /// <param name="id">The unique id</param>
        public async Task DeleteAsync(Guid id)
        {
            var language = await _db.Languages
                .FirstOrDefaultAsync(l => l.Id == id)
                .ConfigureAwait(false);

            if (language != null)
            {
                _db.Languages.Remove(language);
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
