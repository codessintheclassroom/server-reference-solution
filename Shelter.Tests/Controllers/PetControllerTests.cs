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
    public class PetControllerTests : GenericCrudControllerTests<Pet, Pet.PetV1>
    {
        public PetControllerTests(WebApplicationFactory<Startup> factory)
            : base(factory, new Pet.PetV1.Renderer(), new PetTester(), "v1")
        {
            NeedsAuthForCreate = true;
            NeedsAuthForModify = true;
        }
    }
}