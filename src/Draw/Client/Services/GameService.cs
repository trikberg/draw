using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Draw.Shared.Draw;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    public class GameService : IGameService, IDisposable
    {
        private HubConnection hubConnection;
        private NavigationManager navigationManager;

        private GameState gameState = new GameState();
        private List<RoomStateDTO> rooms = new List<RoomStateDTO>();
        private List<Player> players = new List<Player>();
        private RoomStateDTO currentRoomState = null;
        private Guid? playerGuid = null;

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

        public GameService(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
            Init();
        }

        public void Dispose()
        {
            gameState?.TurnTimer?.Dispose();
        }

        private async void Init()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/game"))
                .Build();

            InitServerCallbacks();

            await hubConnection.StartAsync();

            List<RoomStateDTO> newRooms = await hubConnection.InvokeAsync<List<RoomStateDTO>>("GetRooms");
            AddRooms(newRooms);
        }

        private void InitServerCallbacks()
        {
            hubConnection.On<DrawLineEventArgs>("DrawLine", (e) => gameState.DrawLine(e));
            hubConnection.On<FillEventArgs>("Fill", (e) => gameState.Fill(e));
            hubConnection.On<string>("ChangeBackgroundColor", (color) => BackgroundColorChanged?.Invoke(this, color));
            hubConnection.On<string>("ClearCanvas", (backgroundColor) => gameState.ClearCanvas(backgroundColor));
            hubConnection.On("Undo", () => gameState.Undo());

            hubConnection.On<PlayerDTO>("PlayerJoined", (p) =>
            {
                Player player = new Player(p);
                if (!players.Contains(player))
                {
                    players.Add(player);
                    PlayerListChanged?.Invoke(this, null);
                }
            });

            hubConnection.On<PlayerDTO>("PlayerLeft", (p) =>
            {
                Player player = new Player(p);
                if (players.Contains(player))
                {
                    players.Remove(player);
                    PlayerListChanged?.Invoke(this, null);
                    gameState.AddChatMessage(new ChatMessage(ChatMessageType.GameFlow, p.Name, p.Name + " left the game."));
                }
            });

            hubConnection.On<RoomStateDTO>("RoomCreated", (roomState) => AddRoom(roomState));
            hubConnection.On<RoomStateDTO>("RoomDeleted", (roomState) => RemoveRoom(roomState));

            hubConnection.On<RoomStateDTO>("RoomStateChanged", (state) =>
            {
                if (currentRoomState != null && state.RoomName.Equals(currentRoomState.RoomName))
                {
                    currentRoomState = state;
                }

                RoomStateDTO room = rooms.Where(r => r.RoomName.Equals(state.RoomName)).FirstOrDefault();
                if (room != null)
                {
                    room.GameInProgress = state.GameInProgress;
                    room.RoomSettings = state.RoomSettings;
                }
                RoomSettingsChanged?.Invoke(this, null);
            });

            hubConnection.On("GameStarted", () =>
            {
                foreach (Player p in players)
                {
                    p.Score = 0;
                    p.Position = null;
                }
                GameStarted?.Invoke(this, null);
            });

            hubConnection.On<int, int, ChatMessage>("RoundStarted", (currentRound, roundCount, chatMessage) =>
                gameState.NewRoundStart(currentRound, roundCount, chatMessage));

            hubConnection.On<ChatMessage>("ChatMessage", (cm) => gameState.AddChatMessage(cm));

            hubConnection.On<WordDTO, WordDTO, WordDTO, int>("ActivePlayerWordChoice", (w1, w2, w3, timeout) =>
                ActivePlayerWordChoiceStarted?.Invoke(this, new WordChoiceEventArgs(w1, w2, w3, timeout)));

            hubConnection.On<PlayerDTO, int>("PlayerWordChoice", (player, timeout) =>
                PlayerWordChoiceStarted?.Invoke(this, (player, timeout)));

            hubConnection.On<WordDTO, int>("ActivePlayerDrawing", (word, time) => 
                gameState.ActivePlayerDrawStart(word, time));

            hubConnection.On<PlayerDTO, string, int>("PlayerDrawing", (player, wordHint, time) =>
                gameState.PlayerDrawStart(player, wordHint, time));

            hubConnection.On<HintLetter>("HintLetter", (hint) => gameState.HintLetter(hint));
            hubConnection.On<PlayerDTO, int, WordDTO, ChatMessage>("CorrectGuess", (player, timeRemaining, word, chatMessage) =>
            {
                CorrectGuessMade?.Invoke(this, player);
                gameState.TurnTimer.SetTime(timeRemaining);
                if (word != null)
                {
                    gameState.CorrectWord(word);
                }
                if (chatMessage != null)
                {
                    gameState.AddChatMessage(chatMessage);
                }
            });
            hubConnection.On<List<PlayerScore>, int>("TurnScores", (scores, timeout) =>
            {
                gameState.TurnTimer.Stop();
                TurnScores?.Invoke(this, (scores, timeout));
            });
            hubConnection.On<List<PlayerScore>>("UpdateTotalScores", (scores) => UpdatePlayerScores(scores));
            hubConnection.On<List<PlayerScore>, int>("GameScores", (scores, timeout) => GameScores?.Invoke(this, (scores, timeout)));
            hubConnection.On("GameEnded", () => navigationManager.NavigateTo("/waitingroom"));
        }

        public IEnumerable<RoomStateDTO> Rooms => rooms;

        public IEnumerable<Player> Players => players;

        public RoomStateDTO RoomState => currentRoomState;

        public GameState GameState => gameState;

        public Guid? PlayerGuid => playerGuid;

        public async Task SetPlayerName(string userName)
        {
            playerGuid = await hubConnection.InvokeAsync<Guid>("SetPlayerName", userName);
        }

        private void AddRooms(IEnumerable<RoomStateDTO> newRooms)
        {
            rooms.AddRange(newRooms);
            RoomListChanged?.Invoke(this, null);
        }

        private void AddRoom(RoomStateDTO newRoom)
        {
            rooms.Add(newRoom);
            RoomListChanged?.Invoke(this, null);
        }

        private void RemoveRoom(RoomStateDTO room)
        {
            RoomStateDTO room2 = rooms.Where(r => room.RoomName.Equals(r.RoomName)).FirstOrDefault();
            if (room2 != null)
            {
                rooms.Remove(room2);
                RoomListChanged?.Invoke(this, null);
            }
        }

        public async Task<bool> CreateRoom(string roomName, RoomSettings roomSettings)
        {
            return await hubConnection.InvokeAsync<bool>("CreateRoom", roomName, roomSettings);
        }

        public async Task<bool> JoinRoom(string roomName, string password)
        {
            RoomStateDTO roomState = await hubConnection.InvokeAsync<RoomStateDTO>("JoinRoom", roomName, password);
            if (roomState == null)
            {
                players = new List<Player>();
                PlayerListChanged?.Invoke(this, null);
                currentRoomState = null;
                RoomSettingsChanged?.Invoke(this, null);
                return false;
            }
            else
            {
                players = roomState.Players.Select((p) => new Player(p)).ToList();
                PlayerListChanged?.Invoke(this, null);
                currentRoomState = roomState;
                RoomSettingsChanged?.Invoke(this, null);
                if (roomState.GameInProgress)
                {
                    navigationManager.NavigateTo("/room");
                }
                else
                {
                    navigationManager.NavigateTo("/waitingroom");
                }
                return true;
            }
        }
        public Task<bool> LeaveRoom()
        {
            return hubConnection.InvokeAsync<bool>("LeaveRoom");
        }

        public Task<bool> StartGame()
        {
            return hubConnection.InvokeAsync<bool>("StartGame");
        }

        public Task SetRoomSettings(RoomSettings roomSettings)
        {
            this.currentRoomState.RoomSettings = roomSettings;
            return hubConnection.InvokeAsync("SetRoomSettings", roomSettings);
        }

        public Task WordChosen(int wordIndex)
        {
            return hubConnection.InvokeAsync("WordChosen", wordIndex);
        }

        public Task MakeGuess(string guess)
        {
            return hubConnection.InvokeAsync("MakeGuess", guess);
        }

        public Task DrawLine(DrawLineEventArgs e)
        {
            return hubConnection.SendAsync("DrawLine", e);
        }

        public Task Fill(FillEventArgs e)
        {
            return hubConnection.SendAsync("Fill", e);
        }

        public Task ClearCanvas(string backgroundColor)
        {
            return hubConnection.SendAsync("ClearCanvas", backgroundColor);
        }

        public Task Undo()
        {
            return hubConnection.SendAsync("Undo");
        }

        public Task ChangeBackgroundColor(string color)
        {
            return hubConnection.SendAsync("ChangeBackgroundColor", color);
        }

        private void UpdatePlayerScores(List<PlayerScore> scores)
        {
            foreach (PlayerScore ps in scores)
            {
                foreach (Player p in players)
                {
                    if (p.Equals(ps.Player))
                    {
                        p.Score = ps.Score;
                    }
                }
            }

            UpdatePlayerPositions();
        }

        private void UpdatePlayerPositions()
        {
            List<Player> sortedPlayers = new List<Player>(players);
            sortedPlayers.Sort((p1, p2) => p2.Score.CompareTo(p1.Score));
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                if (i > 0 && sortedPlayers[i].Score == sortedPlayers[i - 1].Score)
                {
                    sortedPlayers[i].Position = sortedPlayers[i - 1].Position;
                }
                else
                {
                    sortedPlayers[i].Position = i + 1;
                }
            }
        }
    }
}
