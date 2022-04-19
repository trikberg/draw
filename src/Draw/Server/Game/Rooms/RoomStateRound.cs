using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Server.Game.Rooms
{
    internal class RoomStateRound : IRoomState
    {
        private int roundNumber;
        private Room room;
        private RoomStateGame roomStateGame;
        private int entryCount = 0;
        private List<Player> playersAlreadyDrawn = new List<Player>();

        public RoomStateRound(int roundNumber, Room room, RoomStateGame roomStateGame)
        {
            this.roundNumber = roundNumber;
            this.room = room;
            this.roomStateGame = roomStateGame;
        }

        public async Task Enter()
        {
            if (entryCount == 0)
            {
                ChatMessage cm = new ChatMessage(ChatMessageType.GameFlow,
                                                 null,
                                                 "Round " + (roundNumber + 1) + " starting.");
                await room.SendAll("RoundStarted", roundNumber + 1, room.RoomSettings.Rounds, cm);
            }

            IEnumerable<Player> remainingPlayers = room.Players.Except(playersAlreadyDrawn).Where(p => p.IsConnected);
            if (remainingPlayers.Count() > 0)
            {
                Player player = remainingPlayers.First();
                playersAlreadyDrawn.Add(player);
                room.RoomState = new RoomStatePlayerTurn(player, room, this);
            }
            else
            {
                room.RoomState = roomStateGame;
            }
            entryCount++;
        }

        public async Task AddPlayer(Player player, bool isReconnect)
        {
            await roomStateGame.AddPlayer(player, isReconnect);
            ChatMessage cm = new ChatMessage(ChatMessageType.GameFlow,
                                             null,
                                             "Round " + (roundNumber + 1) + " starting.");
            await room.SendPlayer(player, "RoundStarted", roundNumber + 1, room.RoomSettings.Rounds, cm);
        }

        public Task RemovePlayer(Player player)
        {
            return Task.CompletedTask;
        }

        internal Task AddScore(List<(Player player, int score)> scores)
        {
            return roomStateGame.AddScore(scores);
        }
    }
}