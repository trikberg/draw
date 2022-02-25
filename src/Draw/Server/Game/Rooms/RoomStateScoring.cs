using Microsoft.AspNetCore.SignalR;
using Draw.Shared.Game;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System;

namespace Draw.Server.Game.Rooms
{
    internal class RoomStateScoring : IRoomState
    {
        private Player activePlayer;
        private Room room;
        private Word word;
        private List<(Player player, double timeRemaining)> playerResults;
        private List<Player> playersGuessing;
        private RoomStatePlayerTurn roomStatePlayerTurn;
        private GameTimer turnEndTimer;

        private List<(Player player, int score)> playerScores;

        public RoomStateScoring(Player player, 
                                Room room, 
                                Word word, 
                                List<(Player player, double timeRemaining)> playerResults, 
                                List<Player> playersGuessing, 
                                RoomStatePlayerTurn roomStatePlayerTurn)
        {
            this.activePlayer = player;
            this.room = room;
            this.word = word;
            this.playerResults = playerResults;
            this.playersGuessing = playersGuessing;
            this.roomStatePlayerTurn = roomStatePlayerTurn;
        }

        public async Task Enter()
        {
            int timeout = 8 + playerScores.Count;
            CalculateScores();
            await SendScores(timeout);
            roomStatePlayerTurn.Scores = playerScores;
            turnEndTimer = new GameTimer(timeout * 1000, TurnEndTimerElapsed);
            turnEndTimer.Start();
        }

        public async Task AddPlayer(Player player)
        {
            await roomStatePlayerTurn.AddPlayer(player);
            List<PlayerScore> turnScores =
                playerScores.Select(s => new PlayerScore(s.player.ToPlayerDTO(), s.score)).ToList();
            int timeRemaining = (int)(turnEndTimer.TimeRemaining / 1000);
            await room.SendPlayer(player, "TurnScores", turnScores, timeRemaining);

        }

        public Task RemovePlayer(Player player)
        {
            return Task.CompletedTask;
        }

        private void TurnEndTimerElapsed(object sender, ElapsedEventArgs e)
        {
            turnEndTimer.Dispose();
            room.RoomState = roomStatePlayerTurn;
        }

        private void CalculateScores()
        {
            playerScores = new List<(Player player, int score)>();
            foreach (Player p in playersGuessing)
            {
                playerScores.Add(new(p, 0));
            }

            if (playerResults.Count > 0)
            {
                double bestTime = playerResults[0].timeRemaining;
                foreach ((Player player, double timeRemaining) result in playerResults)
                {
                    int score = (int)(((result.timeRemaining / bestTime) * 100) + 0.5);
                    score = Math.Max(score, 10);
                    playerScores.Add(new(result.player, score));
                }
            }

            int totalGuesserCount = playersGuessing.Count + playerResults.Count;
            if (totalGuesserCount > 0)
            {
                int activePlayerScore;
                if (playerResults.Count == 0)
                {
                    activePlayerScore = -10;
                }
                else
                {
                    activePlayerScore = (int)((100.0 * playerResults.Count / totalGuesserCount) + 0.5);
                    activePlayerScore += word.Difficulty * 3;
                    activePlayerScore += word.CharacterCount * 3;
                }
                playerScores.Add(new(activePlayer, activePlayerScore));
            } 
            else
            {
                playerScores.Add(new(activePlayer, 0));
            }
        }

        private Task SendScores(int timeout)
        {
            List<PlayerScore> turnScores = 
                playerScores.Select(s => new PlayerScore(s.player.ToPlayerDTO(), s.score)).ToList();
            return room.SendAll("TurnScores", turnScores, word.ToWordDTO(), timeout);
        }
    }
}