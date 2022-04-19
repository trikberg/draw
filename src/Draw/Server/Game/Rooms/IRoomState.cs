using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Server.Game.Rooms
{
    internal interface IRoomState
    {
        Task Enter();

        Task AddPlayer(Player player, bool isReconnect);
        Task RemovePlayer(Player player);
    }
}
