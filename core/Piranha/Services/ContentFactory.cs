/*
 * Copyright (c) 2019-2020 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Piranha.Extend;
using Piranha.Models;

namespace Piranha.Services
{
    /// <summary>
    /// The content factory is responsible for creating models and
    /// initializing them after they have been loaded.
    /// </summary>
    public sealed class ContentFactory
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="services">The current service provider</param>
        public ContentFactory(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Creates and initializes a new content model.
        /// </summary>
        /// <param name="type">The content type</param>
        /// <typeparam name="T">The model type</typeparam>
        /// <returns>The new model</returns>
        public async Task<T> CreateAsync<T>(ContentType type) where T : Content
        {
            using (var scope = _services.CreateScope())
            {
                // Get the model type
                var modelType = typeof(T);
                if (!typeof(IDynamicContent).IsAssignableFrom(modelType))
                {
                    var assembly = Assembly.Load(type.AssemblyName);
                    modelType = assembly.GetType(type.TypeName);

                    if (modelType != typeof(T) && !typeof(T).IsAssignableFrom(modelType))
                    {
                        return null;
                    }
                }

                // Create the model
                T model = (T)Activator.CreateInstance(modelType);
                model.TypeId = type.Id;

                // Initialize basic properties
                if (model is IBlockContent blockModel)
                {
                    blockModel.Blocks = new List<Block>();
                }
                if (model is ICategorizedContent categorizedContent)
                {
                    categorizedContent.Category = new Taxonomy();
                }
                if (model is ITaggedContent taggedContent)
                {
                    taggedContent.Tags = new List<Taxonomy>();
                }

                // Initialize regions
                foreach (var regionType in type.Regions)
                {
                    object region = null;

                    if (!regionType.Collection)
                    {
                        if (model is IDynamicContent)
                        {
                            region = await CreateDynamicRegionAsync(scope, regionType);
                        }
                        else
                        {
                            region = await CreateRegionAsync(scope, model, modelType, regionType).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (model is IDynamicContent)
                        {
                            // Create a region item without initialization for type reference
                            var listObject = await CreateDynamicRegionAsync(scope, regionType, false).ConfigureAwait(false);

                            if (listObject != null)
                            {
                                // Create the region list
                                region = Activator.CreateInstance(typeof(RegionList<>).MakeGenericType(listObject.GetType()));
                                ((IRegionList)region).Model = (IDynamicContent)model;
                                ((IRegionList)region).TypeId = type.Id;
                                ((IRegionList)region).RegionId = regionType.Id;
                            }
                        }
                        else
                        {
                            var property = modelType.GetProperty(regionType.Id, App.PropertyBindings);

                            if (property != null)
                            {
                                region = Activator.CreateInstance(typeof(List<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]));
                            }
                        }
                    }

                    if (region != null)
                    {
                        if (model is IDynamicContent dynamicModel)
                        {
                            ((IDictionary<string, object>)dynamicModel.Regions).Add(regionType.Id, region);
                        }
                        else
                        {
                            modelType.SetPropertyValue(regionType.Id, model, region);
                        }
                    }
                }
                return model;
            }
        }

        /// <summary>
        /// Initializes the given dynamic model.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="type">The content type</param>
        /// <typeparam name="T">The model type</typeparam>
        /// <returns>The initialized model</returns>
        public async Task<T> InitDynamicAsync<T>(T model, ContentType type) where T : IDynamicContent
        {
            using (var scope = _services.CreateScope())
            {
                foreach (var regionType in type.Regions)
                {
                    // Try to get the region from the model
                    if (((IDictionary<string, object>)model.Regions).TryGetValue(regionType.Id, out var region))
                    {
                        if (!regionType.Collection)
                        {
                            // Initialize it
                            await InitDynamicRegionAsync(scope, region, regionType).ConfigureAwait(false);
                        }
                        else
                        {
                            // This region was a collection. Initialize all items
                            foreach (var item in (IList)region)
                            {
                                await InitDynamicRegionAsync(scope, item, regionType).ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (model is IBlockContent blockModel)
                {
                    foreach (var block in blockModel.Blocks)
                    {
                        await InitBlockAsync(scope, block).ConfigureAwait(false);

                        if (block is BlockGroup blockGroup)
                        {
                            foreach (var child in blockGroup.Items)
                            {
                                await InitBlockAsync(scope, child).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// Initializes the given model.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="type">The content type</param>
        /// <typeparam name="T">The model type</typeparam>
        /// <returns>The initialized model</returns>
        public async Task<T> InitAsync<T>(T model, ContentType type) where T : Content
        {
            if (model is IDynamicContent)
            {
                throw new ArgumentException("For dynamic models InitDynamic should be used.");
            }

            using (var scope = _services.CreateScope())
            {
                foreach (var regionType in type.Regions)
                {
                    // Try to get the region from the model
                    var region = model.GetType().GetPropertyValue(regionType.Id, model);

                    if (region != null)
                    {
                        if (!regionType.Collection)
                        {
                            // Initialize it
                            await InitRegionAsync(scope, region, regionType).ConfigureAwait(false);
                        }
                        else
                        {
                            // This region was a collection. Initialize all items
                            foreach (var item in (IList)region)
                            {
                                await InitRegionAsync(scope, item, regionType).ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (!(model is IContentInfo) && model is IBlockContent blockModel)
                {
                    foreach (var block in blockModel.Blocks)
                    {
                        await InitBlockAsync(scope, block).ConfigureAwait(false);

                        if (block is Extend.BlockGroup)
                        {
                            foreach (var child in ((Extend.BlockGroup)block).Items)
                            {
                                await InitBlockAsync(scope, child).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// Initializes all fields in the given dynamic region.
        /// </summary>
        /// <param name="scope">The current service scope</param>
        /// <param name="region">The region</param>
        /// <param name="regionType">The region type</param>
        private async Task InitDynamicRegionAsync(IServiceScope scope, object region, ContentTypeRegion regionType)
        {
            if (region != null)
            {
                if (regionType.Fields.Count == 1)
                {
                    // This region only has one field, that means
                    // the region is in fact a field.
                    await InitFieldAsync(scope, region).ConfigureAwait(false);
                }
                else
                {
                    // Initialize all fields
                    foreach (var fieldType in regionType.Fields)
                    {
                        if (((IDictionary<string, object>)region).TryGetValue(fieldType.Id, out var field))
                        {
                            await InitFieldAsync(scope, field).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes all fields in the given region.
        /// </summary>
        /// <param name="scope">The current service scope</param>
        /// <param name="region">The region</param>
        /// <param name="regionType">The region type</param>
        private async Task InitRegionAsync(IServiceScope scope, object region, ContentTypeRegion regionType)
        {
            if (region != null)
            {
                if (regionType.Fields.Count == 1)
                {
                    // This region only has one field, that means
                    // the region is in fact a field.
                    await InitFieldAsync(scope, region).ConfigureAwait(false);
                }
                else
                {
                    var type = region.GetType();

                    // Initialize all fields
                    foreach (var fieldType in regionType.Fields)
                    {
                        var field = type.GetPropertyValue(fieldType.Id, region);

                        if (field != null)
                        {
                            await InitFieldAsync(scope, field).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes all fields in the given block.
        /// </summary>
        /// <param name="scope">The current service scope</param>
        /// <param name="block">The block</param>
        private async Task InitBlockAsync(IServiceScope scope, Extend.Block block)
        {
            if (block != null)
            {
                var type = block.GetType();
                var properties = block.GetType().GetProperties(App.PropertyBindings);

                foreach (var property in properties)
                {
                    if (typeof(Extend.IField).IsAssignableFrom(property.PropertyType))
                    {
                        var field = property.GetValue(block);

                        if (field != null)
                        {
                            await InitFieldAsync(scope, field).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new dynamic region.
        /// </summary>
        /// <param name="scope">The current service scope</param>
        /// <param name="regionType">The region type</param>
        /// <param name="initFields">If fields should be initialized</param>
        /// <returns>The created region</returns>
        private async Task<object> CreateDynamicRegionAsync(IServiceScope scope, ContentTypeRegion regionType, bool initFields = true)
        {
            if (regionType.Fields.Count == 1)
            {
                var field = regionType.Fields[0].CreateInstance();
                if (field != null)
                {
                    if (initFields)
                    {
                        await InitFieldAsync(scope, field).ConfigureAwait(false);
                    }
                    return field;
                }
            }
            else
            {
                var reg = new ExpandoObject();

                foreach (var fieldType in regionType.Fields)
                {
                    var field = fieldType.CreateInstance();
                    if (field != null)
                    {
                        if (initFields)
                        {
                            await InitFieldAsync(scope, field).ConfigureAwait(false);
                        }
                        ((IDictionary<string, object>)reg).Add(fieldType.Id, field);
                    }
                }
                return reg;
            }
            return null;
        }

        /// <summary>
        /// Creates a new region.
        /// </summary>
        /// <param name="scope">The current service scope</param>
        /// <param name="model">The model to create the region for</param>
        /// <param name="modelType">The model type</param>
        /// <param name="regionType">The region type</param>
        /// <param name="initFields">If fields should be initialized</param>
        /// <returns>The created region</returns>
        private async Task<object> CreateRegionAsync(IServiceScope scope, object model, Type modelType, ContentTypeRegion regionType, bool initFields = true)
        {
            if (regionType.Fields.Count == 1)
            {
                var field = regionType.Fields[0].CreateInstance();
                if (field != null)
                {
                    if (initFields)
                    {
                        await InitFieldAsync(scope, field).ConfigureAwait(false);
                    }
                    return field;
                }
            }
            else
            {
                var property = modelType.GetProperty(regionType.Id, App.PropertyBindings);

                if (property != null)
                {
                    var reg = Activator.CreateInstance(property.PropertyType);

                    foreach (var fieldType in regionType.Fields)
                    {
                        var field = fieldType.CreateInstance();
                        if (field != null)
                        {
                            if (initFields)
                            {
                                await InitFieldAsync(scope, field).ConfigureAwait(false);
                            }
                            reg.GetType().SetPropertyValue(fieldType.Id, reg, field);
                        }
                    }
                    return reg;
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes the given field.
        /// </summary>
        /// <param name="scope">The current service scope</param>
        /// <param name="field">The field</param>
        /// <returns>The initialized field</returns>
        private async Task<object> InitFieldAsync(IServiceScope scope, object field)
        {
            var init = field.GetType().GetMethod("Init");

            if (init != null)
            {
                var param = new List<object>();

                foreach (var p in init.GetParameters())
                {
                    param.Add(scope.ServiceProvider.GetService(p.ParameterType));
                }

                // Check for async
                if (typeof(Task).IsAssignableFrom(init.ReturnType))
                {
                    await ((Task)init.Invoke(field, param.ToArray())).ConfigureAwait(false);
                }
                else
                {
                    init.Invoke(field, param.ToArray());
                }
            }
            return field;
        }
    }
}