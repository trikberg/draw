using Microsoft.AspNetCore.SignalR;
using Draw.Server.Game.Rooms;
using Draw.Server.Hubs;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Draw.Server.Game
{
    public class Lobby
    {
        private readonly ILogger<Lobby> logger;
        private IHubContext<GameHub> hubContext;
        private List<Room> rooms = new List<Room>();
        private Dictionary<Player, Room> playerToRoomDictionary = new Dictionary<Player, Room>();
        private string lobbyGroupName = Guid.NewGuid().ToString();

        public Lobby(IHubContext<GameHub> context, ILogger<Lobby> logger)
        {
            this.logger = logger;
            hubContext = context;
            for (int i = 0; i < 5; i++)
            {
                AddRoom(new PublicRoom(context, "Room " + i, new RoomSettings()));
            }
        }

        internal void AddRoom(Room room)
        {
            int count;
            lock (rooms)
            {
                rooms.Add(room);
                count = rooms.Count;
            }
            logger.LogInformation("Room added. Index: " + room.RoomIndex + ". Name: " + room.RoomName + ". Active rooms: " + count);
        }

        internal IEnumerable<Player> GetPlayersInRoom(string roomName)
        {
            Room room;
            lock (rooms)
            {
                room = rooms.Where(r => r.RoomName.Equals(roomName)).FirstOrDefault();
            }

            if (room == null)
            {
                return null;
            }
            return room.Players;
        }

        internal void AddPlayer(Player player)
        {
            hubContext.Groups.AddToGroupAsync(player.ConnectionId, lobbyGroupName);
        }

        internal IEnumerable<RoomStateDTO> GetRooms()
        {
            lock (rooms)
            {
                return rooms.Select(r => r.ToRoomStateDTO());
            }
        }

        internal async Task<bool> CreateRoom(string roomName, RoomSettings roomSettings)
        {
            lock (rooms)
            {
                if (string.IsNullOrWhiteSpace(roomName) ||
                    rooms.Where(r => r.RoomName.Equals(roomName, StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
                {
                    return false;
                }
            }
            Room newRoom = new PublicRoom(hubContext, roomName, roomSettings);
            AddRoom(newRoom);
            await hubContext.Clients.Group(lobbyGroupName).SendAsync("RoomCreated", newRoom.ToRoomStateDTO());
            return true;
        }

        internal async Task<RoomStateDTO> JoinRoom(Player player, Room newRoom, string password)
        {
            if (newRoom == null || player == null)
            {
                return null;
            }

            if (newRoom.RoomSettings.IsPrivateRoom &&
                !newRoom.RoomSettings.Password.Equals(password))
            {
                return null;
            }

            if (playerToRoomDictionary.TryGetValue(player, out Room oldRoom))
            {
                await LeaveRoom(player, oldRoom);
            }
            else
            {
                await hubContext.Groups.RemoveFromGroupAsync(player.ConnectionId, lobbyGroupName);
            }

            playerToRoomDictionary.Add(player, newRoom);
            await newRoom.AddPlayer(player);
            await hubContext.Groups.AddToGroupAsync(player.ConnectionId, newRoom.RoomName);
            await hubContext.Clients.GroupExcept(newRoom.RoomName, player.ConnectionId).SendAsync("PlayerJoined", player.ToPlayerDTO());
            await hubContext.Clients.Group(lobbyGroupName).SendAsync("RoomStateChanged", newRoom.ToRoomStateDTO());
            return newRoom.ToRoomStateDTO();
        }

        internal async Task SetRoomSettings(RoomSettings settings, Room room, Player player)
        {
            if (await room.SetRoomSettings(settings, player))
            {
                await hubContext.Clients.Group(lobbyGroupName).SendAsync("RoomStateChanged", room.ToRoomStateDTO());
            }
        }

        internal async Task<bool> StartGame(Room room, Player player)
        {
            if (room.StartGame(player))
            {
                await hubContext.Clients.Group(lobbyGroupName).SendAsync("RoomStateChanged", room.ToRoomStateDTO());
                logger.LogInformation("New game started in room " + room.RoomName + ". Language: " + room.RoomSettings.Language +  ". Player count: " + room.Players.Count + ".");
                return true;
            }
            return false;
        }

        internal Room GetRoom(Player player)
        {
            if (player == null)
            {
                return null;
            }

            if (playerToRoomDictionary.TryGetValue(player, out Room room))
            {
                return room;
            }

            return null;
        }

        internal Room GetRoom(string roomName)
        {
            lock (rooms)
            {
                return rooms.Where(r => (roomName.Equals(r.RoomName))).FirstOrDefault();
            }
        }

        internal async Task LeaveRoom(Player player, Room room)
        {
            if (playerToRoomDictionary.Remove(player))
            {
                await room.RemovePlayer(player);
                if (room.Players.Count == 0)
                {
                    lock (rooms)
                    {
                        rooms.Remove(room);
                    }
                    await hubContext.Clients.Group(lobbyGroupName).SendAsync("RoomDeleted", room.ToRoomStateDTO());
                }
                await hubContext.Clients.GroupExcept(room.RoomName, player.ConnectionId).SendAsync("PlayerLeft", player.ToPlayerDTO());
                await hubContext.Groups.RemoveFromGroupAsync(player.ConnectionId, room.RoomName);
                await hubContext.Clients.Group(lobbyGroupName).SendAsync("RoomStateChanged", room.ToRoomStateDTO());
            }
        }
    }
}
