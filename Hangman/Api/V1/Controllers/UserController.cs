using System.Threading.Tasks;
using AutoMapper;
using Hangman.Core.DTOs;
using Hangman.Core.Models;
using Hangman.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hangman.Api.V1.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserSvc _userSvc;
        private readonly IMapper _mapper;

        public UserController(ILogger<UserController> logger, IUserSvc userSvc, IMapper mapper)
        {
            _logger = logger;
            _userSvc = userSvc;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDTO createUserRequestDTO)
        {
            _logger.LogInformation("Received new create user request {createUserRequestDTO}", createUserRequestDTO);

            var newUser = await _userSvc.Create(createUserRequestDTO);
            var newUserResponse = _mapper.Map<User, UserResponseDTO>(newUser);

            return StatusCode(201, newUserResponse);
        }
    }
}