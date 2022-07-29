using Draw.Shared.Game;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Draw.Server.Game.Rooms
{
    internal class RoomStateGame : IRoomState
    {
        private int entryCount = 0;
        private Room room;
        private RoomStateLobby roomStateLobby;
        private Dictionary<Player, int> playerScores = new Dictionary<Player, int>();
        private Timer? gameEndTimer;

        public RoomStateGame(Room room, RoomStateLobby rsl)
        {
            this.room = room;
            this.roomStateLobby = rsl;
        }

        public async Task Enter()
        {
            if (entryCount == 0)
            {
               await room.SendAll("GameStarted");
            }

            if (room.Players.Where(p => p.IsConnected).Count() == 0)
            {
                room.RoomState = roomStateLobby;
                return;
            }

            if (entryCount < room.RoundCount)
            {
                room.RoomState = new RoomStateRound(entryCount, room, this);
            }
            else
            {
                int timeout = 12 + (2 * playerScores.Count);
                await GameScores(timeout);
                gameEndTimer = new Timer(timeout * 1000);
                gameEndTimer.Elapsed += GameEndTimerElapsed;
                gameEndTimer.AutoReset = false;
                gameEndTimer.Start();
            }
            entryCount++;
        }

        public async Task AddPlayer(Player player, bool isReconnect)
        {
            await room.SendPlayer(player, "GameStarted");
            List<PlayerScore> totalScores = GetTotalScores();
            await room.SendPlayer(player, "UpdateTotalScores", totalScores);
        }

        public Task RemovePlayer(Player player)
        {
            return Task.CompletedTask;
        }

        private void GameEndTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            gameEndTimer?.Stop();
            gameEndTimer?.Dispose();
            room.RoomState = roomStateLobby;
            _ = room.SendAll("GameEnded");
        }

        internal Task AddScore(List<(Player player, int score)> scores)
        {
            lock (playerScores)
            {
                foreach ((Player player, int score) newScore in scores)
                {
                    if (playerScores.TryGetValue(newScore.player, out int currentScore))
                    {
                        playerScores.Remove(newScore.player);
                        playerScores.Add(newScore.player, currentScore + newScore.score);
                    }
                    else
                    {
                        playerScores.Add(newScore.player, newScore.score);
                    }
                }
            }
            return SendTotalScores();
        }

        private Task SendTotalScores()
        {
            List<PlayerScore> totalScores = GetTotalScores();
            return room.SendAll("UpdateTotalScores", totalScores);
        }

        private Task GameScores(int timeout)
        {
            List<PlayerScore> totalScores = GetTotalScores();
            return room.SendAll("GameScores", totalScores, timeout);
        }

        private List<PlayerScore> GetTotalScores()
        {
            List<PlayerScore> totalScores;
            lock (playerScores)
            {
                totalScores = playerScores.Select(s => new PlayerScore(s.Key.ToPlayerDTO(), s.Value)).ToList();
            }
            return totalScores;
        }
    }
}