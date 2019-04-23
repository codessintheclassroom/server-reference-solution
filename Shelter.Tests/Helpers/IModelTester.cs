using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shelter.Tests.Helpers
{
    public interface IModelTester<TModel>
    {
        TModel Create();

        string GetId(TModel model);

        void AssertEqual(TModel expected, TModel current, bool ignoreId = true);

        string Plural { get; }

        string Singular { get; }
    }
}
