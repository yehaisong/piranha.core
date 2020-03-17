/*
 * Copyright (c) 2018-2020 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System;
using AutoMapper;
using Piranha.Data.EF;

namespace Piranha.Services
{
    public class LegacyContentServiceFactory : ILegacyContentServiceFactory
    {
        private readonly ILegacyContentFactory _factory;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="factory">The content factory</param>
        public LegacyContentServiceFactory(ILegacyContentFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Creates a new content service for the specified types.
        /// </summary>
        /// <param name="mapper">The AutoMapper instance to use for transformation</param>
        /// <returns>The content service</returns>
        public ILegacyContentService<TContent, TField, TModelBase> Create<TContent, TField, TModelBase>(IMapper mapper)
            where TContent : Data.ContentBase<TField>
            where TField : Data.ContentFieldBase
            where TModelBase : Models.ContentBase
        {
            return new LegacyContentService<TContent, TField, TModelBase>(_factory, mapper);
        }

        /// <summary>
        /// Creates a new page content service.
        /// </summary>
        /// <returns>The content service</returns>
        public ILegacyContentService<Data.Page, Data.PageField, Models.PageBase> CreatePageService()
        {
            return new LegacyContentService<Data.Page, Data.PageField, Models.PageBase>(_factory, Module.Mapper);
        }

        /// <summary>
        /// Creates a new post content service.
        /// </summary>
        /// <returns>The content service</returns>
        public ILegacyContentService<Data.Post, Data.PostField, Models.PostBase> CreatePostService()
        {
            return new LegacyContentService<Data.Post, Data.PostField, Models.PostBase>(_factory, Module.Mapper);
        }

        /// <summary>
        /// Creates a new site content service.
        /// </summary>
        /// <returns>The content service</returns>
        public ILegacyContentService<Data.Site, Data.SiteField, Models.SiteContentBase> CreateSiteService()
        {
            return new LegacyContentService<Data.Site, Data.SiteField, Models.SiteContentBase>(_factory, Module.Mapper);
        }
    }
}
