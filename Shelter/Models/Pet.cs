namespace Shelter.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Pet
    {

        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public PetStatus Status { get; set; }

        [Required]
        public string Kind { get; set; }

        public string Breed { get; set; }

        public string Description { get; set; }

        public DateTime Birthday { get; set; }

        public Uri[] Photos { get; set; }

        public class PetV1 : IModelView<Pet>
        {
            public string Id { get; set; }

            [Required]
            public string Name { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public PetStatus Status { get; set; }

            [Required]
            public string Kind { get; set; }

            public string Breed { get; set; }

            public string Description { get; set; }

            public DateTime Birthday { get; set; }

            public Uri[] Photos { get; set; }

            public class Renderer : IModelRenderer<Pet, PetV1>
            {
                public Pet FromView(PetV1 view)
                {
                    return new Pet
                    {
                        Id = view.Id,
                        Name = view.Name,
                        Status = view.Status,
                        Kind = view.Kind,
                        Breed = view.Breed,
                        Description = view.Description,
                        Birthday = view.Birthday,
                        Photos = view.Photos
                    };
                }

                public PetV1 ToView(Pet model)
                {
                    return new PetV1
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Status = model.Status,
                        Kind = model.Kind,
                        Breed = model.Breed,
                        Description = model.Description,
                        Birthday = model.Birthday,
                        Photos = model.Photos
                    };
                }
            }
        }
    }
}