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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Cache;
using Piranha.Models;
using Piranha.Repositories;

namespace Piranha.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly ILanguageRepository _repo;
        private readonly ICache _cache;
        private static readonly string CacheKey = "Piranha_Languages";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="repo">The main repository</param>
        /// <param name="cache">The optional model cache</param>
        public LanguageService(ILanguageRepository repo, ICache cache)
        {
            _repo = repo;

            if (App.CacheLevel != CacheLevel.None)
            {
                _cache = cache;
            }
        }

        /// <summary>
        /// Gets all available models.
        /// </summary>
        /// <returns>The available models</returns>
        public Task<IEnumerable<Language>> GetAllAsync()
        {
            return GetLanguages();
        }

        /// <summary>
        /// Gets the model with the specified id.
        /// </summary>
        /// <param name="id">The unique id</param>
        /// <returns>The model</returns>
        public async Task<Language> GetByIdAsync(Guid id)
        {
            if (_cache != null && App.CacheLevel != CacheLevel.None)
            {
                var languages = await GetLanguages().ConfigureAwait(false);

                return languages.FirstOrDefault(l => l.Id == id);
            }
            else
            {
                return await _repo.GetByIdAsync(id).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the model with the given slug id.
        /// </summary>
        /// <param name="slug">The unique slug</param>
        /// <returns>The model</returns>
        public async Task<Language> GetBySlugAsync(string slug)
        {
            if (_cache != null && App.CacheLevel != CacheLevel.None)
            {
                var languages = await GetLanguages().ConfigureAwait(false);

                return languages.FirstOrDefault(l => l.Slug == slug);
            }
            else
            {
                return await _repo.GetBySlugdAsync(slug).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the default side.
        /// </summary>
        /// <returns>The modell</returns>
        public async Task<Language> GetDefaultAsync()
        {
            if (_cache != null && App.CacheLevel != CacheLevel.None)
            {
                var languages = await GetLanguages().ConfigureAwait(false);

                return languages.FirstOrDefault(l => l.IsDefault);
            }
            else
            {
                return await _repo.GetDefaultAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds or updates the given model in the database
        /// depending on its state.
        /// </summary>
        /// <param name="model">The model</param>
        public async Task SaveAsync(Language model)
        {
            // Make sure we have an id
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }

            // Validate model
            var context = new ValidationContext(model);
            Validator.ValidateObject(model, context, true);

            // Make sure we have a slug
            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                model.Slug = Utils.GenerateSlug(model.Title);
            }
            else
            {
                model.Slug = Utils.GenerateSlug(model.Slug);
            }

            // Make sure the slug is unique
            var lang = await _repo.GetBySlugdAsync(model.Slug).ConfigureAwait(false);
            if (lang != null && lang.Id != model.Id)
            {
                throw new ValidationException($"The slug field must be unique");
            }

            // Make sure we have a default language
            if (model.IsDefault)
            {
                // Make sure no other site is default first
                var def = await GetDefaultAsync().ConfigureAwait(false);

                if (def != null && def.Id != model.Id)
                {
                    def.IsDefault = false;
                    await _repo.SaveAsync(def).ConfigureAwait(false);
                }
            }
            else
            {
                // Make sure we have a default site
                var def = await _repo.GetDefaultAsync().ConfigureAwait(false);
                if (def == null || def.Id == model.Id)
                    model.IsDefault = true;
            }

            // Call hooks & save
            App.Hooks.OnBeforeSave(model);
            await _repo.SaveAsync(model).ConfigureAwait(false);
            App.Hooks.OnAfterSave(model);

            // Clear cache
            _cache?.Remove(CacheKey);
        }

        /// <summary>
        /// Deletes the model with the specified id.
        /// </summary>
        /// <param name="id">The unique id</param>
        public async Task DeleteAsync(Guid id)
        {
            var model = await _repo.GetByIdAsync(id).ConfigureAwait(false);

            if (model != null)
            {
                await DeleteAsync(model).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes the given model.
        /// </summary>
        /// <param name="model">The model</param>
        public async Task DeleteAsync(Language model)
        {
            // Call hooks & delete
            App.Hooks.OnBeforeDelete(model);
            await _repo.DeleteAsync(model.Id).ConfigureAwait(false);
            App.Hooks.OnAfterDelete(model);

            // Clear cache
            _cache?.Remove(CacheKey);
        }

        /// <summary>
        /// Gets the languages from the database.
        /// </summary>
        private async Task<IEnumerable<Language>> GetLanguages()
        {
            var types = _cache?.Get<IEnumerable<Language>>(CacheKey);

            if (types == null)
            {
                types = await _repo.GetAllAsync().ConfigureAwait(false);

                _cache?.Set(CacheKey, types);
            }
            return types;
        }
    }
}
