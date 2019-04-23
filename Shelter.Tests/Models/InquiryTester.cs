using Shelter.Models;
using Shelter.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Shelter.Tests.Models
{
    public class InquiryTester : IModelTester<Inquiry>
    {
        public string Plural => "inquiries";

        public string Singular => "inquiry";

        public Inquiry Create()
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

        public void AssertEqual(Inquiry expected, Inquiry current, bool ignoreId = true)
        {
            if (!ignoreId)
                Assert.Equal(expected.Id, current.Id);

            Assert.Equal(expected.PetId, current.PetId);
            Assert.Equal(expected.Name, current.Name);
            Assert.Equal(expected.Email, current.Email);
            Assert.Equal(expected.Message, current.Message);
        }

        public string GetId(Inquiry model) => model.Id;
    }
}
