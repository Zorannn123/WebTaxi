﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    public interface IUser : IService
    {
        Task<bool> LoginAsync(LoginDto logDto);
    }
}
