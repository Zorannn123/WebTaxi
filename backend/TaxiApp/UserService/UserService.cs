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
                        Image = string.IsNullOrEmpty(regDto.Image) ? "" : UploadUserImage(regDto.Image)
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
            using (var ms = new MemoryStream(Convert.FromBase64String(base64Image.Split(',')[1])))
            {
                using (var image = Image.FromStream(ms))
                {
                    return new Blob().UploadImage(new Bitmap(image), Guid.NewGuid().ToString() + ".jpg");
                }
            }
        }


    }
}
