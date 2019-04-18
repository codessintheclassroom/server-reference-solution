using System;
using System.Threading.Tasks;
using Shelter.Models;
using Shelter.Store;
using Xunit;

namespace Shelter.Tests.Store
{
    public abstract class GenericDatastoreTests<TModel, TDatastore>
        where TDatastore : IDatastore<TModel>, new()
    {
        protected abstract TModel CreateModel(bool nullId = true);

        protected abstract string ModelId(TModel model);

        protected abstract void AssertModelEqual(TModel expected, TModel current, bool ignoreId = true);

        [Fact]
        public async Task TestStoreNewId()
        {
            var store = new TDatastore();

            var stored = await store.StoreAsync(CreateModel());

            Assert.False(string.IsNullOrEmpty(ModelId(stored)));
            AssertModelEqual(stored, await store.GetAsync(ModelId(stored)));
        }

        [Fact]
        public async Task TestStoreExistingId()
        {
            var store = new TDatastore();

            var model = CreateModel();

            var stored = await store.StoreAsync(model);

            AssertModelEqual(model, stored, true);
            AssertModelEqual(stored, await store.GetAsync(ModelId(stored)), false);
        }

        [Fact]
        public async Task TestListEmpty()
        {
            var store = new TDatastore();

            Assert.Empty(await store.ListAsync());
        }

        [Fact]
        public async Task TestStoreAndList()
        {
            var store = new TDatastore();

            Assert.Empty(await store.ListAsync());
            var stored = await store.StoreAsync(CreateModel());

            Assert.NotEmpty(await store.ListAsync());
            Assert.Contains(stored, await store.ListAsync());
        }

        [Fact]
        public async Task TestGetNotFound()
        {
            var store = new TDatastore();

            Assert.Null(await store.GetAsync(ModelId(CreateModel(false))));
        }

        [Fact]
        public async Task TestStoreAndGet()
        {
            var store = new TDatastore();

            var stored = await store.StoreAsync(CreateModel());

            AssertModelEqual(stored, await store.GetAsync(ModelId(stored)));
        }

        [Fact]
        public async Task TestRemoveNotFound()
        {
            var store = new TDatastore();

            Assert.False(await store.RemoveAsync(ModelId(CreateModel(false))));
        }

        [Fact]
        public async Task TestStoreAndRemove()
        {
            var store = new TDatastore();

            var stored = await store.StoreAsync(CreateModel());

            Assert.True(await store.RemoveAsync(ModelId(stored)));
            Assert.Null(await store.GetAsync(ModelId(stored)));
        }
    }
}