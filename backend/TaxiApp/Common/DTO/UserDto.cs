using Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class UserDto
    {
        [DataMember]
        public string? UserName { get; set; }
        [DataMember]
        public string? Email { get; set; }
        [DataMember]
        public string? Password { get; set; }
        [DataMember]
        public string? FirstName { get; set; }
        [DataMember]
        public string? LastName { get; set; }
        [DataMember]
        public string? DateOfBirth { get; set; }
        [DataMember]
        public string? Address { get; set; }
        [DataMember]
        public string? Role { get; set; }
        [DataMember]
        public string? Image { get; set; }
    }
}
