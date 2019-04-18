using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shelter.Models;
using Shelter.Store;

namespace Shelter.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    public class PetV1Controller : PetController<Pet.PetV1>
    {
        public PetV1Controller(IDatastore<Pet> store, IModelRenderer<Pet, Pet.PetV1> renderer)
            : base(store, renderer)
        {

        }
    }
}
