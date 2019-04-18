using Microsoft.AspNetCore.Mvc.Testing;
using Shelter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Shelter.Tests.Controllers
{
    public abstract class GenericCrudControllerTests<TModel, TView> : IClassFixture<WebApplicationFactory<Startup>>
        where TView : IModelView<TModel>, new()
    {
        public GenericCrudControllerTests(WebApplicationFactory<Startup> factory, IModelRenderer<TModel, TView> renderer, string version, string singular, string plural)
        {
            this.Factory = factory;
            this.Renderer = renderer;
            this.Version = version;
            this.Singular = singular;
            this.Plural = plural;
        }

        protected WebApplicationFactory<Startup> Factory { get; }

        public IModelRenderer<TModel, TView> Renderer { get; }

        public string Version { get; }

        public string Singular { get; }

        public string Plural { get; }

        public Uri SingularEndpoint(string id) => new Uri($"/api/{Version}/{Singular}/{id}", UriKind.Relative);

        public Uri PluralEndpoint() => new Uri($"/api/{Version}/{Plural}", UriKind.Relative);

        protected abstract TModel CreateModel();

        protected abstract string ModelId(TModel model);

        protected virtual async Task<TModel> StoreModelAsync(TModel model)
        {
            var view = Renderer.ToView(model);

            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync(PluralEndpoint(), view);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<TView>();
            return Renderer.FromView(result);
        }

        protected virtual async Task<string> FoundId()
        {
            return ModelId(await StoreModelAsync(CreateModel()));
        }

        protected virtual Task<string> NotFoundId()
        {
            return Task.FromResult(ModelId(CreateModel()));
        }

        protected abstract void AssertModelEqual(TModel expected, TModel current, bool ignoreId = true);

        [Theory]
        [InlineData("this is not json", "text/plain", HttpStatusCode.UnsupportedMediaType)]
        [InlineData("this is not valid json", "application/json", HttpStatusCode.BadRequest)]
        [InlineData(@"{ ""invalid"": ""field"" }", "application/json", HttpStatusCode.BadRequest)]
        public async Task TestCreateBadData(string content, string mimeType, HttpStatusCode responseCode)
        {
            var client = Factory.CreateClient();

            var response = await client.PostAsync(PluralEndpoint(), new StringContent(content, System.Text.Encoding.UTF8, mimeType));
            Assert.Equal(responseCode, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestCreate()
        {
            var client = Factory.CreateClient();

            var model = CreateModel();
            var view = Renderer.ToView(model);

            var response = await client.PostAsJsonAsync(PluralEndpoint(), view);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var returnedView = await response.Content.ReadAsAsync<TView>();
            var returnedModel = Renderer.FromView(returnedView);

            AssertModelEqual(model, returnedModel, ignoreId: true);

            Assert.Equal(SingularEndpoint(ModelId(returnedModel)), response.Headers.Location);
        }

        [Theory]
        [InlineData("this is not json", "text/plain", HttpStatusCode.UnsupportedMediaType)]
        [InlineData("this is not valid json", "application/json", HttpStatusCode.BadRequest)]
        [InlineData(@"{ ""invalid"": ""field"" }", "application/json", HttpStatusCode.BadRequest)]
        public async Task TestModifyBadData(string content, string mimeType, HttpStatusCode responseCode)
        {
            var client = Factory.CreateClient();

            var response = await client.PutAsync(SingularEndpoint(await FoundId()), new StringContent(content, System.Text.Encoding.UTF8, mimeType));
            Assert.Equal(responseCode, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestModifyNotFound()
        {
            var client = Factory.CreateClient();

            var modification = Renderer.ToView(CreateModel());

            var response = await client.PutAsJsonAsync(SingularEndpoint(await NotFoundId()), modification);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestModify()
        {
            var client = Factory.CreateClient();

            var modificationModel = CreateModel();
            var modification = Renderer.ToView(modificationModel);

            var id = await FoundId();

            var response = await client.PutAsJsonAsync(SingularEndpoint(id), modification);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var modified = await response.Content.ReadAsAsync<TView>();
            var modifiedModel = Renderer.FromView(modified);
            AssertModelEqual(modificationModel, modifiedModel, ignoreId: true);
        }

        [Fact]
        public async Task TestList()
        {
            var client = Factory.CreateClient();

            var ids = new[]
            {
                await FoundId(),
                await FoundId()
            };

            var response = await client.GetAsync(PluralEndpoint());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var views = await response.Content.ReadAsAsync<IEnumerable<TView>>();
            var models = views.Select(view => Renderer.FromView(view)).ToArray();

            Assert.All(ids, id => models.Any(model => ModelId(model) == id));
        }

        [Fact]
        public async Task TestGet()
        {
            var client = Factory.CreateClient();

            var id = await FoundId();

            var response = await client.GetAsync(SingularEndpoint(id));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var view = await response.Content.ReadAsAsync<TView>();
            var model = Renderer.FromView(view);
            Assert.Equal(id, ModelId(model));
        }

        [Fact]
        public async Task TestGetNotFound()
        {
            var client = Factory.CreateClient();

            var response = await client.GetAsync(SingularEndpoint(await NotFoundId()));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestNoModifyByPost()
        {
            var client = Factory.CreateClient();

            var originalId = await FoundId();

            // First get the original value (pre-modification)
            var response = await client.GetAsync(SingularEndpoint(originalId));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var originalView = await response.Content.ReadAsAsync<TView>();

            // Then modify the entry in the database the normal way
            response = await client.PutAsJsonAsync(SingularEndpoint(originalId), CreateModel());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var modifiedView = await response.Content.ReadAsAsync<TView>();

            // Then finally try and create the same original view again (with its ID duplicated)
            var createResponse = await client.PostAsJsonAsync(PluralEndpoint(), originalView);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var createdView = await createResponse.Content.ReadAsAsync<TView>();
            AssertModelEqual(Renderer.FromView(originalView), Renderer.FromView(createdView), ignoreId: true);
            Assert.NotEqual(ModelId(Renderer.FromView(modifiedView)), ModelId(Renderer.FromView(createdView)));

        }
    }
}
