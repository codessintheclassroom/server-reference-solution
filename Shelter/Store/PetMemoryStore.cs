using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shelter.Models;

namespace Shelter.Store
{
    public class PetMemoryStore : IDatastore<Models.Pet>
    {
        private ConcurrentDictionary<string, Models.Pet> store = new ConcurrentDictionary<string, Pet>();

        public Task<Pet> GetAsync(string id)
        {
            if (this.store.TryGetValue(id, out var pet))
                return Task.FromResult(pet);
            return Task.FromResult<Pet>(null);
        }

        public Task<IEnumerable<Pet>> ListAsync()
        {
            return Task.FromResult((IEnumerable<Pet>)this.store.Values.ToArray());
        }

        public Task<bool> RemoveAsync(string id)
        {
            return Task.FromResult(this.store.TryRemove(id, out var pet));
        }

        public Task<Pet> StoreAsync(Pet item)
        {
            if (string.IsNullOrEmpty(item.Id))
                item.Id = GenerateId();

            this.store.AddOrUpdate(item.Id, item, (id, _) => item);

            return Task.FromResult(item);
        }

        private string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}