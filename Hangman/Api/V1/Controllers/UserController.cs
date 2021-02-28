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
        private readonly IUserSvc _userSvc;
        private readonly IJwtSvc _jwtSvc;
        private readonly IMapper _mapper;

        public UserController(IUserSvc userSvc, IJwtSvc jwtSvc, IMapper mapper)
        {
            _mapper = mapper;
            _userSvc = userSvc;
            _jwtSvc = jwtSvc;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDTO createUserRequestDTO)
        {
            var newUser = await _userSvc.Create(createUserRequestDTO);
            var newUserResponse = _mapper.Map<User, UserResponseDTO>(newUser);

            return StatusCode(201, newUserResponse);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequestDTO authenticationRequestDTO)
        {
            var loggedUser = await _userSvc.Authenticate(authenticationRequestDTO);  // throws if authentication fails
            var signedUserJwt = _jwtSvc.GenerateToken(loggedUser);

            return Ok(new LoggedUserJwtResponseDTO { Username = loggedUser.Username, Token = signedUserJwt });
        }
    }
}