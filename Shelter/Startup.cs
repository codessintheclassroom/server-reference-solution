using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shelter.Config;
using Shelter.Models;
using Shelter.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shelter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddApiVersioning();

            var authSchemes = new HashSet<string>();

            switch (Configuration.GetValue<string>("Authentication:Mode"))
            {
                case "AzureAD":
                    authSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    authSchemes.Add(AzureADDefaults.JwtBearerAuthenticationScheme);
                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddAzureADBearer(JwtBearerDefaults.AuthenticationScheme, AzureADDefaults.JwtBearerAuthenticationScheme, opts =>
                        {
                            Configuration.Bind("Authentication:AzureAD", opts);
                        });

                        services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, opts =>
                        {
                            Configuration.Bind("Authentication:JwtBearer", opts.TokenValidationParameters);
                        });

                    break;
                case "JwtBearer":
                default:
                    authSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    services.AddAuthentication()
                        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
                        {
                            Configuration.Bind("Authentication:JwtBearer", opts.TokenValidationParameters);

                            opts.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Configuration.GetValue<string>("Authentication:JwtBearer:SigningKey")));

                            opts.Validate();
                        });
                    break;
            }

            services.AddAuthorization(opts =>
            {
                var scopes = new List<Config.AuthPolicy>();
                Configuration.Bind("Authentication:Policies", scopes);
                foreach(var scope in scopes)
                {
                    opts.AddPolicy(scope.Name, p =>
                    {
                        p.RequireAssertion(ctx =>
                            ctx.User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope" && c.Value.Split(" ").Contains(scope.Claim)));
                        p.RequireAssertion(ctx => 
                            scope.Roles.Any(role => ctx.User.IsInRole(role)));
                        p.AddAuthenticationSchemes(authSchemes.ToArray());
                    });
                }
            });

            services.AddSingleton<IModelRenderer<Pet, Pet.PetV1>, Pet.PetV1.Renderer>();
            services.AddSingleton<IModelRenderer<Inquiry, Inquiry.InquiryV1>, Inquiry.InquiryV1.Renderer>();

            services.AddSingleton<IDatastore<Pet>, PetMemoryStore>();
            services.AddSingleton<IDatastore<Inquiry>, InquiryMemoryStore>();

            services.Configure<Seed>(Configuration.GetSection("Seed"));
            services.AddHostedService<Services.SeedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseApiVersioning().UseMvc();
        }
    }
}
