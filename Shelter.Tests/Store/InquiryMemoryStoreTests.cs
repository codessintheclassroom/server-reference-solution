using System;
using System.Threading.Tasks;
using Shelter.Models;
using Shelter.Store;
using Xunit;

namespace Shelter.Tests.Store
{
    public class InquiryMemoryStoryTests : GenericDatastoreTests<Inquiry, InquiryMemoryStore>
    {
        protected override Inquiry CreateModel(bool nullId = true)
        {
            var rand = new Random();

            return new Inquiry
            {
                Id = nullId ? null : Guid.NewGuid().ToString("N"),
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