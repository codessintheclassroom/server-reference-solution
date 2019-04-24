using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shelter.Config;
using Shelter.Models;
using Shelter.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shelter.Services
{
    public class SeedService : IHostedService
    {
        private readonly IOptions<Seed> config;
        private readonly IDatastore<Pet> petStore;
        private readonly IModelRenderer<Pet, Pet.PetV1> petRenderer;
        private readonly IDatastore<Inquiry> inquiryStore;
        private readonly IModelRenderer<Inquiry, Inquiry.InquiryV1> inquiryRenderer;

        public SeedService(
            IOptions<Seed> config,
            IDatastore<Pet> petStore,
            IModelRenderer<Pet, Pet.PetV1> petRenderer,
            IDatastore<Inquiry> inquiryStore,
            IModelRenderer<Inquiry, Inquiry.InquiryV1> inquiryRenderer)
        {
            this.config = config;
            this.petStore = petStore;
            this.petRenderer = petRenderer;
            this.inquiryStore = inquiryStore;
            this.inquiryRenderer = inquiryRenderer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var pet in config.Value.Pets)
                await this.petStore.StoreAsync(petRenderer.FromView(pet));

            foreach (var inquiry in config.Value.Inquiries)
                await this.inquiryStore.StoreAsync(inquiryRenderer.FromView(inquiry));
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
