using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.RepositoryInterface
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<User> FindByEmailAsync(string email);
        Task<User> FindByPersonalIdentificationNumberAsync(string personalIdentificationNumber);
        Task UpdateUserAsync(User user);
        Task<User> FindByFieldAsync(string value);

    }
}
