using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Common.Enum;
using Common.Models;

namespace Common.TableStorage
{
    public class UserEntity : ITableEntity
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public TypeOfUser UserType { get; set; }
        public string? Image { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public UserEntity()
        {
            PartitionKey = "User";
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;
        }

        public UserEntity(User user) : base()
        {
            RowKey = user.Email;
            UserName = user.UserName;
            Email = user.Email;
            Password = user.Password;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DateOfBirth = user.DateOfBirth;
            Address = user.Address;
            UserType = user.UserType;
            Image = user.Image;
        }
    }
}
