using Microsoft.AspNetCore.Components.Web;
using Draw.Shared.Draw;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    internal interface IGameService : IDisposable
    {
        public event EventHandler<string> BackgroundColorChanged;
        public event EventHandler RoomListChanged;
        public event EventHandler PlayerListChanged;
        public event EventHandler RoomSettingsChanged;
        public event EventHandler GameStarted;
        public event EventHandler<WordChoiceEventArgs> ActivePlayerWordChoiceStarted;
        public event EventHandler<(PlayerDTO player, int timeout)> PlayerWordChoiceStarted;
        public event EventHandler<PlayerDTO> CorrectGuessMade;
        public event EventHandler<(List<PlayerScore> scores, int timeout)> TurnScores;
        public event EventHandler<(List<PlayerScore> scores, int timeout)> GameScores;

        public IEnumerable<RoomStateDTO> Rooms { get; }
        public IEnumerable<Player> Players { get; }
        public RoomStateDTO RoomState { get; }
        public GameState GameState { get; }
        public Guid? PlayerGuid { get; }

        public Task SetPlayerName(string userName);
        public Task<bool> CreateRoom(string roomName, RoomSettings roomSettings);
        public Task<bool> JoinRoom(string roomName, string password);
        public Task SetRoomSettings(RoomSettings roomSettings);
        public Task<bool> StartGame();
        public Task<bool> LeaveRoom();
        public Task WordChosen(int wordIndex);
        public Task MakeGuess(string guess);
        public Task DrawLine(DrawLineEventArgs e);
        public Task Fill(FillEventArgs args);
        public Task ChangeBackgroundColor(string color);
        public Task ClearCanvas();
        public Task Undo();
    }
}
