using Application.Dtos;
using Application.ServiceInterface;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            try
            {
                var newUser = await _userService.Register(userDto);
                return Ok(newUser);
            }
            catch (MultivalidationException ex)
            {
                return BadRequest(ex.ValidationErrors);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userLoginDto)
        {
            try
            {
                var user = await _userService.Login(userLoginDto);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (MultivalidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("generate-temporary-password")]
        public async Task<IActionResult> GenerateTemporaryPassword(UserDto userLoginDto)
        {
            try
            {
                var temporaryPassword = await _userService.GenerateTemporaryPassword(userLoginDto);
                return Ok(temporaryPassword);
            }
            catch (MultivalidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
