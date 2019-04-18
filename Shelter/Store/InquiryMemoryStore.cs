using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shelter.Models;

namespace Shelter.Store
{
    public class InquiryMemoryStore : IDatastore<Inquiry>
    {
        private ConcurrentDictionary<string, Inquiry> store = new ConcurrentDictionary<string, Inquiry>();

        public Task<Inquiry> GetAsync(string id)
        {
            if (this.store.TryGetValue(id, out var inquiry))
                return Task.FromResult(inquiry);
            return Task.FromResult<Inquiry>(null);
        }

        public Task<IEnumerable<Inquiry>> ListAsync()
        {
            return Task.FromResult((IEnumerable<Inquiry>)this.store.Values.ToArray());
        }

        public Task<bool> RemoveAsync(string id)
        {
            return Task.FromResult(this.store.TryRemove(id, out var pet));
        }

        public Task<Inquiry> StoreAsync(Inquiry item)
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