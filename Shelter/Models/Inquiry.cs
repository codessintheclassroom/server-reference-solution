using System;
using System.ComponentModel.DataAnnotations;

namespace Shelter.Models
{
    public class Inquiry
    {
        public string Id { get; set; }

        [Required]
        public string PetId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string Message { get; set; }

        public class InquiryV1 : IModelView<Inquiry>
        {
            public string Id { get; set; }

            [Required]
            public string PetId { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Email { get; set; }

            public string Message { get; set; }

            public class Renderer : IModelRenderer<Inquiry, InquiryV1>
            {
                public Inquiry FromView(InquiryV1 view)
                {
                    return new Inquiry
                    {
                        Id = view.Id,
                        PetId = view.PetId,
                        Name = view.Name,
                        Email = view.Email,
                        Message = view.Message
                    };
                }
                public InquiryV1 ToView(Inquiry model)
                {
                    return new InquiryV1
                    {
                        Id = model.Id,
                        PetId = model.PetId,
                        Name = model.Name,
                        Email = model.Email,
                        Message = model.Message
                    };
                }
            }
        }
    }
}
