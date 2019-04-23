using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shelter.Models;
using Shelter.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Shelter.Tests.Controllers
{
    public abstract class GenericCrudControllerTests<TModel, TView> : IClassFixture<WebApplicationFactory<Startup>>
        where TView : IModelView<TModel>, new()
    {
        public GenericCrudControllerTests(WebApplicationFactory<Startup> factory, IModelRenderer<TModel, TView> renderer, IModelTester<TModel> model, string version)
        {
            this.Factory = factory;
            this.Renderer = renderer;
            this.Model = model;
            this.Version = version;
        }

        protected WebApplicationFactory<Startup> Factory { get; }

        public IModelRenderer<TModel, TView> Renderer { get; }

        public string Version { get; }

        public Uri SingularEndpoint(string id) => new Uri($"/api/{Version}/{Model.Singular}/{id}", UriKind.Relative);

        public Uri PluralEndpoint() => new Uri($"/api/{Version}/{Model.Plural}", UriKind.Relative);
        
        public IModelTester<TModel> Model { get; set; }


        public bool SupportsList { get; set; } = true;

        public bool NeedsAuthForList { get; set; } = false;

        public bool SupportsGet { get; set; } = true;

        public bool NeedsAuthForGet { get; set; } = false;

        public bool SupportsCreate { get; set; } = true;

        public bool NeedsAuthForCreate { get; set; } = false;

        public bool SupportsModify { get; set; } = true;

        public bool NeedsAuthForModify { get; set; } = false;

        public bool SupportsDelete { get; set; } = false;

        public bool NeedsAuthForDelete { get; set; } = false;

        protected virtual async Task<AuthenticationHeaderValue> GetAuthHeaderAsync(HttpMethod method, Uri endpoint)
        {
            var config = Factory.Server.Host.Services.GetRequiredService<IConfiguration>();

            var signingKey = Convert.FromBase64String(config.GetValue<string>("Authentication:JwtBearer:SigningKey"));
            var tokenHandler = new JwtSecurityTokenHandler();

            var securityKey = new SymmetricSecurityKey(signingKey);
            var token = ConfigureAuthToken(new JwtSecurityToken(signingCredentials: new SigningCredentials(securityKey, "HS256")), method, endpoint);

            var tokenText = tokenHandler.WriteToken(token);

            return new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, tokenText);
        }

        protected virtual JwtSecurityToken ConfigureAuthToken(JwtSecurityToken token, HttpMethod method, Uri endpoint)
        {
            var config = Factory.Server.Host.Services.GetRequiredService<IConfiguration>();

            var policies = new List<Config.AuthPolicy>();
            config.Bind("Authentication:Policies", policies);

            var roles = policies.Select(p => p.Roles).Aggregate(new HashSet<string>(), (l, r) => new HashSet<string>(l.Concat(r))).ToArray();
            var scopes = policies.Select(p => p.Claim).ToArray();

            return new JwtSecurityToken(
                audience: "test",
                issuer: "Test Suite",
                claims: new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sid, "test"),
                        new Claim(JwtRegisteredClaimNames.Sub, "Test User"),
                        new Claim(ClaimTypes.Name, "Test User"),
                    }
                    .Concat(scopes.Select(s => new Claim("http://schemas.microsoft.com/identity/claims/scope", s)))
                    .Concat(roles.Select(r => new Claim("roles", r))),
                expires: DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)),
                notBefore: DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(30)),
                signingCredentials: token.SigningCredentials);
        }

        protected virtual async Task<TModel> StoreModelAsync(TModel model)
        {
            var view = Renderer.ToView(model);

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = NeedsAuthForCreate ? await GetAuthHeaderAsync(HttpMethod.Post, PluralEndpoint()) : null;

            var response = await client.PostAsJsonAsync(PluralEndpoint(), view);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<TView>();
            return Renderer.FromView(result);
        }

        protected virtual async Task<string> FoundId()
        {
            return Model.GetId(await StoreModelAsync(Model.Create()));
        }

        protected virtual Task<string> NotFoundId()
        {
            return Task.FromResult(Model.GetId(Model.Create()));
        }

        [SkippableTheory]
        [InlineData("this is not json", "text/plain", HttpStatusCode.UnsupportedMediaType)]
        [InlineData("this is not valid json", "application/json", HttpStatusCode.BadRequest)]
        [InlineData(@"{ ""invalid"": ""field"" }", "application/json", HttpStatusCode.BadRequest)]
        public async Task TestCreateBadData(string content, string mimeType, HttpStatusCode responseCode)
        {
            Skip.IfNot(SupportsCreate, $"The POST {PluralEndpoint()} is not supported by this controller");

            var client = Factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = NeedsAuthForCreate ? await GetAuthHeaderAsync(HttpMethod.Post, PluralEndpoint()) : null;

            var response = await client.PostAsync(PluralEndpoint(), new StringContent(content, System.Text.Encoding.UTF8, mimeType));
            Assert.Equal(responseCode, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [SkippableFact]
        public async Task TestCreate()
        {
            Skip.IfNot(SupportsCreate, $"The POST {PluralEndpoint()} is not supported by this controller");

            var client = Factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = NeedsAuthForCreate ? await GetAuthHeaderAsync(HttpMethod.Post, PluralEndpoint()) : null;

            var model = Model.Create();
            var view = Renderer.ToView(model);

            var response = await client.PostAsJsonAsync(PluralEndpoint(), view);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var returnedView = await response.Content.ReadAsAsync<TView>();
            var returnedModel = Renderer.FromView(returnedView);

            Model.AssertEqual(model, returnedModel, ignoreId: true);

            Assert.Equal(SingularEndpoint(Model.GetId(returnedModel)), response.Headers.Location);
        }

        [SkippableTheory]
        [InlineData("this is not json", "text/plain", HttpStatusCode.UnsupportedMediaType)]
        [InlineData("this is not valid json", "application/json", HttpStatusCode.BadRequest)]
        [InlineData(@"{ ""invalid"": ""field"" }", "application/json", HttpStatusCode.BadRequest)]
        public async Task TestModifyBadData(string content, string mimeType, HttpStatusCode responseCode)
        {
            Skip.IfNot(SupportsModify, $"The PUT {SingularEndpoint("{id}")} is not supported by this controller");

            var id = await FoundId();

            var client = Factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = NeedsAuthForModify ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(id)) : null;

            var response = await client.PutAsync(SingularEndpoint(id), new StringContent(content, System.Text.Encoding.UTF8, mimeType));
            Assert.Equal(responseCode, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [SkippableFact]
        public async Task TestModifyNotFound()
        {
            Skip.IfNot(SupportsModify, $"The PUT {SingularEndpoint("{id}")} is not supported by this controller");
            var id = await NotFoundId();

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = NeedsAuthForModify ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(id)) : null;

            var modification = Renderer.ToView(Model.Create());

            var response = await client.PutAsJsonAsync(SingularEndpoint(id), modification);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [SkippableFact]
        public async Task TestModify()
        {
            Skip.IfNot(SupportsModify, $"The PUT {SingularEndpoint("{id}")} is not supported by this controller");

            var id = await FoundId();

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = NeedsAuthForModify ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(id)) : null;

            var modificationModel = Model.Create();
            var modification = Renderer.ToView(modificationModel);

            var response = await client.PutAsJsonAsync(SingularEndpoint(id), modification);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var modified = await response.Content.ReadAsAsync<TView>();
            var modifiedModel = Renderer.FromView(modified);
            Model.AssertEqual(modificationModel, modifiedModel, ignoreId: true);
        }

        [SkippableFact]
        public async Task TestList()
        {
            Skip.IfNot(SupportsList, $"The GET {PluralEndpoint()} is not supported by this controller");

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = NeedsAuthForList ? await GetAuthHeaderAsync(HttpMethod.Get, PluralEndpoint()) : null;

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

            Assert.All(ids, id => models.Any(model => Model.GetId(model) == id));
        }

        [SkippableFact]
        public async Task TestGet()
        {
            Skip.IfNot(SupportsGet, $"The GET {SingularEndpoint("{id}")} is not supported by this controller");

            var id = await FoundId();

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = NeedsAuthForGet ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(id)) : null;

            var response = await client.GetAsync(SingularEndpoint(id));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var view = await response.Content.ReadAsAsync<TView>();
            var model = Renderer.FromView(view);
            Assert.Equal(id, Model.GetId(model));
        }

        [SkippableFact]
        public async Task TestGetNotFound()
        {
            Skip.IfNot(SupportsGet, $"The GET {SingularEndpoint("{id}")} is not supported by this controller");

            var id = await NotFoundId();

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = NeedsAuthForGet ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(id)) : null;

            var response = await client.GetAsync(SingularEndpoint(id));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [SkippableFact]
        public async Task TestNoModifyByPost()
        {
            Skip.IfNot(SupportsGet, $"The GET {SingularEndpoint("{id}")} is not supported by this controller");
            Skip.IfNot(SupportsModify, $"The PUT {SingularEndpoint("{id}")} is not supported by this controller");
            Skip.IfNot(SupportsCreate, $"The POST {PluralEndpoint()} is not supported by this controller");

            var originalId = await FoundId();

            var client = Factory.CreateClient();

            // First get the original value (pre-modification)
            client.DefaultRequestHeaders.Authorization = NeedsAuthForGet ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(originalId)) : null;
            var response = await client.GetAsync(SingularEndpoint(originalId));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var originalView = await response.Content.ReadAsAsync<TView>();

            // Then modify the entry in the database the normal way
            client.DefaultRequestHeaders.Authorization = NeedsAuthForModify ? await GetAuthHeaderAsync(HttpMethod.Put, SingularEndpoint(originalId)) : null;
            response = await client.PutAsJsonAsync(SingularEndpoint(originalId), Model.Create());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var modifiedView = await response.Content.ReadAsAsync<TView>();

            // Then finally try and create the same original view again (with its ID duplicated)
            client.DefaultRequestHeaders.Authorization = NeedsAuthForCreate ? await GetAuthHeaderAsync(HttpMethod.Post, PluralEndpoint()) : null;
            var createResponse = await client.PostAsJsonAsync(PluralEndpoint(), originalView);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            var createdView = await createResponse.Content.ReadAsAsync<TView>();
            Model.AssertEqual(Renderer.FromView(originalView), Renderer.FromView(createdView), ignoreId: true);
            Assert.NotEqual(Model.GetId(Renderer.FromView(modifiedView)), Model.GetId(Renderer.FromView(createdView)));

        }
    }
}
