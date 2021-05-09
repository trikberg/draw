using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Draw.Server.Game.Rooms
{
    internal class RoomStatePlayerTurn : IRoomState
    {
        private Player player;
        private Room room;
        private RoomStateRound roomRoundState;
        private int entryCount = 0;

        public RoomStatePlayerTurn(Player player, Room room, RoomStateRound roomRoundState)
        {
            this.player = player;
            this.room = room;
            this.roomRoundState = roomRoundState;
        }

        public List<(Player player, int score)> Scores { get; internal set; } = new();

        public async Task Enter()
        {
            switch (entryCount)
            {
                case 0: 
                    await StartPlayerTurn();
                    break;
                case 1:
                    await EndPlayerTurn();
                    break;
                default: throw new InvalidOperationException("Unknown entryCount value: " + entryCount + ".");
            }
            entryCount++;
        }

        public async Task AddPlayer(Player player)
        {
            await roomRoundState.AddPlayer(player);
            await room.SendPlayer(player, "ChatMessage", new ChatMessage(ChatMessageType.GameFlow,
                                                                         null,
                                                                         this.player.Name + "'s turn to draw."));
        }

        public Task RemovePlayer(Player player)
        {
            return Task.CompletedTask;
        }

        private async Task EndPlayerTurn()
        {
            await roomRoundState.AddScore(Scores);
            room.RoomState = roomRoundState;
        }

        private async Task StartPlayerTurn()
        {
            await room.SendAll("ChatMessage", new ChatMessage(ChatMessageType.GameFlow,
                                                              null,
                                                              player.Name + "'s turn to draw."));
            room.RoomState = new RoomStateWordChoice(player, room, this);
        }
    }
}