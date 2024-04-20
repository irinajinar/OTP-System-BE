using Application.Dtos;
using Application.RepositoryInterface;
using Application.ServiceInterface;
using Domain.Exceptions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<User> Register(UserDto userRegisterDto)
        {
            ValidateUser(userRegisterDto);

            await EnsureUniqueField(userRegisterDto.Email, "Email");
            await EnsureUniqueField(userRegisterDto.PersonalIdentificationNumber, "Personal Identification Number");

            var temporaryPassword = GenerateTemporaryPassword();
            var newUser = new User
            {
                Email = userRegisterDto.Email,
                PersonalIdentificationNumber = userRegisterDto.PersonalIdentificationNumber,
                Pin = userRegisterDto.Pin,
                TemporaryPassword = temporaryPassword
            };

            await _userRepository.AddUserAsync(newUser);

            return newUser;
        }

        public async Task<User> Login(UserDto userLoginDto)
        {
            ValidateUser(userLoginDto);

            var user = await _userRepository.FindByEmailAsync(userLoginDto.Email);

            if (user == null || !IsValidUser(user, userLoginDto))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            user.TemporaryPassword = GenerateNewTemporaryPassword(user.TemporaryPassword);
            user.TemporaryPasswordGeneratedTime = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            return user;
        }

        public async Task<string> GenerateTemporaryPassword(UserDto userLoginDto)
        {
            var user = await _userRepository.FindByEmailAsync(userLoginDto.Email);
            if (user == null || !IsValidUser(user, userLoginDto))
            {
                throw new MultivalidationException("Invalid credentials.");
            }

            if (!IsTemporaryPasswordExpired(user))
            {
                return user.TemporaryPassword;
            }

            var temporaryPassword = GenerateRandomPassword();
            user.TemporaryPassword = temporaryPassword;
            user.TemporaryPasswordGeneratedTime = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            return temporaryPassword;
        }

        private static void ValidateUser(UserDto userDto)
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrEmpty(userDto.Email) || !IsValidEmail(userDto.Email))
            {
                validationErrors.Add("Invalid or missing email address.");
            }

            if (string.IsNullOrEmpty(userDto.PersonalIdentificationNumber))
            {
                validationErrors.Add("Personal identification number is required.");
            }

            if (string.IsNullOrEmpty(userDto.Pin))
            {
                validationErrors.Add("PIN is required.");
            }

            if (validationErrors.Any())
            {
                throw new MultivalidationException(validationErrors);
            }
        }

        private async Task EnsureUniqueField(string value, string fieldName)
        {
            var existingUser = await _userRepository.FindByFieldAsync(value);
            if (existingUser != null)
            {
                throw new MultivalidationException($"{fieldName} already exists.");
            }
        }

        private static bool IsValidUser(User user, UserDto userDto)
        {
            return user.Pin == userDto.Pin && user.PersonalIdentificationNumber == userDto.PersonalIdentificationNumber;
        }

        private static bool IsValidEmail(string email)
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

        private static string GenerateTemporaryPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GenerateRandomPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GenerateNewTemporaryPassword(string existingTemporaryPassword)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
                        
            var newPassword = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                        
            return existingTemporaryPassword + newPassword;
        }

        private static bool IsTemporaryPasswordExpired(User user)
        {
            var temporaryPasswordExpirationTimeSpan = TimeSpan.FromMinutes(5);
            return DateTime.UtcNow > user.TemporaryPasswordGeneratedTime.Add(temporaryPasswordExpirationTimeSpan);
        }
    }
}
