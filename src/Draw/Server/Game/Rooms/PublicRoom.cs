using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;

namespace Draw.Server.Game.Rooms
{
    public class PublicRoom : Room
    {
        public PublicRoom(IHubContext<Hubs.GameHub> context, string roomName, RoomSettings settings) 
            : base(context, roomName, settings)
        {
        }
    }
}
