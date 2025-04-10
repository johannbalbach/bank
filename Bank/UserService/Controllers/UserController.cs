using Bank.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using UserService.Dtos;
using UserService.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("user/[controller]")]
    public class UserController: ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }
        //[HttpPost("register")]
        //[AllowAnonymous]
        //public async Task<UserDto> Register(UserRegisterDto userDto)
        //{
        //    var result = await _service.RegisterUser(userDto);
        //    return result;
        //}

        //[HttpPost("login")]
        //[AllowAnonymous]
        //public async Task<TokenResponseDto> Login([FromBody] LoginRequestDto loginRequest)
        //{
        //    var token = await _service.Login(loginRequest);
        //    return token;
        //}
        [HttpGet("GetMyProfile")]
        [Authorize]
        public async Task<UserDto> GetMyProfile()
        {
            var user = await _service.GetUserById(GetUserId());
            return user;
        }

        [HttpGet("GetAllUsers")]
        [Authorize]
        public async Task<UserBrieflyPaginationListDto> GetAllUsers([FromQuery] UserBrieflyPaginationListQueryDto query)
        {
            return await _service.GetAllUsers(query);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<UserDto> GetUser(Guid id)
        {
            var user = await _service.GetUserById(id);
            return user;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult> UpdateUser(Guid id, [FromBody] UserEditDto userDto)
        {
            var success = await _service.UpdateUser(id, userDto);
            return Ok(success);
        }

        [HttpPost("{id}/block")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult> BlockUser(Guid id, bool isBlocked)
        {
            var success = await _service.BlockUser(id, isBlocked);
            return Ok(success);
        }

        [HttpGet("{id}/role")]
        [Authorize]
        public async Task<UserRole> CheckRole(Guid id)
        {
            return await _service.CheckRole(id);
        }

        private Guid GetUserId()
        {
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (userEmailClaim == null)
                throw new Exception("Token not found");

            return new Guid(userEmailClaim);
        }
    }
}
