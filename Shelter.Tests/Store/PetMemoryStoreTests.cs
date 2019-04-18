using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Shelter.Models;
using Shelter.Store;
using Xunit;

namespace Shelter.Tests.Store
{
    public class PetMemoryStoryTests
    : IClassFixture<WebApplicationFactory<Shelter.Startup>>
    {
        public PetMemoryStoryTests()
        {

        }

        [Fact]
        public async Task TestStoreNewId()
        {
            var store = new PetMemoryStore();

            var stored = await store.StoreAsync(new Pet
            {
                Name = "Testo",
                Breed = "Testlet",
                Kind = "Test",
                Birthday = new DateTime(1970, 1, 1),
                Status = PetStatus.Available,
                Description = "This is a test!",
                Photos = new[] { new Uri("http://test.com/test.png") }
            });

            Assert.False(string.IsNullOrEmpty(stored.Id));
            Assert.Equal(stored, await store.GetAsync(stored.Id));
        }

        [Fact]
        public async Task TestStoreExistingId()
        {
            var store = new PetMemoryStore();

            var id = Guid.NewGuid().ToString("N");

            var stored = await store.StoreAsync(new Pet
            {
                Id = id,
                Name = "Testo",
                Breed = "Testlet",
                Kind = "Test",
                Birthday = new DateTime(1970, 1, 1),
                Status = PetStatus.Available,
                Description = "This is a test!",
                Photos = new[] { new Uri("http://test.com/test.png") }
            });

            Assert.Equal(id, stored.Id);
            Assert.Equal(stored, await store.GetAsync(stored.Id));
        }

        [Fact]
        public async Task TestListEmpty()
        {
            var store = new PetMemoryStore();

            Assert.Empty(await store.ListAsync());
        }

        [Fact]
        public async Task TestStoreAndList()
        {
            var store = new PetMemoryStore();

            Assert.Empty(await store.ListAsync());
            var stored = await store.StoreAsync(new Pet
            {
                Name = "Testo",
                Breed = "Testlet",
                Kind = "Test",
                Birthday = new DateTime(1970, 1, 1),
                Status = PetStatus.Available,
                Description = "This is a test!",
                Photos = new[] { new Uri("http://test.com/test.png") }
            });

            Assert.NotEmpty(await store.ListAsync());
            Assert.Contains(stored, await store.ListAsync());
        }

        [Fact]
        public async Task TestGetNotFound()
        {
            var store = new PetMemoryStore();

            Assert.Null(await store.GetAsync("000000000000000000000000"));
        }

        [Fact]
        public async Task TestStoreAndGet()
        {
            var store = new PetMemoryStore();

            var stored = await store.StoreAsync(new Pet
            {
                Name = "Testo",
                Breed = "Testlet",
                Kind = "Test",
                Birthday = new DateTime(1970, 1, 1),
                Status = PetStatus.Available,
                Description = "This is a test!",
                Photos = new[] { new Uri("http://test.com/test.png") }
            });

            Assert.Equal(stored, await store.GetAsync(stored.Id));
        }

        [Fact]
        public async Task TestRemoveNotFound()
        {
            var store = new PetMemoryStore();

            Assert.False(await store.RemoveAsync("000000000000000000000000"));
        }

        [Fact]
        public async Task TestStoreAndRemove()
        {
            var store = new PetMemoryStore();

            var stored = await store.StoreAsync(new Pet
            {
                Name = "Testo",
                Breed = "Testlet",
                Kind = "Test",
                Birthday = new DateTime(1970, 1, 1),
                Status = PetStatus.Available,
                Description = "This is a test!",
                Photos = new[] { new Uri("http://test.com/test.png") }
            });

            Assert.True(await store.RemoveAsync(stored.Id));
            Assert.Null(await store.GetAsync(stored.Id));
        }
    }
}