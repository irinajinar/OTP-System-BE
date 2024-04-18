using Application.Dtos;
using Application.RepositoryInterface;
using Application.ServiceInterface;
using Domain.Exceptions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServiceImplementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<User> Register(UserDto userDto)
        {
            if (userDto == null)
            {
                throw new MultivalidationException("Body is null");
            }

            var validationErrors = ValidateUserRegistration(userDto);
            if (validationErrors.Any())
            {
                throw new MultivalidationException(validationErrors);
            }

            var existingUserByEmail = await _userRepository.FindByEmailAsync(userDto.Email);
            if (existingUserByEmail != null)
            {
                throw new MultivalidationException(400, "Email already exists");
            }

            var existingUserByPersonalIdentificationNumber = await _userRepository.FindByPersonalIdentificationNumberAsync(userDto.PersonalIdentificationNumber);
            if (existingUserByPersonalIdentificationNumber != null)
            {
                throw new MultivalidationException(400, "Personal identification number already exists");
            }

            var newUser = new User
            {
                Email = userDto.Email,
                PersonalIdentificationNumber = userDto.PersonalIdentificationNumber,
                Pin = userDto.Pin
            };

            await _userRepository.AddUserAsync(newUser);

            return newUser;
        }

        private List<string> ValidateUserRegistration(UserDto userDto)
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrEmpty(userDto.Email))
            {
                validationErrors.Add("Email is required.");
            }
            else if (!IsValidEmail(userDto.Email))
            {
                validationErrors.Add("Invalid email address.");
            }

            if (string.IsNullOrEmpty(userDto.PersonalIdentificationNumber))
            {
                validationErrors.Add("Personal identification number is required.");
            }

            if (string.IsNullOrEmpty(userDto.Pin))
            {
                validationErrors.Add("PIN is required.");
            }

            return validationErrors;
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
