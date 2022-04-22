using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;
using System;
using System.Threading.Tasks;

namespace Draw.Server.Game.Rooms
{
    public class PublicRoom : Room
    {
        public PublicRoom(IHubContext<Hubs.GameHub> context, string roomName, RoomSettings settings, Func<Room, Task> gameEndedCallback) 
            : base(context, roomName, settings, gameEndedCallback)
        {
        }
    }
}
