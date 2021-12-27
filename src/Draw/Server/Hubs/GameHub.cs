using Draw.Server.Game;
using Draw.Server.Game.Rooms;
using Draw.Shared.Draw;
using Draw.Shared.Game;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Server.Hubs
{
    public class GameHub : Hub
    {
        public const string HubUrl = "/game";

        private static Dictionary<string, Player> playerDictionary = new Dictionary<string, Player>();

        private Lobby lobby;
        private ILogger<GameHub> logger;

        public GameHub(Lobby lobby, ILogger<GameHub> logger)
        {
            this.lobby = lobby;
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            HttpTransportType? tranportType = Context.Features.Get<IHttpTransportFeature>()?.TransportType;
            logger.LogDebug("OnConnected SignalR TransportType: " + tranportType);
            if (!playerDictionary.ContainsKey(Context.ConnectionId))
            {
                Player player = new Player(Context.ConnectionId);
                playerDictionary.Add(Context.ConnectionId, player);
                lobby.AddPlayer(player);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                logger.LogWarning(exception, "Player disconnect with exception " + exception.GetType().Name);
            }
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await lobby.LeaveRoom(player, room);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public List<RoomStateDTO> GetRooms()
        {
            return lobby.GetRooms().ToList();
        }

        public Guid SetPlayerName(string userName)
        {
            if (playerDictionary.TryGetValue(Context.ConnectionId, out Player player))
            {
                player.Name = userName;
            }
            else
            {
                player = new Player(Context.ConnectionId, userName);
                playerDictionary.Add(Context.ConnectionId, player);
            }
            return player.Id;
        }

        public Task<bool> CreateRoom(string roomName, RoomSettings roomSettings)
        {
            return lobby.CreateRoom(roomName, roomSettings);
        }

        public async Task<RoomStateDTO> JoinRoom(string roomName, string password)
        {
            Player player = GetPlayer(Context.ConnectionId);
            if (player == null)
            {
                return null;
            }

            Room newRoom = lobby.GetRoom(roomName);
            if (newRoom != null)
            {
                return await lobby.JoinRoom(player, newRoom, password);
            }
            return null;
        }

        public async Task<bool> LeaveRoom()
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (player == null || room == null)
            {
                return false;
            }
            await lobby.LeaveRoom(player, room);
            return true;
        }

        public async Task SetRoomSettings(RoomSettings settings)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await lobby.SetRoomSettings(settings, room, player);
            }
        }

        public async Task<bool> StartGame()
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                return await lobby.StartGame(room, player);
            }
            return false;
        }

        public void WordChosen(int wordIndex)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                room.WordChosen(wordIndex, player);
            }
        }

        public async Task MakeGuess(string guess)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await room.MakeGuess(player, guess);
            }    
        }

        private Player GetPlayer(string connectionId)
        {
            if (playerDictionary.TryGetValue(connectionId, out Player player))
            {
                return player;
            }
            return null;
        }

        public async Task DrawLine(DrawLineEventArgs e)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await room.DrawLine(player, e);
                //await Clients.OthersInGroup(room.RoomName).SendAsync("DrawLine", e);
            }
        }

        public async Task Fill(FillEventArgs e)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await room.Fill(player, e);
                //await Clients.OthersInGroup(room.RoomName).SendAsync("DrawLine", e);
            }
        }

        public async Task ClearCanvas(string backgroundColor)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await room.ClearCanvas(player, backgroundColor);
            }
        }

        public async Task Undo()
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await room.Undo(player);
            }
        }

        public async Task ChangeBackgroundColor(string color)
        {
            Player player = GetPlayer(Context.ConnectionId);
            Room room = lobby.GetRoom(player);
            if (room != null && player != null)
            {
                await room.ChangeBackgroundColor(player, color);
            }
        }
    }
}
