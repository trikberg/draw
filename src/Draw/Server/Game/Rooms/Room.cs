using Microsoft.AspNetCore.SignalR;
using Draw.Server.Hubs;
using Draw.Shared.Draw;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Server.Game.Rooms
{
    public class Room
    {
        private List<Word> unusedWords = null;
        private List<Word> rejectedWords = null;
        private int wordCount = 0;
        private List<Player> players = new List<Player>();
        private IHubContext<GameHub> hubContext;
        private IRoomState roomState;

        public Room(IHubContext<Hubs.GameHub> context, string name) : this(context, name, new RoomSettings())
        {
        }

        public Room(IHubContext<Hubs.GameHub> context, string name, RoomSettings settings)
        {
            hubContext = context;
            RoomName = name;
            RoomSettings = settings;
            RoomState = new RoomStateLobby(this);
        }

        internal IHubContext<GameHub> HubContext => hubContext;
        internal List<Word> UnusedWords => unusedWords;
        public List<Player> Players => players;

        public string RoomName { get; }

        public RoomSettings RoomSettings { get; internal set; }

        internal IRoomState RoomState
        {
            get { return roomState; }
            set
            {
                if (value != null && roomState != value)
                {
                    roomState = value;
                    _ = roomState.Enter();
                }
            }
        }

        public int RoundCount => RoomSettings.Rounds;

        public bool IsGameInProgress => !(roomState is RoomStateLobby);

        public async Task AddPlayer(Player player)
        {
            players.Add(player);
            await roomState.AddPlayer(player);
        }

        internal RoomStateDTO ToRoomStateDTO()
        {
            List<PlayerDTO> playersDTO;
            lock (players)
            {
                playersDTO = players.Select(p => p.ToPlayerDTO()).ToList();
            }
            return new RoomStateDTO(RoomName, playersDTO, RoomSettings, IsGameInProgress);
        }

        public async Task<bool> RemovePlayer(Player player)
        {
            if (players.Remove(player))
            {
                await roomState.RemovePlayer(player);
                return true;
            }
            return false;
        }

        internal async Task MakeGuess(Player player, string guess)
        {
            if (roomState is RoomStateDrawing rsd)
            {
                await rsd.MakeGuess(player, guess);
            }
            else
            {
                ChatMessage cm = new ChatMessage(ChatMessageType.Chat, player.Name, guess);
                await SendAll("ChatMessage", cm);
            }
        }

        internal bool StartGame(Player player)
        {
            if (player.Equals(players.FirstOrDefault()) &&
                RoomState is RoomStateLobby rsl)
            {
                RoomState = new RoomStateGame(this, rsl);
                return true;
            }
            return false;
        }

        internal void WordChosen(int wordIndex, Player player)
        {
            if (RoomState is RoomStateWordChoice rswc)
            {
                rswc.WordChosen(wordIndex, player);
            }
        }

        internal (Word, Word, Word) GetNext3Words()
        {
            if (unusedWords == null)
            {
                GetFreshWordList();
            }

            List<Word> selectedWords = new List<Word>(3); 

            while (selectedWords.Count < 3)
            {
                if (unusedWords.Count == 0)
                {
                    if (rejectedWords.Count > 0.2 * wordCount)
                    {
                        unusedWords = rejectedWords;
                        rejectedWords = new List<Word>();
                    }
                    else
                    {
                        GetFreshWordList();
                    }
                }

                Word wordCandidate = unusedWords[0];
                unusedWords.RemoveAt(0);
                if (RoomSettings.WordDifficultyDelta > 1 &&
                    selectedWords.Count((w) => w.Difficulty == wordCandidate.Difficulty) > 0)
                {
                    unusedWords.Add(wordCandidate);
                }
                else
                {
                    selectedWords.Add(wordCandidate);
                }
            }

            return (selectedWords[0], selectedWords[1], selectedWords[2]);
        }

        private void GetFreshWordList()
        {
            unusedWords = WordProvider.Instance.GetWords(RoomSettings.Language,
                                                         RoomSettings.MinWordDifficulty,
                                                         RoomSettings.MaxWordDifficulty);
            wordCount = unusedWords.Count;
            rejectedWords = new List<Word>();
        }

        internal void AddRejectedWord(Word word)
        {
            rejectedWords.Add(word);
        }

        internal async Task<bool> SetRoomSettings(RoomSettings settings, Player player)
        {
            if (RoomState is RoomStateLobby rsl)
            {
                return await rsl.SetRoomSettings(settings, player);
            }
            return false;
        }

        internal async Task ChangeBackgroundColor(Player player, string color)
        {
            if (RoomState is RoomStateDrawing rsd)
            {
                await rsd.ChangeBackgroundColor(player, color);
            }
        }

        internal async Task DrawLine(Player player, DrawLineEventArgs e)
        {
            if (RoomState is RoomStateDrawing rsd)
            {
                await rsd.DrawLine(player, e);
            }
        }

        internal async Task Fill(Player player, FillEventArgs e)
        {
            if (RoomState is RoomStateDrawing rsd)
            {
                await rsd.Fill(player, e);
            }
        }

        internal async Task ClearCanvas(Player player)
        {
            if (RoomState is RoomStateDrawing rsd)
            {
                await rsd.ClearCanvas(player);
            }
        }

        internal async Task Undo(Player player)
        {
            if (RoomState is RoomStateDrawing rsd)
            {
                await rsd.Undo(player);
            }
        }

        #region SignalR send methods
        internal Task SendAll(string method)
        {
            return HubContext.Clients.Group(RoomName).SendAsync(method);
        }

        internal Task SendAll(string method, object arg)
        {
            return HubContext.Clients.Group(RoomName).SendAsync(method, arg);
        }

        internal Task SendAll(string method, object arg1, object arg2)
        {
            return HubContext.Clients.Group(RoomName).SendAsync(method, arg1, arg2);
        }

        internal Task SendAll(string method, object arg1, object arg2, object arg3)
        {
            return HubContext.Clients.Group(RoomName).SendAsync(method, arg1, arg2, arg3);
        }

        internal Task SendAllExcept(Player player, string method)
        {
            return HubContext.Clients.GroupExcept(RoomName, player.ConnectionId).SendAsync(method);
        }

        internal Task SendAllExcept(Player player, string method, object arg)
        {
            return HubContext.Clients.GroupExcept(RoomName, player.ConnectionId).SendAsync(method, arg);
        }

        internal Task SendAllExcept(Player player, string method, object arg1, object arg2)
        {
            return HubContext.Clients.GroupExcept(RoomName, player.ConnectionId).SendAsync(method, arg1, arg2);
        }

        internal Task SendAllExcept(Player player, string method, object arg1, object arg2, object arg3)
        {
            return HubContext.Clients.GroupExcept(RoomName, player.ConnectionId).SendAsync(method, arg1, arg2, arg3);
        }

        internal Task SendAllExcept(Player player, string method, object arg1, object arg2, object arg3, object arg4)
        {
            return HubContext.Clients.GroupExcept(RoomName, player.ConnectionId).SendAsync(method, arg1, arg2, arg3, arg4);
        }

        internal Task SendPlayer(Player player, string method)
        {
            return HubContext.Clients.Client(player.ConnectionId).SendAsync(method);
        }

        internal Task SendPlayer(Player player, string method, object arg)
        {
            return HubContext.Clients.Client(player.ConnectionId).SendAsync(method, arg);
        }

        internal Task SendPlayer(Player player, string method, object arg1, object arg2)
        {
            return HubContext.Clients.Client(player.ConnectionId).SendAsync(method, arg1, arg2);
        }

        internal Task SendPlayer(Player player, string method, object arg1, object arg2, object arg3)
        {
            return HubContext.Clients.Client(player.ConnectionId).SendAsync(method, arg1, arg2, arg3);
        }

        internal Task SendPlayer(Player player, string method, object arg1, object arg2, object arg3, object arg4)
        {
            return HubContext.Clients.Client(player.ConnectionId).SendAsync(method, arg1, arg2, arg3, arg4);
        }
        #endregion
    }
}
