using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.ServiceInterface
{
    public interface IUserService
    {
        Task<User> Register(UserDto userRegisterDto);

        Task<User> Login (UserDto userLoginDto);

        Task<string> GenerateTemporaryPassword(UserDto userLoginDto);

    }
}
