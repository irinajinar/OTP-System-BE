using Moq;
using Application.Dtos;
using Application.RepositoryInterface;
using Application.ServiceImplementation;
using Domain.Models;
using FluentAssertions;
using Domain.Exceptions;

namespace ServiceTest
{
    public class UserServiceTests
    {
        private UserService _userService;
        private Mock<IUserRepository> _mockUserRepository;

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Test]
        public async Task Register_ValidUser_ReturnsUser()
        {
            // Arrange
            var userDto = new UserDto
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "1234567890",
                Pin = "1234" 
            };

            var newUser = new User
            {
                Email = userDto.Email,
                PersonalIdentificationNumber = userDto.PersonalIdentificationNumber,
                Pin = "1234", 
                TemporaryPassword = "temporaryPassword"
            };

            _mockUserRepository.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(newUser));

            // Act
            var result = await _userService.Register(userDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(userDto.Email);
            result.PersonalIdentificationNumber.Should().Be(userDto.PersonalIdentificationNumber);
        }

        [Test]
        public async Task RegisterInvalidUserThrowsException()
        {
            // Arrange
            var invalidUserDto = new UserDto
            {                
                Email = "test@example.com",
                PersonalIdentificationNumber = "test",
                Pin = null
            };

            await Task.Run(() =>
            {
                Assert.ThrowsAsync<MultivalidationException>(() => _userService.Register(invalidUserDto));
            });
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsUserWithNewTemporaryPassword()
        {
            // Arrange
            var userDto = new UserDto
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "1123"
            };
                       
            var existingUser = new User
            {
                Email = userDto.Email,
                PersonalIdentificationNumber = userDto.PersonalIdentificationNumber,
                Pin = userDto.Pin
            };
            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(userDto.Email))
                              .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.Login(userDto);

            // Assert
            Assert.NotNull(result); 
            Assert.NotNull(result.TemporaryPassword);
            Assert.That(result.Email, Is.EqualTo(existingUser.Email)); 
            Assert.That(result.PersonalIdentificationNumber, Is.EqualTo(existingUser.PersonalIdentificationNumber));
            Assert.That(result.Pin, Is.EqualTo(existingUser.Pin));

            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u => u == result)), Times.Once);
        }

        [Test]
        public async Task Login_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userDto = new UserDto
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "invalid" 
            };

            var existingUser = new User
            {
                Email = "test@example.com", 
                PersonalIdentificationNumber = "12345",
                Pin = "0000" 
            };
            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(userDto.Email))
                              .ReturnsAsync(existingUser);

            // Act and Assert
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.Login(userDto));

            // Assert
            Assert.NotNull(exception);

            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        
        [Test]
        public async Task GenerateTemporaryPassword_ExpiredTemporaryPassword_ReturnsExpirationMessage()
        {
            // Arrange
            var userLoginDto = new UserDto
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "password123"
            };

            var existingUser = new User
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "password123",
                TemporaryPasswordGeneratedTime = DateTime.UtcNow.AddHours(-25) 
            };

            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(userLoginDto.Email))
                              .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.GenerateTemporaryPassword(userLoginDto);

            // Assert
            Assert.That(result, Is.EqualTo("Your password validity has expired."));

            // Verify that the user repository method was not called
            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }


        [Test]        
        public async Task GenerateTemporaryPassword_InvalidCredentials_ThrowsMultivalidationException()
        {
            // Arrange
            var userLoginDto = new UserDto
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "invalidPin"
            };

            var existingUser = new User
            {
                Email = "test@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "password123"
            };

            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(userLoginDto.Email))
                      .ReturnsAsync(existingUser);

            // Act and Assert
            Assert.ThrowsAsync<MultivalidationException>(() => _userService.GenerateTemporaryPassword(userLoginDto));

            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task GenerateTemporaryPassword_EmailDoesNotCorrespond_ThrowsMultivalidationException()
        {
            // Arrange
            var userLoginDto = new UserDto
            {
                Email = "nonexistent@example.com",
                PersonalIdentificationNumber = "12345",
                Pin = "password123"
            };

            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(userLoginDto.Email))
                              .ReturnsAsync((User)null);

            // Act and Assert
            MultivalidationException exception = null;
            try
            {
                await _userService.GenerateTemporaryPassword(userLoginDto);
            }
            catch (MultivalidationException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            _mockUserRepository.Verify(repo => repo.FindByEmailAsync(userLoginDto.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }


    }
}