using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Draw.Server.Game.Rooms
{
    public class RoomStateLobby : IRoomState
    {
        private readonly Room room;
        private readonly Func<Room, Task> gameEndedCallback;
        private int entryCount = 0;

        internal RoomStateLobby(Room room, Func<Room, Task> gameEndedCallback)
        {
            this.room = room;
            this.gameEndedCallback = gameEndedCallback;
        }

        public async Task Enter()
        {
            if (entryCount > 0)
            {
                await gameEndedCallback(room);
            }
            entryCount++;
            return;
        }

        internal async Task<bool> SetRoomSettings(RoomSettings settings, Player player)
        {
            Player? firstPlayer = room.Players.FirstOrDefault();
            if (firstPlayer != null && firstPlayer.Equals(player))
            {
                room.RoomSettings = settings;
                await room.SendAllExcept(player, "RoomStateChanged", room.ToRoomStateDTO());
                return true;
            }
            return false;
        }

        public Task AddPlayer(Player player, bool isReconnect)
        {
            return room.SendPlayer(player, "RoomStateChanged", room.ToRoomStateDTO());
        }

        public Task RemovePlayer(Player player)
        {
            return Task.CompletedTask;
        }
    }
}
