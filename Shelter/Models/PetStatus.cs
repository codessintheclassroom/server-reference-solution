using System.Runtime.Serialization;

namespace Shelter.Models
{
    public enum PetStatus
    {
        [EnumMember(Value = "unavailable")]
        Unavailable,

        [EnumMember(Value = "available")]
        Available,

        [EnumMember(Value = "adopted")]
        Adopted

    }
}