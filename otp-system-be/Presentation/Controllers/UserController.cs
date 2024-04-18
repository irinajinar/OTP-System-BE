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

        [HttpPost]
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
     

    }
}
