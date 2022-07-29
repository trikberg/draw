using NLog;
using System.Threading.Tasks;
using System.Timers;

namespace Draw.Server.Game.Rooms
{
    internal class RoomStateWordChoice : IRoomState
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly int wordChoiceTimeout = 12;

        private Player activePlayer;
        private Room room;
        private RoomStatePlayerTurn roomStatePlayerTurn;
        private GameTimer wordChoiceTimer;

        private Word word1;
        private Word word2;
        private Word word3;

        private bool wordChoiceDone = false;

        public RoomStateWordChoice(Player player, Room room, RoomStatePlayerTurn roomStatePlayerTurn)
        {
            this.activePlayer = player;
            this.room = room;
            this.roomStatePlayerTurn = roomStatePlayerTurn;
            wordChoiceTimer = new GameTimer(wordChoiceTimeout * 1000, WordChoiceTimerElapsed);
            (word1, word2, word3) = room.GetNext3Words();
        }

        public async Task Enter()
        {
            await room.SendPlayer(activePlayer, "ActivePlayerWordChoice", word1.ToWordDTO(), word2.ToWordDTO(), word3.ToWordDTO(), wordChoiceTimeout);
            await room.SendAllExcept(activePlayer, "PlayerWordChoice", activePlayer.ToPlayerDTO(), wordChoiceTimeout);
            wordChoiceTimer.Start();
        }

        public async Task AddPlayer(Player player, bool isReconnect)
        {
            int timeRemaining = (int)(wordChoiceTimer.TimeRemaining / 1000);
            if (isReconnect && player.Equals(activePlayer))
            {
                await room.SendPlayer(activePlayer, "ActivePlayerWordChoice", word1.ToWordDTO(), word2.ToWordDTO(), word3.ToWordDTO(), wordChoiceTimeout);
            }
            else
            {
                await room.SendPlayer(player, "PlayerWordChoice", activePlayer.ToPlayerDTO(), timeRemaining);
            }
        }

        public Task RemovePlayer(Player player)
        {
            if (player.Equals(activePlayer))
            {
                room.RoomState = roomStatePlayerTurn;
            }
            return Task.CompletedTask;
        }

        private void WordChoiceTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            WordChosen(1, activePlayer);
        }

        internal void WordChosen(int wordIndex, Player player)
        {
            lock (this)
            {
                // Guard against user choosing word exactly when timer runs out.
                if (wordChoiceDone)
                {
                    return;
                }
                wordChoiceDone = true;
                wordChoiceTimer.Dispose();

                if (this.activePlayer.Equals(player))
                {
                    Word word;
                    switch (wordIndex)
                    {
                        case 1:
                            word = word1;
                            room.AddRejectedWord(word2);
                            room.AddRejectedWord(word3);
                            break;
                        case 2:
                            word = word2;
                            room.AddRejectedWord(word1);
                            room.AddRejectedWord(word3);
                            break;
                        case 3:
                        default:
                            word = word3;
                            room.AddRejectedWord(word1);
                            room.AddRejectedWord(word2);
                            break;
                    };

                    room.RoomState = new RoomStateDrawing(player, room, word, roomStatePlayerTurn);
                }
                else
                {
                    logger.Warn("Someone else (" + player.Name + ") than active player (" + player.Name + ") tried to choose a word.");
                }
            }
        }
    }
}