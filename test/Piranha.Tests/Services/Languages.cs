/*
 * Copyright (c) 2017-2020 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Piranha.AttributeBuilder;
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;
using Piranha.Repositories;
using Piranha.Services;

namespace Piranha.Tests.Repositories
{
    [Collection("Integration tests")]
    public class LanguagesCached : Languages
    {
        public override async Task InitializeAsync()
        {
            cache = new Cache.SimpleCache();

            await base.InitializeAsync();
        }
    }

    [Collection("Integration tests")]
    public class Languages : BaseTestsAsync
    {
        private const string LANG_1 = "MyFirstLanguage";
        private const string LANG_2 = "MySecondLanguage";
        private const string LANG_3 = "MyThirdLanguage";
        private const string LANG_4 = "MyFourthLanguage";
        private const string LANG_5 = "MyFifthLanguage";
        private const string LANG_6 = "MySixthLanguage";

        private readonly Guid LANG_1_ID = Guid.NewGuid();

        public override async Task InitializeAsync()
        {
            using (var api = CreateApi())
            {
                Piranha.App.Init(api);

                var def = await api.Languages.GetDefaultAsync();
                if (def != null)
                {
                    await api.Languages.DeleteAsync(def);
                }

                await api.Languages.SaveAsync(new Language
                {
                    Id = LANG_1_ID,
                    Title = LANG_1,
                    Slug = "lang1",
                    IsDefault = true
                });

                await api.Languages.SaveAsync(new Language
                {
                    Title = LANG_4,
                    Slug = "lang4",
                });
                await api.Languages.SaveAsync(new Language
                {
                    Title = LANG_5,
                    Slug = "lang5",
                });
                await api.Languages.SaveAsync(new Language
                {
                    Title = LANG_6,
                    Slug = "lang6",
                });
            }
        }

        public override async Task DisposeAsync()
        {
            using (var api = CreateApi())
            {
                var languages = await api.Languages.GetAllAsync();
                foreach (var lang in languages.Where(l => !l.IsDefault))
                {
                    await api.Languages.DeleteAsync(lang);
                }
            }
        }

        [Fact]
        public void IsCached()
        {
            using (var api = CreateApi()) {
                Assert.Equal(this.GetType() == typeof(LanguagesCached), ((Api)api).IsCached);
            }
        }

        [Fact]
        public async Task Add()
        {
            using (var api = CreateApi())
            {
                await api.Languages.SaveAsync(new Language
                {
                    Title = LANG_2,
                    Slug = "lang2"
                });
            }
        }

        [Fact]
        public async Task AddDuplicateKey()
        {
            using (var api = CreateApi())
            {
                await Assert.ThrowsAnyAsync<ValidationException>(async () =>
                    await api.Languages.SaveAsync(new Language
                    {
                        Title = LANG_1,
                        Slug = "lang1"
                    }));
            }
        }

        [Fact]
        public async Task AddEmptyFailure()
        {
            using (var api = CreateApi())
            {
                await Assert.ThrowsAnyAsync<ValidationException>(async () =>
                    await api.Languages.SaveAsync(new Language()));
            }
        }

        [Fact]
        public async Task AddAndGenerateSlug()
        {
            var id = Guid.NewGuid();

            using (var api = CreateApi())
            {
                await api.Languages.SaveAsync(new Language
                {
                    Id = id,
                    Title = "Generate slug"
                });

                var lang = await api.Languages.GetByIdAsync(id);

                Assert.NotNull(lang);
                Assert.Equal("generate-slug", lang.Slug);
            }
        }

        [Fact]
        public async Task GetAll()
        {
            using (var api = CreateApi())
            {
                var models = await api.Languages.GetAllAsync();

                Assert.NotNull(models);
                Assert.NotEmpty(models);
            }
        }

        [Fact]
        public async Task GetNoneById()
        {
            using (var api = CreateApi())
            {
                var none = await api.Languages.GetByIdAsync(Guid.NewGuid());

                Assert.Null(none);
            }
        }

        [Fact]
        public async Task GetNoneBySlug()
        {
            using (var api = CreateApi())
            {
                var none = await api.Languages.GetBySlugAsync("none-existing-slug");

                Assert.Null(none);
            }
        }

        [Fact]
        public async Task GetById()
        {
            using (var api = CreateApi())
            {
                var model = await api.Languages.GetByIdAsync(LANG_1_ID);

                Assert.NotNull(model);
                Assert.Equal(LANG_1, model.Title);
            }
        }

        [Fact]
        public async Task GetBySlug()
        {
            using (var api = CreateApi())
            {
                var model = await api.Languages.GetBySlugAsync("lang1");

                Assert.NotNull(model);
                Assert.Equal(LANG_1, model.Title);
            }
        }

        [Fact]
        public async Task GetDefault()
        {
            using (var api = CreateApi())
            {
                var model = await api.Languages.GetDefaultAsync();

                Assert.NotNull(model);
                Assert.Equal(LANG_1, model.Title);
            }
        }

        [Fact]
        public async Task Delete()
        {
            using (var api = CreateApi())
            {
                var model = await api.Languages.GetBySlugAsync("lang4");

                Assert.NotNull(model);

                await api.Languages.DeleteAsync(model);
            }
        }

        [Fact]
        public async Task DeleteById()
        {
            using (var api = CreateApi())
            {
                var model = await api.Languages.GetBySlugAsync("lang5");

                Assert.NotNull(model);

                await api.Languages.DeleteAsync(model.Id);
            }
        }
    }
}
