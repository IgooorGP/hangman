using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangman.Services;
using Hangman.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hangman.Api.V1.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerServiceAsync _playerServiceAsync;
        private readonly ILogger<PlayerController> _logger;

        public PlayerController(ILogger<PlayerController> logger, IPlayerServiceAsync playerServiceAsync)
        {
            _playerServiceAsync = playerServiceAsync;
            _logger = logger;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Player>> GetById(Guid id)
        {
            _logger.LogInformation("Calling playerService to get player with id: {id:l}", id);
            var player = await _playerServiceAsync.GetById(id);

            if (player != null) return Ok(player);

            return NotFound();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> All()
        {
            _logger.LogInformation("Calling playerService to get all players...");
            var players = await _playerServiceAsync.GetAll();

            _logger.LogInformation($"Returning all players: {@players}", players);
            return Ok(players);
        }

        [HttpPost]
        public async Task<ActionResult<Player>> Create(NewPlayerData newPlayerData)
        {
            var player = await _playerServiceAsync.Create(newPlayerData);
            _logger.LogInformation("New player has been created, name: {name}", player.Name);

            return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);
        }
    }
}