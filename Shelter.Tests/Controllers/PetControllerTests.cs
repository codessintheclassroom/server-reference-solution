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
    public class PetControllerTests
    : IClassFixture<WebApplicationFactory<Shelter.Startup>>
    {
        private readonly WebApplicationFactory<Shelter.Startup> _factory;

        public PetControllerTests(WebApplicationFactory<Shelter.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TestGetList()
        {
            var client = _factory.CreateClient();

            // Create a pet so that we can validate that it gets included in our list
            await client.PostAsJsonAsync("/api/v1/pets", new Pet.PetV1
            {
                Name = "Testo",
                Kind = "dog",
                Status = PetStatus.Available,
                Breed = "Testi",
                Description = "This is a test!",
                Birthday = new DateTime(1970, 1, 1),
                Photos = new[] {
                    new Uri("https://test.com/test.png")
                }
            });

            var response = await client.GetAsync("/api/v1/pets");
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var pets = await response.Content.ReadAsAsync<IEnumerable<Pet.PetV1>>();
            Assert.NotEmpty(pets);
        }

        [Fact]
        public async Task TestGet()
        {
            var client = _factory.CreateClient();

            var newPet = new Pet.PetV1
            {
                Name = "Testo",
                Kind = "dog",
                Status = PetStatus.Available,
                Breed = "Testi",
                Description = "This is a test!",
                Birthday = new DateTime(1970, 1, 1),
                Photos = new[] {
                    new Uri("https://test.com/test.png")
                }
            };

            var createResponse = await client.PostAsJsonAsync("/api/v1/pets", newPet);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.Equal("application/json; charset=utf-8", createResponse.Content.Headers.ContentType.ToString());

            var response = await client.GetAsync(createResponse.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", createResponse.Content.Headers.ContentType.ToString());

            var fetchedPet = await response.Content.ReadAsAsync<Pet.PetV1>();
            Assert.Equal(newPet.Name, fetchedPet.Name);
            Assert.Equal(newPet.Kind, fetchedPet.Kind);
            Assert.Equal(newPet.Status, fetchedPet.Status);
            Assert.Equal(newPet.Breed, fetchedPet.Breed);
            Assert.Equal(newPet.Description, fetchedPet.Description);
            Assert.Equal(newPet.Birthday, fetchedPet.Birthday);
            Assert.Equal(newPet.Photos, fetchedPet.Photos);

            response = await client.GetAsync($"/api/v1/pet/{Guid.NewGuid().ToString("N")}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("this is not json", "text/plain", HttpStatusCode.UnsupportedMediaType)]
        [InlineData("this is not valid json", "application/json", HttpStatusCode.BadRequest)]
        [InlineData(@"{ ""status"": ""tested"" }", "application/json", HttpStatusCode.BadRequest)]
        public async Task TestCreateBadData(string content, string mimeType, HttpStatusCode responseCode)
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync("/api/v1/pets", new StringContent(content, System.Text.Encoding.UTF8, mimeType));
            Assert.Equal(responseCode, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestCreateValidData()
        {
            var client = _factory.CreateClient();

            var newPet = new Pet.PetV1
            {
                Name = "Testo",
                Kind = "dog",
                Status = PetStatus.Available,
                Breed = "Testi",
                Description = "This is a test!",
                Birthday = new DateTime(1970, 1, 1),
                Photos = new[] {
                    new Uri("https://test.com/test.png")
                }
            };

            var response = await client.PostAsJsonAsync("/api/v1/pets", newPet);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var createdPet = await response.Content.ReadAsAsync<Pet.PetV1>();
            Assert.Equal(newPet.Name, createdPet.Name);
            Assert.Equal(newPet.Kind, createdPet.Kind);
            Assert.Equal(newPet.Status, createdPet.Status);
            Assert.Equal(newPet.Breed, createdPet.Breed);
            Assert.Equal(newPet.Description, createdPet.Description);
            Assert.Equal(newPet.Birthday, createdPet.Birthday);
            Assert.Equal(newPet.Photos, createdPet.Photos);

            Assert.Equal(new Uri($"/api/v1/pet/{createdPet.Id}", UriKind.Relative), response.Headers.Location);

            var fetchedPet = await client.GetAsync(response.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, fetchedPet.StatusCode);
        }

        [Fact]
        public async Task TestModifyNotFound()
        {
            var client = _factory.CreateClient();

            var newPet = new Pet.PetV1
            {
                Name = "Testo",
                Kind = "dog",
                Status = PetStatus.Available,
                Breed = "Testi",
                Description = "This is a test!",
                Birthday = new DateTime(1970, 1, 1),
                Photos = new[] {
                    new Uri("https://test.com/test.png")
                }
            };

            var response = await client.PutAsJsonAsync("/api/v1/pet/000000000000000000000000", newPet);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestModifyValidData()
        {
            var client = _factory.CreateClient();

            var newPet = new Pet.PetV1
            {
                Name = "Testo",
                Kind = "dog",
                Status = PetStatus.Available,
                Breed = "Testi",
                Description = "This is a test!",
                Birthday = new DateTime(1970, 1, 1),
                Photos = new[] {
                    new Uri("https://test.com/test.png")
                }
            };

            var response = await client.PostAsJsonAsync("/api/v1/pets", newPet);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
            var location = response.Headers.Location;

            newPet.Status = PetStatus.Unavailable;

            response = await client.PutAsJsonAsync(location, newPet);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var modifiedPet = await response.Content.ReadAsAsync<Pet.PetV1>();
            Assert.Equal(newPet.Name, modifiedPet.Name);
            Assert.Equal(newPet.Kind, modifiedPet.Kind);
            Assert.Equal(newPet.Status, modifiedPet.Status);
            Assert.Equal(newPet.Breed, modifiedPet.Breed);
            Assert.Equal(newPet.Description, modifiedPet.Description);
            Assert.Equal(newPet.Birthday, modifiedPet.Birthday);
            Assert.Equal(newPet.Photos, modifiedPet.Photos);
        }
    }
}