/*
 * Copyright (c) 2017 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Piranha.Data.EF.SQLite;
using Piranha.Repositories;
using Piranha.Services;

namespace Piranha.ImageSharp.Tests
{
    /// <summary>
    /// Base class for using the api.
    /// </summary>
    public abstract class BaseTests : IDisposable
    {
        protected readonly IStorage storage = new Local.FileStorage("uploads/", "~/uploads/");
        protected IServiceProvider services = new ServiceCollection().BuildServiceProvider();
        protected readonly IImageProcessor processor = new ImageSharpProcessor();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseTests() {
            Init();
        }

        /// <summary>
        /// Disposes the test class.
        /// </summary>
        public void Dispose() {
            Cleanup();
        }

        /// <summary>
        /// Sets up & initializes the tests.
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// Cleans up any possible data and resources
        /// created by the test.
        /// </summary>
        protected abstract void Cleanup();

        /// <summary>
        /// Gets the test context.
        /// </summary>
        protected virtual IDb GetDb() {
            var builder = new DbContextOptionsBuilder<SQLiteDb>();

            builder.UseSqlite("Filename=./piranha.imagesharp.tests.db");

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
                new Piranha.Repositories.MediaRepository(db),
                new PageRepository(db, serviceFactory),
                new PageTypeRepository(db),
                new ParamRepository(db),
                new PostRepository(db, serviceFactory),
                new PostTypeRepository(db),
                new SiteRepository(db, serviceFactory),
                new SiteTypeRepository(db),
                storage: storage,
                processor: processor
            );
        }
    }
}
