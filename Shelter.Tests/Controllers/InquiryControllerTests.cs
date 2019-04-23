using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Shelter.Models;
using Shelter.Tests.Models;
using Xunit;

namespace Shelter.Tests.Controllers
{
    public class InquiryControllerTests : GenericCrudControllerTests<Inquiry, Inquiry.InquiryV1>
    {
        public InquiryControllerTests(WebApplicationFactory<Startup> factory)
            : base(factory, new Inquiry.InquiryV1.Renderer(), new InquiryTester(), "v1")
        {
            NeedsAuthForCreate = true;
            NeedsAuthForGet = true;
            NeedsAuthForList = true;
            NeedsAuthForModify = true;
        }
    }
}