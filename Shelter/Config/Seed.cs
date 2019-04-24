using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shelter.Config
{
    public class Seed
    {
        public List<Models.Pet.PetV1> Pets { get; set; } = new List<Models.Pet.PetV1>();

        public List<Models.Inquiry.InquiryV1> Inquiries { get; set; } = new List<Models.Inquiry.InquiryV1>();
    }
}
