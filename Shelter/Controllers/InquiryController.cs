using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelter.Models;
using Shelter.Store;

namespace Shelter.Controllers
{
    public abstract class InquiryController<TView> : ControllerBase
        where TView : IModelView<Inquiry>, new()
    {
        public InquiryController(IDatastore<Inquiry> store, IModelRenderer<Inquiry, TView> renderer)
        {
            Store = store;
            Renderer = renderer;
        }

        protected IDatastore<Inquiry> Store { get; }

        protected IModelRenderer<Inquiry, TView> Renderer { get; }

        // GET api/v1/inquiries
        [HttpGet]
        [Route("api/v{version:apiVersion}/inquiries")]
        [Authorize("Inquiries.Read")]
        public virtual async Task<ActionResult<IEnumerable<TView>>> GetList()
        {
            var inquiries = await Store.ListAsync();
            return Ok(inquiries.Select(inquiry => Renderer.ToView(inquiry)));
        }

        // POST api/v1/inquiries
        [HttpPost]
        [Route("api/v{version:apiVersion}/inquiries")]
        public virtual async Task<ActionResult<TView>> Create([FromBody]TView pet)
        {
            var model = Renderer.FromView(pet);
            model.Id = null;

            var newInquiry = await Store.StoreAsync(model);

            return Created(Url.Action("GetInquiry", new
            {
                version = this.RouteData.Values["version"],
                id = newInquiry.Id
            }), Renderer.ToView(newInquiry));
        }

        // GET api/v1/inquiry/{id}
        [HttpGet]
        [Route("api/v{version:apiVersion}/inquiry/{id}")]
        [ActionName("GetInquiry")]
        [Authorize("Inquiries.Read")]
        public virtual async Task<ActionResult<TView>> Get(string id)
        {
            var inquiry = await Store.GetAsync(id);
            if (inquiry == null)
                return NotFound();

            return Renderer.ToView(inquiry);
        }

        // PUT api/v1/inquiry/{id}
        [HttpPut]
        [Route("api/v{version:apiVersion}/inquiry/{id}")]
        [Authorize("Inquiries.Write")]
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
