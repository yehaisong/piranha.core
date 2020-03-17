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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Piranha.Services
{
    using Piranha.Data;
    using Piranha.Data.EF;

    public sealed class TransformationService : ITransformationService
    {
        private readonly IContentFactory _factory;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="factory">The content factory</param>
        public TransformationService(IContentFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Transforms the given data into a new model.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="content">The content entity</param>
        /// <param name="type">The content type</param>
        /// <param name="languageId">The selected language id</param>
        /// <param name="process">Optional func that should be called after transformation</param>
        /// <returns>The page model</returns>
        public async Task<T> ToModelAsync<T>(Content content, Models.ContentType type, Guid languageId, Func<Content, T, Task> process = null)
            where T : Models.Content
        {
            // Make sure we have a content type
            if (type == null) return null;

            var modelType = typeof(T);

            // Get the real type if we're not transforming to a dynamic model
            if (!typeof(Models.IDynamicContent).IsAssignableFrom(modelType))
            {
                var assembly = Assembly.Load(type.AssemblyName);
                modelType = assembly.GetType(type.TypeName);

                // Make sure the content has the requested type
                if (modelType != typeof(T) && !typeof(T).IsAssignableFrom(modelType))
                {
                    return null;
                }
            }

            // Create an initialized model
            var model = await _factory.CreateAsync<T>(type).ConfigureAwait(false);
            var translation = content.Translations.FirstOrDefault(t => t.LanguageId == languageId);

            // Map basic fields
            if (model is Models.RoutedContent routedModel)
            {
                Module.Mapper.Map<Content, Models.RoutedContent>(content, routedModel);

                // Map content translation
                if (translation != null)
                {
                    Module.Mapper.Map<ContentTranslation, Models.RoutedContent>(translation, routedModel);
                }
            }
            else
            {
                Module.Mapper.Map<Content, Models.Content>(content, model);

                // Map content translation
                if (translation != null)
                {
                    Module.Mapper.Map<ContentTranslation, Models.Content>(translation, model);
                }
            }

            // Map regions
            foreach (var regionType in type.Regions)
            {
                var fields = content.Fields.Where(f => f.RegionId == regionType.Id).OrderBy(f => f.SortOrder).ToList();

                if (!regionType.Collection)
                {
                    foreach (var fieldType in regionType.Fields)
                    {
                        var field = fields.FirstOrDefault(f => f.FieldId == fieldType.Id && f.SortOrder == 0);

                        if (field != null)
                        {
                            if (regionType.Fields.Count == 1)
                            {
                                SetSimpleValue(model, regionType.Id, field, languageId);
                            }
                            else
                            {
                                SetComplexValue(model, regionType.Id, fieldType.Id, field, languageId);
                            }
                        }
                    }
                }
                else
                {
                    var fieldCount = content.Fields.Where(f => f.RegionId == regionType.Id).Select(f => f.SortOrder).DefaultIfEmpty(-1).Max() + 1;
                    var sortOrder = 0;

                    while (fieldCount > sortOrder)
                    {
                        if (regionType.Fields.Count == 1)
                        {
                            var field = fields.SingleOrDefault(f => f.FieldId == regionType.Fields[0].Id && f.SortOrder == sortOrder);
                            if (field != null)
                                AddSimpleValue(model, regionType.Id, field, languageId);
                        }
                        else
                        {
                            await AddComplexValueAsync(model, type, regionType.Id, fields.Where(f => f.SortOrder == sortOrder).ToList(), languageId)
                                .ConfigureAwait(false);
                        }
                        sortOrder++;
                    }
                }
            }

            // Map blocks
            if (model is Models.IBlockContent blockModel)
            {
                // TODO: Map blocks
            }

            // Map category
            if (model is Models.ICategorizedContent categorizedModel)
            {
                // TODO: Map category
            }

            // Map tags
            if (model is Models.ITaggedContent taggedContent)
            {
                // TODO: Map tags
            }
            return model;
        }

        /// <summary>
        /// Transforms the given model into content data.
        /// </summary>
        /// <param name="db">The current db context</param>
        /// <param name="model">The model</param>
        /// <param name="type">The conten type</param>
        /// <param name="languageId">The selected language id</param>
        /// <param name="dest">The optional dest object</param>
        /// <returns>The content data</returns>
        public async Task<Content> ToContentAsync<T>(IDb db, T model, Models.ContentType type, Guid languageId, Content dest = null)
            where T : Models.Content
        {
            var content = dest != null ? dest : new Content();

            // Make sure we have an Id
            if (model.Id != Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }

            // Make sure we have a translation
            var translation = content.Translations.FirstOrDefault(t => t.LanguageId == languageId);
            if (translation == null)
            {
                translation = new ContentTranslation
                {
                    ContentId = model.Id,
                    LanguageId = languageId
                };
                content.Translations.Add(translation);

                await db.ContentTranslations.AddAsync(translation).ConfigureAwait(false);
            }

            // Map basic fields
            if (model is Models.RoutedContent routedModel)
            {
                Module.Mapper.Map<Models.RoutedContent, Content>(routedModel, content);
                Module.Mapper.Map<Models.RoutedContent, ContentTranslation>(routedModel, translation);
            }
            else
            {
                Module.Mapper.Map<Models.Content, Content>(model, content);
                Module.Mapper.Map<Models.Content, ContentTranslation>(model, translation);
            }

            // Map regions
            foreach (var regionType in type.Regions)
            {
                // Make sure the model has data for the region
                if (!HasRegion(model, regionType.Id)) continue;

                if (!regionType.Collection)
                {
                    await MapRegionAsync(db, content, GetRegion(model, regionType.Id), regionType, regionType.Id, languageId).ConfigureAwait(false);
                }
                else
                {
                    var items = new List<Guid>();
                    var sortOrder = 0;
                    foreach (var region in GetListRegion(model, regionType.Id))
                    {
                        var fields = await MapRegionAsync(db, content, region, regionType, regionType.Id, languageId, sortOrder++).ConfigureAwait(false);

                        if (fields.Count > 0)
                            items.AddRange(fields);
                    }

                    // Now delete removed collection items
                    var removedFields = content.Fields
                        .Where(f => f.RegionId == regionType.Id && !items.Contains(f.Id))
                        .ToList();

                    foreach (var removed in removedFields)
                    {
                        content.Fields.Remove(removed);
                    }
                }
            }

            // Map blocks
            if (model is Models.IBlockContent blockModel)
            {
                // TODO: Map blocks
            }

            // Map category
            if (model is Models.ICategorizedContent categorizedModel)
            {
                // TODO: Map category
            }

            // Map tags
            if (model is Models.ITaggedContent taggedContent)
            {
                // TODO: Map tags
            }
            return content;
        }

        /// <summary>
        /// Sets the value of a single field region.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="regionId">The region id</param>
        /// <param name="field">The field</param>
        /// <param name="languageId">The selected language id</param>
        private void SetSimpleValue(Models.Content model, string regionId, ContentField field, Guid languageId)
        {
            if (model is Models.IDynamicContent dynamicModel)
            {
                ((IDictionary<string, object>)dynamicModel.Regions)[regionId] =
                    DeserializeField(field, languageId);
            }
            else
            {
                var regionProp = model.GetType().GetProperty(regionId, App.PropertyBindings);

                if (regionProp != null)
                {
                    regionProp.SetValue(model, DeserializeField(field, languageId));
                }
            }
        }

        /// <summary>
        /// Sets the value of a complex region.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="regionId">The region id</param>
        /// <param name="fieldId">The field id</param>
        /// <param name="field">The field</param>
        /// <param name="languageId">The selected language id</param>
        private void SetComplexValue(Models.Content model, string regionId, string fieldId, ContentField field, Guid languageId)
        {
            if (model is Models.IDynamicContent dynamicModel)
            {
                ((IDictionary<string, object>)((IDictionary<string, object>)dynamicModel.Regions)[regionId])[fieldId] =
                    DeserializeField(field, languageId);
            }
            else
            {
                var regionProp = model.GetType().GetProperty(regionId, App.PropertyBindings);

                if (regionProp != null)
                {
                    var obj = regionProp.GetValue(model);
                    if (obj != null)
                    {
                        var fieldProp = obj.GetType().GetProperty(fieldId, App.PropertyBindings);

                        if (fieldProp != null)
                        {
                            fieldProp.SetValue(obj, DeserializeField(field, languageId));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a single field value to a collection region.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="regionId">The region id</param>
        /// <param name="field">The field</param>
        /// <param name="languageId">The selected language id</param>
        private void AddSimpleValue(Models.Content model, string regionId, ContentField field, Guid languageId)
        {
            if (model is Models.IDynamicContent dynamicModel)
            {
                ((IList)((IDictionary<string, object>)dynamicModel.Regions)[regionId]).Add(
                    DeserializeField(field, languageId));
            }
            else
            {
                var regionProp = model.GetType().GetProperty(regionId, App.PropertyBindings);

                if (regionProp != null)
                {
                    ((IList)regionProp.GetValue(model)).Add(DeserializeField(field, languageId));
                }
            }
        }

        /// <summary>
        /// Adds a complex region to a collection region.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="contentType">The content type</param>
        /// <param name="regionId">The region id</param>
        /// <param name="fields">The field</param>
        /// <param name="languageId">The selected language id</param>
        private async Task AddComplexValueAsync(Models.Content model, Models.ContentType contentType, string regionId, IList<ContentField> fields, Guid languageId)
        {
            // Make sure we have fields
            if (fields.Count == 0) return;

            if (model is Models.IDynamicContent dynamicModel)
            {
                var list = (IList)((IDictionary<string, object>)dynamicModel.Regions)[regionId];
                var obj = await _factory.CreateDynamicRegionAsync(contentType, regionId).ConfigureAwait(false);

                foreach (var field in fields)
                {
                    if (((IDictionary<string, object>)obj).ContainsKey(field.FieldId))
                    {
                        ((IDictionary<string, object>)obj)[field.FieldId] =
                            DeserializeField(field, languageId);
                    }
                }
                list.Add(obj);
            }
            else
            {
                var regionProp = model.GetType().GetProperty(regionId, App.PropertyBindings);

                if (regionProp != null)
                {
                    var list = (IList)regionProp.GetValue(model);
                    var obj = Activator.CreateInstance(list.GetType().GenericTypeArguments.First());

                    foreach (var field in fields)
                    {
                        var fieldProp = obj.GetType().GetProperty(field.FieldId, App.PropertyBindings);
                        if (fieldProp != null)
                        {
                            fieldProp.SetValue(obj, DeserializeField(field, languageId));
                        }
                    }
                    list.Add(obj);
                }
            }
        }

        /// <summary>
        /// Maps a region to the given data entity.
        /// </summary>
        /// <param name="db">The current db context</param>
        /// <param name="content">The content entity</param>
        /// <param name="region">The region to map</param>
        /// <param name="regionType">The region type</param>
        /// <param name="regionId">The region id</param>
        /// <param name="languageId">The selected language id</param>
        /// <param name="sortOrder">The optional sort order</param>
        private async Task<IList<Guid>> MapRegionAsync(IDb db, Content content, object region, Models.ContentTypeRegion regionType, string regionId, Guid languageId, int sortOrder = 0)
        {
            var items = new List<Guid>();

            // Now map all of the fields
            foreach (var fieldType in regionType.Fields)
            {
                var type = App.Fields.GetById(fieldType.Type);

                object fieldValue = null;

                if (regionType.Fields.Count == 1)
                {
                    fieldValue = region;
                }
                else
                {
                    fieldValue = GetComplexValue(region, fieldType.Id);
                }

                if (fieldValue != null)
                {
                    // Check that the returned value matches the type specified
                    // for the page type, otherwise deserialization won't work
                    // when the model is retrieved from the database.
                    if (fieldValue.GetType() != type.Type)
                        throw new ArgumentException("Given field value does not match the configured type");

                    // Check if we have the current field in the database already
                    var field = content.Fields
                        .SingleOrDefault(f => f.RegionId == regionId && f.FieldId == type.Id && f.SortOrder == sortOrder);

                    // If not, create a new field
                    if (field == null)
                    {
                        field = new ContentField
                        {
                            Id = Guid.NewGuid(),
                            ContentId = content.Id,
                            RegionId = regionId,
                            FieldId = fieldType.Id
                        };
                        content.Fields.Add(field);

                        await db.ContentFields.AddAsync(field).ConfigureAwait(false);
                    }

                    // Update field info
                    field.TypeId = fieldType.Id;
                    field.SortOrder = sortOrder;

                    if (type.IsTranslatable)
                    {
                        // Make sure we have a translation available
                        var translation = field.Translations.FirstOrDefault(t => t.LanguageId == languageId);
                        if (translation == null)
                        {
                            translation = new ContentFieldTranslation
                            {
                                FieldId = field.Id,
                                LanguageId = languageId
                            };
                            field.Translations.Add(translation);

                            await db.ContentFieldTranslations.AddAsync(translation).ConfigureAwait(false);
                        }
                        field.Value = null;
                        translation.Value = App.SerializeObject(fieldValue, type.Type);
                    }
                    else
                    {
                        field.Value = App.SerializeObject(fieldValue, type.Type);
                    }
                    items.Add(field.Id);
                }
            }
            return items;
        }

        /// <summary>
        /// Checks if the given model has a region with the specified id.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="model">The model</param>
        /// <param name="regionId">The region id</param>
        /// <returns>If the region exists</returns>
        private bool HasRegion<T>(T model, string regionId) where T : Models.Content
        {
            if (model is Models.IDynamicContent dynamicModel)
            {
                return ((IDictionary<string, object>)dynamicModel.Regions).ContainsKey(regionId);
            }
            else
            {
                return model.GetType().GetProperty(regionId, App.PropertyBindings) != null;
            }
        }

        /// <summary>
        /// Gets the region with the given key.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="model">The model</param>
        /// <param name="regionId">The region id</param>
        /// <returns>The region</returns>
        private object GetRegion<T>(T model, string regionId) where T : Models.Content
        {
            if (model is Models.IDynamicContent dynamicModel)
            {
                return ((IDictionary<string, object>)dynamicModel.Regions)[regionId];
            }
            else
            {
                return model.GetType().GetProperty(regionId, App.PropertyBindings).GetValue(model);
            }
        }

        /// <summary>
        /// Gets the enumerator for the given region collection.
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="model">The model</param>
        /// <param name="regionId">The region id</param>
        /// <returns>The enumerator</returns>
        private IEnumerable GetListRegion<T>(T model, string regionId) where T : Models.Content
        {
            object value = null;

            if (model is Models.IDynamicContent dynamicModel)
            {
                value = ((IDictionary<string, object>)dynamicModel.Regions)[regionId];
            }
            else
            {
                value = model.GetType().GetProperty(regionId, App.PropertyBindings).GetValue(model);
            }
            if (value is IEnumerable)
                return (IEnumerable)value;
            return null;
        }

        /// <summary>
        /// Gets a field value from a complex region.
        /// </summary>
        /// <param name="region">The region</param>
        /// <param name="fieldId">The field id</param>
        /// <returns>The value</returns>
        private object GetComplexValue(object region, string fieldId)
        {
            if (region is ExpandoObject)
            {
                return ((IDictionary<string, object>)region)[fieldId];
            }
            else
            {
                var fieldProp = region.GetType().GetProperty(fieldId, App.PropertyBindings);

                if (fieldProp != null)
                {
                    return fieldProp.GetValue(region);
                }
            }
            return null;
        }


        /// <summary>
        /// Deserializes the given field value.
        /// </summary>
        /// <param name="field">The page field</param>
        /// <param name="languageId">The selected language id</param>
        /// <returns>The value</returns>
        private object DeserializeField(ContentField field, Guid languageId)
        {
            var type = App.Fields.GetByType(field.TypeId);

            if (type != null)
            {
                if (type.IsTranslatable)
                {
                    var translation = field.Translations.FirstOrDefault(f => f.LanguageId == languageId);

                    if (translation != null)
                    {
                        return App.DeserializeObject(translation.Value, type.Type);
                    }
                }
                else
                {
                    return App.DeserializeObject(field.Value, type.Type);
                }
            }
            return null;
        }
    }
}
