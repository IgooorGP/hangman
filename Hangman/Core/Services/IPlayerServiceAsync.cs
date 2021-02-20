using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Services
{
    public interface IPlayerServiceAsync
    {
        public Task<Player?> GetById(Guid id);
        public Task<IEnumerable<Player>> GetAll();
        public Task<Player> Create(NewPlayerData newPlayerData);
        public Task<Player?> GetByPlayerName(string playerName);
    }
}