using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Common.DTO;
using Common.Blob;
using System.Security.Cryptography;
using System.Net;
using Common.Models;
using Azure.Data.Tables;
using Common.Interface;
using Common.TableStorage;
using System.Text;
using System.Drawing;
using Common.Enum;
using System.Fabric.Management.ServiceModel;
using System.Fabric.Description;

namespace UserService
{
    internal sealed class UserService : StatefulService, IUser
    {
        private TableClient usersTable = null;
        private Thread usersTableThread = null;
        private IReliableDictionary<string, User> usersDictionary = null;


        public UserService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SetTableAsync();
            usersDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UsersDictionary");
            await FillUserDictionary();
            usersTableThread = new Thread(new ThreadStart(UsersTableThread));
            usersTableThread.Start();
        }

        private async Task SetTableAsync()
        {
            var tsClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tsClient.CreateTableIfNotExistsAsync("User");
            usersTable = tsClient.GetTableClient("User");
        }

        private async Task FillUserDictionary()
        {
            var items = usersTable.QueryAsync<UserEntity>(x => true).GetAsyncEnumerator();

            using (var transaction = StateManager.CreateTransaction())
            {
                if (items.Current != null)
                {
                    while (await items.MoveNextAsync())
                    {
                        var user = new User(items.Current);
                        await usersDictionary.TryAddAsync(transaction, user.Email, user);
                    }
                    await transaction.CommitAsync();
                }
                else
                {
                    transaction.Abort();
                }
            }
        }
        private async void UsersTableThread()
        {
            while (true)
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    var temp = (await usersDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();
                    while(await temp.MoveNextAsync(CancellationToken.None))
                    {
                        var user = temp.Current.Value;
                        var userEntity = new UserEntity(user);
                        await usersTable.UpsertEntityAsync(userEntity, TableUpdateMode.Merge, CancellationToken.None);
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public static string HashPassword(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("a7Y8F2Pph9Dp1B5dM4k4/A==");

            using (var sha256 = new SHA256Managed())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];

                // Concatenate password and salt
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Hash the concatenated password and salt
                byte[] hashedBytes = sha256.ComputeHash(saltedPassword);

                // Concatenate the salt and hashed password for storage
                byte[] hashedPasswordWithSalt = new byte[hashedBytes.Length + salt.Length];
                Buffer.BlockCopy(salt, 0, hashedPasswordWithSalt, 0, salt.Length);
                Buffer.BlockCopy(hashedBytes, 0, hashedPasswordWithSalt, salt.Length, hashedBytes.Length);

                return Convert.ToBase64String(hashedPasswordWithSalt);
            }
        }


        public async Task<bool> LoginAsync(LoginDto logDto)
        {
            if (logDto == null || string.IsNullOrWhiteSpace(logDto.Email) || string.IsNullOrWhiteSpace(logDto.Password))
            {
                throw new ArgumentException("Invalid login data.");
            }
            
            using (var transaction = StateManager.CreateTransaction())
                {
                    var userResult = await usersDictionary.TryGetValueAsync(transaction, logDto.Email);

                    if (userResult.HasValue && !string.IsNullOrEmpty(userResult.Value.Password))
                    {
                        string hashedPassword = HashPassword(logDto.Password);

                        if (hashedPassword.Equals(userResult.Value.Password))
                        {
                            return true; 
                        }
                    }
                }
            

            return false; // Failed login
        }

        public async Task<bool> RegisterAsync(RegisterDto regDto)
        {
            bool result = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var userExists = await usersDictionary.TryGetValueAsync(transaction, regDto.Email);

                if (!userExists.HasValue)
                {
                    var newUser = new User(regDto)
                    {
                        Password = HashPassword(regDto.Password),
                        Image = UploadUserImage(regDto.Image)
                    };

                    try
                    {
                        await usersDictionary.AddAsync(transaction, regDto.Email, newUser);

                        await transaction.CommitAsync();
                        result = true;
                    }
                    catch (Exception)
                    {
                        transaction.Abort();
                        result = false;
                    }
                }
            }

            return result;
        }

        private string UploadUserImage(string base64Image)
        {
            try
            {
                // Ensure the Base64 string is properly formatted
                var imageData = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;

                // Convert Base64 string to byte array
                using (var ms = new MemoryStream(Convert.FromBase64String(imageData)))
                {
                    // Create an image from the byte array
                    using (var image = Image.FromStream(ms))
                    {
                        // Upload the image to blob storage and return the URL or path
                        return new Blob().UploadImage(new Bitmap(image), Guid.NewGuid().ToString() + ".jpg");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return a default or empty string
                // Log.Error("Error uploading image", ex);
                return string.Empty;
            }
        }


        public async Task<UserDto> GetCurrentUserAsync(string email)
        {
            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, email);
                if (currUser.HasValue)
                {
                    User user = currUser.Value;
                    UserDto result = new()
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        Password = user.Password,
                        Address = user.Address,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DateOfBirth = user.DateOfBirth,
                        Role = user.UserType.ToString(),
                        Image = user.Image
                    };
                    return result;
                }
                return null;
            }
        }

        public async Task<bool> EditProfileAsync(UserDto userDto)
        {
            bool temp = false;
            using (var transaction = StateManager.CreateTransaction())
            {
                var currProfile = await usersDictionary.TryGetValueAsync(transaction, userDto.Email);
                if (currProfile.HasValue)
                {
                    User currUserCredentials = currProfile.Value;

                    User newUserCredentials = currUserCredentials;

                    if (userDto.FirstName != null)
                    {
                        newUserCredentials.FirstName = userDto.FirstName;
                    }

                    if (userDto.LastName != null)
                    {
                        newUserCredentials.LastName = userDto.LastName;
                    }

                    if (userDto.Address != null)
                    {
                        newUserCredentials.Address = userDto.Address;
                    }

                    if (userDto.UserName != null)
                    {
                        newUserCredentials.UserName = userDto.UserName;
                    }

                    if (userDto.DateOfBirth != null)
                    {
                        newUserCredentials.DateOfBirth = userDto.DateOfBirth;
                    }
                    if (userDto.Password != null)
                    {
                        newUserCredentials.Password = HashPassword(userDto.Password);
                    }
                    if (!string.IsNullOrEmpty(userDto.Image))
                    {
                        string imageUrl = UploadUserImage(userDto.Image);
                        newUserCredentials.Image = imageUrl;
                    }


                    try
                    {
                        await usersDictionary.TryUpdateAsync(transaction, userDto.Email, newUserCredentials, currUserCredentials);
                        await transaction.CommitAsync();
                        temp = true;
                    }catch(Exception ex)
                    {
                        temp = false;
                        transaction.Abort();
                    }

                }
                else
                {
                    temp = false;
                }

            }
            return temp;
        }
    }
}
