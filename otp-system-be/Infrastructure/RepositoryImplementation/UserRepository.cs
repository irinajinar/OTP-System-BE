using Application.RepositoryInterface;
using System;
using Infrastructure.DataContext;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RepositoryImplementation
{
    public class UserRepository:IUserRepository
    {
        private readonly DataAppContext _dataAppContext;

        public UserRepository(DataAppContext dataAppContext)
        {
            _dataAppContext= dataAppContext;
        }

        public async Task AddUserAsync(User user)
        {
            await _dataAppContext.Users.AddAsync(user);
            await _dataAppContext.SaveChangesAsync();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            var user = await _dataAppContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }

        public async Task<User> FindByPersonalIdentificationNumberAsync(string personalIdentificationNumber)
        {
            var user = await _dataAppContext.Users.FirstOrDefaultAsync(u => u.PersonalIdentificationNumber == personalIdentificationNumber);
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _dataAppContext.Users.Update(user);
            await _dataAppContext.SaveChangesAsync();
        }

        public async Task<User> FindByFieldAsync(string value)
        {
            var user = await _dataAppContext.Users.FirstOrDefaultAsync(u => u.Email == value || u.PersonalIdentificationNumber == value);
            return user;
        }
    }
}
