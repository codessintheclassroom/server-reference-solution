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
    public class InquiryV1Controller : InquiryController<Inquiry.InquiryV1>
    {
        public InquiryV1Controller(IDatastore<Inquiry> store, IModelRenderer<Inquiry, Inquiry.InquiryV1> renderer)
            : base(store, renderer)
        {

        }
    }
}
