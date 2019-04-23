using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shelter.Config
{
    public class AuthPolicy
    {
        public string Name { get; set; }

        public string Claim { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
