using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Shelter.Models;
using Xunit;

namespace Shelter.Tests.Controllers
{
    public class InquiryControllerTests : GenericCrudControllerTests<Inquiry, Inquiry.InquiryV1>
    {
        public InquiryControllerTests(WebApplicationFactory<Startup> factory)
            : base(factory, new Inquiry.InquiryV1.Renderer(), "v1", "inquiry", "inquiries")
        {
        }

        protected override Inquiry CreateModel()
        {
            var rand = new Random();

            return new Inquiry
            {
                Id = Guid.NewGuid().ToString("N"),
                PetId = Guid.NewGuid().ToString("N"),
                Name = "Testy McTesterson",
                Email = "test@mctest.me",
                Message = $"We need more tests! We only have {rand.Next(100)} of the {rand.Next(100, 100000)} we need!"
            };
        }

        protected override void AssertModelEqual(Inquiry expected, Inquiry current, bool ignoreId = true)
        {
            if (!ignoreId)
                Assert.Equal(expected.Id, current.Id);

            Assert.Equal(expected.PetId, current.PetId);
            Assert.Equal(expected.Name, current.Name);
            Assert.Equal(expected.Email, current.Email);
            Assert.Equal(expected.Message, current.Message);
        }

        protected override string ModelId(Inquiry model) => model.Id;
    }
}