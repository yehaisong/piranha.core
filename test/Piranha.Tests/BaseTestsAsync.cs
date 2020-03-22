/*
 * Copyright (c) 2017 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Piranha.Data.EF.SQLite;
using Piranha.ImageSharp;
using Piranha.Repositories;
using Piranha.Services;

namespace Piranha.Tests
{
    /// <summary>
    /// Base class for using the api.
    /// </summary>
    public abstract class BaseTestsAsync : IAsyncLifetime
    {
        protected IStorage storage = new Local.FileStorage("uploads/", "~/uploads/");
        protected readonly IImageProcessor processor = new ImageSharpProcessor();
        protected ICache cache;
        protected IServiceProvider services = new ServiceCollection()
            .BuildServiceProvider();

        public abstract Task InitializeAsync();
        public abstract Task DisposeAsync();

        /// <summary>
        /// Gets the test context.
        /// </summary>
        protected IDb GetDb() {
            var builder = new DbContextOptionsBuilder<SQLiteDb>();

            builder.UseSqlite("Filename=./piranha.tests.db");

            return new SQLiteDb(builder.Options);
        }

        protected virtual IApi CreateApi()
        {
            var factory = new LegacyContentFactory(services);
            var contentFactory = new ContentFactory(services);
            var serviceFactory = new LegacyContentServiceFactory(factory);

            var db = GetDb();

            return new Api(
                factory,
                contentFactory,
                new AliasRepository(db),
                new ArchiveRepository(db),
                new ContentRepository(db, new TransformationService(contentFactory)),
                new ContentGroupRepository(db),
                new ContentTypeRepository(db),
                new LanguageRepository(db),
                new Piranha.Repositories.MediaRepository(db),
                new PageRepository(db, serviceFactory),
                new PageTypeRepository(db),
                new ParamRepository(db),
                new PostRepository(db, serviceFactory),
                new PostTypeRepository(db),
                new SiteRepository(db, serviceFactory),
                new SiteTypeRepository(db),
                storage: storage,
                processor: processor,
                cache: cache
            );
        }
    }
}
