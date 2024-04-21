using Application.Dtos;
using Application.RepositoryInterface;
using Application.Security;
using Application.ServiceInterface;
using Domain.Exceptions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Application.ServiceImplementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _encryptionKey;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _encryptionKey = GenerateAesKey();
        }

        public async Task<User> Register(UserDto userRegisterDto)
        {
            ValidateUser(userRegisterDto);

            await EnsureUniqueField(userRegisterDto.Email, "Email");
            await EnsureUniqueField(userRegisterDto.PersonalIdentificationNumber, "Personal Identification Number");

            var temporaryPassword = GenerateRandomPassword();
                        
            var encryptedTemporaryPassword = Encryption.Encrypt(temporaryPassword, _encryptionKey);

            var newUser = new User
            {
                Email = userRegisterDto.Email,
                PersonalIdentificationNumber = userRegisterDto.PersonalIdentificationNumber,
                Pin = userRegisterDto.Pin, 
                TemporaryPassword = encryptedTemporaryPassword 
            };

            await _userRepository.AddUserAsync(newUser);

            return newUser;
        }

        public async Task<User> Login(UserDto userLoginDto)
        {
            ValidateUser(userLoginDto);

            var user = await _userRepository.FindByEmailAsync(userLoginDto.Email);

            if (user == null || user.Pin != userLoginDto.Pin)
            {
                throw new UnauthorizedAccessException("Invalid credentials. No user found with the provided email or PIN doesn't match.");
            }
                        
            var newTemporaryPassword = GenerateRandomPassword();
            var newEncryptedTemporaryPassword = Encryption.Encrypt(newTemporaryPassword, _encryptionKey);

            user.TemporaryPassword = newEncryptedTemporaryPassword;
            user.TemporaryPasswordGeneratedTime = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            return user;
        }

        public async Task<string> GenerateTemporaryPassword(UserDto userLoginDto)
        {
            var user = await _userRepository.FindByEmailAsync(userLoginDto.Email);
                        
            if (user == null || user.Pin != userLoginDto.Pin)
            {
                throw new MultivalidationException("Invalid credentials.");
            }
                        
            if (!IsTemporaryPasswordExpired(user))
            {                
                var temporaryPassword = GenerateRandomPassword();
                                
                var encryptedTemporaryPassword = Encryption.Encrypt(temporaryPassword, _encryptionKey);
                                
                user.TemporaryPassword = encryptedTemporaryPassword;
                user.TemporaryPasswordGeneratedTime = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                                
                return temporaryPassword;
            }
            else
            {                
                return "Your password validity has expired.";
            }
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

        private static string GenerateRandomPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()+=";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static bool IsTemporaryPasswordExpired(User user)
        {
            var temporaryPasswordExpirationTimeSpan = TimeSpan.FromMinutes(5);
            return DateTime.UtcNow > user.TemporaryPasswordGeneratedTime.Add(temporaryPasswordExpirationTimeSpan);
        }

        private static string GenerateAesKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }
    }
}