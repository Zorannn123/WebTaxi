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
using System.Runtime.CompilerServices;
using System.Transactions;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

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
            var users = await GetUsersFromTableAsync();
            using (var transaction = StateManager.CreateTransaction())
            {
               
                foreach (var user in users)
                {
                    await usersDictionary.TryAddAsync(transaction, user.Email, user);
                }
                await transaction.CommitAsync();
            }


            //await FillUserDictionary();
            usersTableThread = new Thread(new ThreadStart(UsersTableThread));
            usersTableThread.Start();
        }

        private async Task SetTableAsync()
        {
            var tsClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tsClient.CreateTableIfNotExistsAsync("User");
            usersTable = tsClient.GetTableClient("User");
        }

        private async Task<IEnumerable<User>> GetUsersFromTableAsync()
        {
            var users = new List<User>();

            await foreach (var entity in usersTable.QueryAsync<UserEntity>(filter: x => true))
            {
                // Convert UserEntity to User
                var user = new User(entity);
                users.Add(user);
            }

            return users;
        }

        private async Task FillUserDictionary()
        {
            var items = usersTable.QueryAsync<UserEntity>(filter: x => true).GetAsyncEnumerator();

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

        #region USER METHODS
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

        public async Task<string> GetUserType(string email)
        {
            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, email);
                if (currUser.HasValue)
                {
                    
                    return currUser.Value.UserType.ToString();
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

        public async Task<bool> IsVerifiedAsync(string email)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, email);
                if (currUser.HasValue)
                {
                    if(currUser.Value.Verification == Verification.Approved)
                    {
                        temp = true;
                    }
                    else
                    {
                        temp = false;
                    }
                }
            }
            return temp;
        }

        public async Task<bool> IsBlockedAsync(string email)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, email);
                if (currUser.HasValue)
                {
                    temp = currUser.Value.IsBlocked;
                }
            }
            return temp;
        }


        #endregion

        #region ADMINISTRATOR METHODS
        //***ADMINISTRATOR METHODS***
        public async Task<bool> ApproveVerificationAsync(string email)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, email);

                if (!currUser.HasValue)
                {
                    temp = false;
                }else if(currUser.Value.Verification != Verification.OnHold){
                    temp = false;
                }
                else
                {
                    var user = currUser.Value;
                    user.Verification = Verification.Approved;

                    try
                    {
                        await usersDictionary.TryUpdateAsync(transaction, email, user, user);
                        await transaction.CommitAsync();
                        temp = true;
                    }
                    catch (Exception)
                    {
                        temp = false;
                        transaction.Abort();
                    }
                }

                if (temp)
                {
                    try
                    {
                        var serviceProxy = ServiceProxy.Create<IEmail>(new Uri("fabric:/TaxiApp/EmailService"));
                        await serviceProxy.SendMailAsync(email, true);
                    }
                    catch (Exception ex)
                    {
                        // Handle or log other exceptions
                        Console.WriteLine($"Exception: {ex.Message}");
                    }
                }
            }

            return temp;
        }

        public async Task<bool> DenyVerificationAsync(string email)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, email);

                if (!currUser.HasValue)
                {
                    temp = false;
                }
                else if (currUser.Value.Verification != Verification.OnHold)
                {
                    temp = false;
                }
                else
                {
                    var user = currUser.Value;
                    user.Verification = Verification.Denied;

                    try
                    {
                        await usersDictionary.TryUpdateAsync(transaction, email, user, user);
                        await transaction.CommitAsync();
                        temp = true;
                    }
                    catch (Exception)
                    {
                        temp = false;
                        transaction.Abort();
                    }
                }
                if (temp)
                {
                    IEmail proxy = ServiceProxy.Create<IEmail>(new Uri("fabric:/TaxiApp/EmailService"));
                    await proxy.SendMailAsync(email, true);
                }
            }

            return temp;
        }

        public async Task<IEnumerable<DriverDto>> GetDriversAsync()
        {
            var retDrivers = new List<DriverDto>();
            using (var transaction = StateManager.CreateTransaction())
            {
                var enumm = (await usersDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();


                while(await enumm.MoveNextAsync(CancellationToken.None))
                {
                    var user = enumm.Current.Value;
                    if(user.UserType == TypeOfUser.Driver)
                    {
                        retDrivers.Add(new DriverDto(user));
                    }
                }
                await transaction.CommitAsync();
            }
            return retDrivers;
        }

        public async Task<bool> DriverBlockAsync(string id)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var currUser = await usersDictionary.TryGetValueAsync(transaction, id);

                if (!currUser.HasValue)
                {
                    temp = false;
                }
                else
                {
                    if(currUser.Value.UserType != TypeOfUser.Driver || currUser.Value.IsBlocked == true)
                    {
                        temp = false;
                    }
                    else
                    {
                        var user = currUser.Value;
                        user.IsBlocked = true;

                        try
                        {
                            await usersDictionary.TryUpdateAsync(transaction, id, user, user);
                            await transaction.CommitAsync();
                            temp = true;
                        }
                        catch (Exception)
                        {
                            temp = false;
                            transaction.Abort();
                        }
                    }
                }
                //TODO:send email
            }

            return temp;
        }


        #endregion

    }
}
