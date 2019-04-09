namespace Shelter.Store
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDatastore<TModel>
    {
        Task<TModel> GetAsync(string id);

        Task<IEnumerable<TModel>> ListAsync();

        Task<TModel> StoreAsync(TModel item);

        Task<bool> RemoveAsync(TModel item);
    }
}