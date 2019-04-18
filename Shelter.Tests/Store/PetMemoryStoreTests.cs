using System;
using System.Threading.Tasks;
using Shelter.Models;
using Shelter.Store;
using Xunit;

namespace Shelter.Tests.Store
{
    public class PetMemoryStoryTests : GenericDatastoreTests<Pet, PetMemoryStore>
    {
        protected override Pet CreateModel(bool nullId = true)
        {
            var rand = new Random();

            return new Pet
            {
                Id = nullId ? null : Guid.NewGuid().ToString("N"),
                Name = "Testo",
                Kind = "dog",
                Status = PetStatus.Available,
                Breed = "Testi",
                Description = "This is a test!",
                Birthday = new DateTime(1970, 1, 1),
                Photos = new[] {
                    new Uri($"https://test.com/test_{rand.Next()}.png")
                }
            };
        }

        protected override void AssertModelEqual(Pet expected, Pet current, bool ignoreId = true)
        {
            if (!ignoreId)
                Assert.Equal(expected.Id, current.Id);

            Assert.Equal(expected.Name, current.Name);
            Assert.Equal(expected.Kind, current.Kind);
            Assert.Equal(expected.Status, current.Status);
            Assert.Equal(expected.Breed, current.Breed);
            Assert.Equal(expected.Description, current.Description);
            Assert.Equal(expected.Birthday, current.Birthday);
            Assert.Equal(expected.Photos, current.Photos);
        }

        protected override string ModelId(Pet model) => model.Id;
    }
}