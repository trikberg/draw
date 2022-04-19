using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Server.Game.Rooms
{
    public class RoomStateLobby : IRoomState
    {
        private Room room;

        internal RoomStateLobby(Room room)
        {
            this.room = room;
        }

        public Task Enter()
        {
            return Task.CompletedTask;
        }

        internal async Task<bool> SetRoomSettings(RoomSettings settings, Player player)
        {
            Player firstPlayer = room.Players.FirstOrDefault();
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
