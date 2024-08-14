using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class DriverDto
    {
        [DataMember]
        public string? UserName { get; set; }
        [DataMember]
        public string? Email { get; set; }

        [DataMember]
        public string? FirstName { get; set; }
        [DataMember]
        public string? LastName { get; set; }
        [DataMember]
        public string? VerifyStatus { get; set; }
        [DataMember]
        public bool? IsBlocked { get; set; }

        public DriverDto(User driver)
        {
            Email = driver.Email;
            UserName = driver.UserName;
            FirstName = driver.FirstName;
            LastName = driver.LastName;
            VerifyStatus = driver.Verification.ToString();
            IsBlocked = driver.IsBlocked;
        }
    }
}
