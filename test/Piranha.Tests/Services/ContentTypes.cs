/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/piranhacms/piranha
 *
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;
using Piranha.Extend.Fields;
using Piranha.Models;
using Piranha.Repositories;
using Piranha.Services;

namespace Piranha.Tests.Services
{
    [Collection("Integration tests")]
    public class ContentTypesCached : ContentTypes
    {
        public override Task InitializeAsync()
        {
            cache = new Cache.SimpleCache();

            return base.InitializeAsync();
        }
    }

    [Collection("Integration tests")]
    public class ContentTypes : BaseTestsAsync
    {
        protected ICache cache;
        private readonly List<ContentType> contentTypes = new List<ContentType>
        {
            new ContentType
            {
                Id = "MyFirstType",
                Title = "My First Type",
                Group = "Test",
                Regions = new List<ContentTypeRegion>
                {
                    new ContentTypeRegion
                    {
                        Id = "Body",
                        Fields = new List<ContentTypeField>
                        {
                            new ContentTypeField
                            {
                                Id = "Default",
                                Type = typeof(HtmlField).FullName,
                            }
                        }
                    }
                }
            },
            new ContentType
            {
                Id = "MySecondType",
                Title = "My Second Type",
                Group = "Test",
                Regions = new List<ContentTypeRegion>
                {
                    new ContentTypeRegion
                    {
                        Id = "Body",
                        Fields = new List<ContentTypeField>
                        {
                            new ContentTypeField
                            {
                                Id = "Default",
                                Type = typeof(TextField).FullName
                            }
                        }
                    }
                }
            },
            new ContentType
            {
                Id = "MyThirdType",
                Title = "My Third Type",
                Group = "Test",
                Regions = new List<ContentTypeRegion>
                {
                    new ContentTypeRegion
                    {
                        Id = "Body",
                        Fields = new List<ContentTypeField>
                        {
                            new ContentTypeField
                            {
                                Id = "Default",
                                Type = typeof(ImageField).FullName
                            }
                        }
                    }
                }
            },
            new ContentType
            {
                Id = "MyFourthType",
                Title = "My Fourth Type",
                Group = "Test",
                Regions = new List<ContentTypeRegion>
                {
                    new ContentTypeRegion
                    {
                        Id = "Body",
                        Fields = new List<ContentTypeField>
                        {
                            new ContentTypeField
                            {
                                Id = "Default",
                                Type = typeof(StringField).FullName
                            }
                        }
                    }
                }
            },
            new ContentType
            {
                Id = "MyFifthType",
                Title = "My Fifth Type",
                Group = "Test",
                Regions = new List<ContentTypeRegion>
                {
                    new ContentTypeRegion
                    {
                        Id = "Body",
                        Fields = new List<ContentTypeField>
                        {
                            new ContentTypeField
                            {
                                Id = "Default",
                                Type = typeof(TextField).FullName
                            }
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Initializes the test data.
        /// </summary>
        public override async Task InitializeAsync()
        {
            var service = CreateService();

            await service.SaveAsync(contentTypes[0]);
            await service.SaveAsync(contentTypes[3]);
            await service.SaveAsync(contentTypes[4]);
        }

        /// <summary>
        /// Disposes and removes the created test data.
        /// </summary>
        public override async Task DisposeAsync()
        {
            var service = CreateService();

            var types = await service.GetAllAsync();

            foreach (var t in types)
            {
                await service.DeleteAsync(t);
            }
        }

        [Fact]
        public async Task Add()
        {
            var service = CreateService();

            await service.SaveAsync(contentTypes[1]);
        }

        [Fact]
        public async Task AddFailsWithoutId()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Title = "My Failed Type",
                    Group = "Test"
                });
            });
        }

        [Fact]
        public async Task AddFailsWithoutTitle()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "MyFailedType",
                    Group = "Test"
                });
            });
        }

        [Fact]
        public async Task AddFailsWithoutGroup()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "MyFailedType",
                    Title = "My Failed Type",
                });
            });
        }

        [Fact]
        public async Task AddFailsWithTooLongId()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "ThisIdIsByFarSoMuchLongerThan64CharactersSoItShouldReallyFailWhenValidating",
                    Title = "My Failed Type",
                    Group = "Test"
                });
            });
        }

        [Fact]
        public async Task AddFailsWithTooLongGroup()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "MyFailedType",
                    Title = "My Failed Type",
                    Group = "ThisGroupIsByFarSoMuchLongerThan64CharactersSoItShouldReallyFailWhenValidating"
                });
            });
        }

        [Fact]
        public async Task AddFailsWithoutRegionId()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "MyFailedType",
                    Title = "My Failed Type",
                    Group = "Test",
                    Regions = new List<ContentTypeRegion>
                    {
                        new ContentTypeRegion
                        {
                            Fields = new List<ContentTypeField>
                            {
                                new ContentTypeField
                                {
                                    Id = "Default",
                                    Type = typeof(ImageField).FullName
                                }
                            }
                        }
                    }
                });
            });
        }

        [Fact]
        public async Task AddFailsWithoutFieldId()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "MyFailedType",
                    Title = "My Failed Type",
                    Group = "Test",
                    Regions = new List<ContentTypeRegion>
                    {
                        new ContentTypeRegion
                        {
                            Id = "Body",
                            Fields = new List<ContentTypeField>
                            {
                                new ContentTypeField
                                {
                                    Type = typeof(ImageField).FullName
                                }
                            }
                        }
                    }
                });
            });
        }

        [Fact]
        public async Task AddFailsWithoutFieldType()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await service.SaveAsync(new ContentType
                {
                    Id = "MyFailedType",
                    Title = "My Failed Type",
                    Group = "Test",
                    Regions = new List<ContentTypeRegion>
                    {
                        new ContentTypeRegion
                        {
                            Id = "Body",
                            Fields = new List<ContentTypeField>
                            {
                                new ContentTypeField
                                {
                                    Id = "Default"
                                }
                            }
                        }
                    }
                });
            });
        }

        [Fact]
        public async Task GetAll()
        {
            var service = CreateService();

            var models = await service.GetAllAsync();

            Assert.NotNull(models);
            Assert.NotEmpty(models);
        }

        [Fact]
        public async Task GetNoneById()
        {
            var service = CreateService();

            var none = await service.GetByIdAsync("none-existing-type");

            Assert.Null(none);
        }

        [Fact]
        public async Task GetById()
        {
            var service = CreateService();

            var model = await service.GetByIdAsync(contentTypes[0].Id);

            Assert.NotNull(model);
            Assert.Equal(contentTypes[0].Regions[0].Fields[0].Id, model.Regions[0].Fields[0].Id);
        }

        [Fact]
        public async Task Update()
        {
            var service = CreateService();

            var model = await service.GetByIdAsync(contentTypes[0].Id);

            Assert.Equal("My First Type", model.Title);

            model.Title = "Updated";

            await service.SaveAsync(model);
        }

        [Fact]
        public async Task Delete()
        {
            var service = CreateService();

            var model = await service.GetByIdAsync(contentTypes[3].Id);

            Assert.NotNull(model);

            await service.DeleteAsync(model);
        }

        [Fact]
        public async Task DeleteById()
        {
            var service = CreateService();

            var model = await service.GetByIdAsync(contentTypes[4].Id);

            Assert.NotNull(model);

            await service.DeleteAsync(model.Id);
        }

        private IContentTypeService CreateService()
        {
            return new ContentTypeService(new ContentTypeRepository(GetDb()), cache);
        }
    }
}
