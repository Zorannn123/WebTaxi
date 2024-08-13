using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enum;
using Common.DTO;
using Common.TableStorage;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class User
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
        public TypeOfUser UserType { get; set; }
        [DataMember]
        public string? Image { get; set; }
        [DataMember]
        public Verification Verification { get; set; }

        public User(UserEntity user)
        {
            Email = user.Email;
            UserName = user.UserName;
            Password = user.Password;
            Address = user.Address;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DateOfBirth = user.DateOfBirth;
            UserType = user.UserType;
            Image = user.Image;
            Verification = user.Verification;
        }

        public User(RegisterDto user)
        {
            Email = user.Email;
            UserName = user.UserName;
            Password = user.Password;
            Address = user.Address;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DateOfBirth = user.DateOfBirth;
            if (user.Role == TypeOfUser.Driver.ToString())
            {
                UserType = TypeOfUser.Driver;
            }
            else
            {
                UserType = TypeOfUser.User;
            }
            Image = user.Image;
            Verification = (UserType == TypeOfUser.User) ? Verification.Approved : Verification.OnHold;
        }

    }
}
