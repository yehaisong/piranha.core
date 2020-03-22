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
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using Piranha.Models;
using Piranha.Repositories;

namespace Piranha.Services
{
    public class ContentService : IContentService
    {
        private readonly IContentRepository _pageRepo;
        private readonly IContentFactory _factory;
        private readonly ILanguageService _langService;
        private readonly ICache _cache;
        private readonly ISearch _search;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="pageRepo">The main page repository</param>
        /// <param name="factory">The content factory</param>
        /// <param name="langService">The language service</param>
        /// <param name="cache">The optional cache service</param>
        /// <param name="search">The optional search service</param>
        public ContentService(IContentRepository pageRepo, IContentFactory factory, ILanguageService langService, ICache cache = null, ISearch search = null)
        {
            _pageRepo = pageRepo;
            _factory = factory;
            _langService = langService;

            if ((int)App.CacheLevel > 2)
            {
                _cache = cache;
            }
            _search = search;
        }

        /// <summary>
        /// Creates and initializes a new content model.
        /// </summary>
        /// <param name="typeId">The content type id</param>
        /// <returns>The created page</returns>
        public async Task<T> CreateAsync<T>(string typeId) where T : Content
        {
            if (typeId == null)
            {
                var attr = typeof(T).GetCustomAttribute<Extend.ContentTypeAttribute>();
                if (attr != null)
                {
                    typeId = attr.Id;
                }
            }

            var type = App.ContentTypes.GetById(typeId);

            if (type != null)
            {
                var model = await _factory.CreateAsync<T>(type).ConfigureAwait(false);

                //using (var config = new Config(_paramService))
                //{
                //    model.EnableComments = config.CommentsEnabledForPages;
                //    model.CloseCommentsAfterDays = config.CommentsCloseAfterDays;
                //}
                return model;
            }
            return null;
        }

        /// <summary>
        /// Gets the content model with the specified id.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="id">The unique id</param>
        /// <param name="languageId">The optional language id</param>
        /// <returns>The content model</returns>
        public async Task<T> GetByIdAsync<T>(Guid id, Guid? languageId = null) where T : Content
        {
            T model = null;

            // Make sure we have a language id
            if (languageId == null)
            {
                languageId = (await _langService.GetDefaultAsync())?.Id;
            }

            // First, try to get the model from cache
            if (typeof(IDynamicContent).IsAssignableFrom(typeof(T)))
            {
                model = _cache?.Get<T>($"DynamicContent_{ id.ToString() }");
            }
            else
            {
                model = _cache?.Get<T>(id.ToString());
            }

            // If we have a model, let's initialize it
            if (model != null)
            {
                await _factory.InitAsync(model, App.ContentTypes.GetById(model.TypeId)).ConfigureAwait(false);
            }

            // If we don't have a model, get it from the repository
            if (model == null)
            {
                model = await _pageRepo.GetByIdAsync<T>(id, languageId.Value).ConfigureAwait(false);

                await OnLoadAsync(model).ConfigureAwait(false);
            }

            // Check that we got back the requested type from the
            // repository
            if (model != null && model is T)
            {
                return model;
            }
            return null;
        }

        /// <summary>
        /// Saves the given content model
        /// </summary>
        /// <param name="model">The content model</param>
        /// <param name="languageId">The optional language id</param>
        public async Task SaveAsync<T>(T model, Guid? languageId = null) where T : Content
        {
            // Make sure we have an Id
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }

            // Make sure we have a language id
            if (languageId == null)
            {
                languageId = (await _langService.GetDefaultAsync())?.Id;
            }

            // Validate model
            var context = new ValidationContext(model);
            Validator.ValidateObject(model, context, true);

            if (model is RoutedContent routedModel)
            {
                // Make sure we have a slug
                if (string.IsNullOrWhiteSpace(routedModel.Slug))
                {
                    routedModel.Slug = Utils.GenerateSlug(!string.IsNullOrWhiteSpace(routedModel.NavigationTitle) ? routedModel.NavigationTitle : routedModel.Title);
                }
                else
                {
                    routedModel.Slug = Utils.GenerateSlug(routedModel.Slug);
                }
            }

            // Call hooks and save
            App.Hooks.OnBeforeSave<Content>(model);
            await _pageRepo.SaveAsync(model, languageId.Value);
            App.Hooks.OnAfterSave<Content>(model);

            // Remove from cache
            await RemoveFromCacheAsync(model).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the content model with the specified id.
        /// </summary>
        /// <param name="id">The unique id</param>
        public async Task DeleteAsync(Guid id)
        {
            var model = await GetByIdAsync<Content>(id).ConfigureAwait(false);

            if (model != null)
            {
                await DeleteAsync(model).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes the given content model.
        /// </summary>
        /// <param name="model">The content model</param>
        public async Task DeleteAsync<T>(T model) where T : Content
        {
            // Call hooks and delete
            App.Hooks.OnBeforeDelete<Content>(model);
            await _pageRepo.DeleteAsync(model.Id).ConfigureAwait(false);
            App.Hooks.OnAfterDelete<Content>(model);

            // Delete search document
            if (_search != null)
            {
                // TODO
                // await _search.DeletePageAsync(model);
            }

            // Remove from cache & invalidate sitemap
            await RemoveFromCacheAsync(model).ConfigureAwait(false);

            // TODO
            // await _siteService.InvalidateSitemapAsync(model.SiteId).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes the model after it has been loaded from
        /// the repository.
        /// </summary>
        /// <param name="model">The content model</param>
        private async Task OnLoadAsync(Content model)
        {
            // Make sure we have a model
            if (model == null) return;

            // Initialize the model
            await _factory.InitAsync(model, App.ContentTypes.GetById(model.TypeId));

            // Execute on load hook
            App.Hooks.OnLoad(model);

            // Update the cache if available
            if (_cache != null)
            {
                // Store the model
                if (model is IDynamicContent)
                {
                    _cache.Set($"DynamicContent_{ model.Id.ToString() }", model);
                }
                else
                {
                    _cache.Set(model.Id.ToString(), model);
                }

                // Store the slug > id mapping if this content is routed
                if (model is RoutedContent routedModel)
                {
                    _cache.Set($"ContentId_{ routedModel.Slug }", model.Id);
                }
            }
        }

        /// <summary>
        /// Removes the given model from the cache.
        /// </summary>
        /// <param name="model">The model</param>
        private Task RemoveFromCacheAsync(Content model)
        {
            return Task.Run(() =>
            {
                if (_cache != null)
                {
                    _cache.Remove(model.Id.ToString());
                    _cache.Remove($"DynamicContent_{ model.Id.ToString() }");

                    if (model is RoutedContent routedModel)
                    {
                        _cache.Remove($"ContentId_{ routedModel.Slug }");
                    }

                    /*
                    * TODO
                    *
                    if (!model.ParentId.HasValue && model.SortOrder == 0)
                    {
                        _cache.Remove($"Page_{model.SiteId}");
                        _cache.Remove($"PageInfo_{model.SiteId}");
                    }

                    // Remove the site & clear the sitemap from cache
                    await _siteService.RemoveSitemapFromCacheAsync(model.SiteId).ConfigureAwait(false);
                    */
                }
            });
        }
    }
}
