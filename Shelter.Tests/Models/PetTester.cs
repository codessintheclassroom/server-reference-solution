using Shelter.Models;
using Shelter.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Shelter.Tests.Models
{
    public class PetTester : IModelTester<Pet>
    {
        public string Plural => "pets";

        public string Singular => "pet";

        public Pet Create()
        {
            var rand = new Random();

            return new Pet
            {
                Id = Guid.NewGuid().ToString("N"),
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

        public void AssertEqual(Pet expected, Pet current, bool ignoreId = true)
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

        public string GetId(Pet model) => model.Id;
    }
}
