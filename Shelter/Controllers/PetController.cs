using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelter.Models;
using Shelter.Store;

namespace Shelter.Controllers
{
    public abstract class PetController<TView> : ControllerBase
        where TView : IModelView<Pet>, new()
    {
        public PetController(IDatastore<Pet> store, IModelRenderer<Pet, TView> renderer)
        {
            Store = store;
            Renderer = renderer;
        }

        protected IDatastore<Pet> Store { get; }

        protected IModelRenderer<Pet, TView> Renderer { get; }

        // GET api/v1/pets
        [HttpGet]
        [Route("api/v{version:apiVersion}/pets")]
        public virtual async Task<ActionResult<IEnumerable<TView>>> GetList()
        {
            var pets = await Store.ListAsync();
            return Ok(pets.Select(pet => Renderer.ToView(pet)));
        }

        // POST api/v1/pets
        [HttpPost]
        [Authorize("Pets.Write")]
        [Route("api/v{version:apiVersion}/pets")]
        public virtual async Task<ActionResult<TView>> Create([FromBody]TView pet)
        {
            var model = Renderer.FromView(pet);
            model.Id = null;

            var newPet = await Store.StoreAsync(model);

            return Created(Url.Action("GetPet", new
            {
                version = this.RouteData.Values["version"],
                id = newPet.Id
            }), Renderer.ToView(newPet));
        }

        // GET api/v1/pet/{id}
        [HttpGet]
        [Route("api/v{version:apiVersion}/pet/{id}")]
        [ActionName("GetPet")]
        public virtual async Task<ActionResult<TView>> Get(string id)
        {
            var pet = await Store.GetAsync(id);
            if (pet == null)
                return NotFound();

            return Renderer.ToView(pet);
        }

        // PUT api/v1/pet/{id}
        [HttpPut]
        [Authorize("Pets.Write")]
        [Route("api/v{version:apiVersion}/pet/{id}")]
        public virtual async Task<ActionResult<TView>> Modify(string id, [FromBody]TView pet)
        {
            if (await Store.GetAsync(id) == null)
                return NotFound();

            var modification = Renderer.FromView(pet);
            modification.Id = id;

            var modified = await Store.StoreAsync(modification);

            return Renderer.ToView(modified);
        }
    }
}
