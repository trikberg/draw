using Draw.Shared.Draw;
using Draw.Shared.Game;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Draw.Server.Game.Rooms
{
    internal class RoomStateDrawing : IRoomState
    {
        private static Random random = new Random(new Guid().GetHashCode());
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Player activePlayer;
        private Room room;
        private Word word;
        private RoomStatePlayerTurn roomStatePlayerTurn;

        private int turnTime;
        private GameTimer timer;
        private List<HintLetter> hintsSent = new List<HintLetter>();
        private GameTimer hintTimer;
        private List<Player> playersGuessing;
        private List<(Player player, double timeRemaining)> playerResults = new List<(Player player, double timeRemaining)>();

        private string currentBackgroundColor = CanvasSettings.DEFAULT_BACKGROUND_COLOR;
        private CommandList drawCommandLog = new CommandList(CanvasSettings.DEFAULT_BACKGROUND_COLOR);
        private List<ChatMessage> chatLog = new List<ChatMessage>();

        private bool turnEnded = false;

        public RoomStateDrawing(Player player, Room room, Word word, RoomStatePlayerTurn roomStatePlayerTurn)
        {
            this.activePlayer = player;
            this.room = room;
            this.word = word;
            this.roomStatePlayerTurn = roomStatePlayerTurn;
            this.turnTime = room.RoomSettings.DrawingTime;
        }

        public async Task Enter()
        {
            playersGuessing = room.Players.Where(p => !p.Equals(this.activePlayer)).ToList();
            string wordHint = WordDTO.GetHintWord(word.TheWord);
            await room.SendPlayer(activePlayer, "ActivePlayerDrawing", word.ToWordDTO(), turnTime);
            await room.SendAllExcept(activePlayer, "PlayerDrawing", activePlayer.ToPlayerDTO(), wordHint, turnTime);
            timer = new GameTimer(turnTime * 1000, TimerElapsed);
            hintTimer = new GameTimer(turnTime * 333, HintTimerElapsed);
            timer.Start();
            hintTimer.Start();
        }

        public async Task AddPlayer(Player player)
        {
            await roomStatePlayerTurn.AddPlayer(player);
            playersGuessing.Add(player);
            string wordHint = WordDTO.GetHintWord(word.TheWord);
            int turnTimeLeft = (int) (timer.TimeRemaining / 1000);
            await room.SendPlayer(player, "PlayerDrawing", activePlayer.ToPlayerDTO(), wordHint, turnTimeLeft);
            foreach (HintLetter hl in hintsSent)
            {
                await room.SendPlayer(player, "HintLetter", hl);
            }
            foreach((Player player, double timeRemaining) result in playerResults)
            {
                await room.SendPlayer(player, "CorrectGuess", result.player.ToPlayerDTO(), null);
            }
            foreach(ChatMessage cm in chatLog)
            {
                await room.SendPlayer(player, "ChatMessage", cm);
            }
        }

        public Task RemovePlayer(Player player)
        {
            roomStatePlayerTurn.RemovePlayer(player);
            if (player.Equals(activePlayer))
            {
                EndTurn();
            }
            else
            {
                playersGuessing.Remove(player);
                if (playersGuessing.Count == 0)
                {
                    EndTurn();
                }
            }
            return Task.CompletedTask;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            EndTurn();
        }

        private void HintTimerElapsed(object sender, ElapsedEventArgs e)
        {
            HintLetter hint = GetWordHint();
            if (hint != null)
            {
                hintsSent.Add(hint);
                _ = room.SendAll("HintLetter", hint);
                if (hintsSent.Count == 1 && word.TheWord.Length < 9)
                {
                    hintTimer.Reset(turnTime * 333);
                }
                else if (hintsSent.Count == 1 && word.TheWord.Length >= 9)
                {
                    hintTimer.Reset(turnTime * 222);
                }
                else if (hintsSent.Count == 2 && word.TheWord.Length >= 9)
                {
                    hintTimer.Reset(turnTime * 222);
                }
            }
        }

        private HintLetter GetWordHint()
        {
            if (hintsSent.Count >= word.TheWord.Length - 1)
            {
                return null;
            }

            int i;
            do
            {
                i = random.Next(word.TheWord.Length);
            } while (!Char.IsLetter(word.TheWord[i]) || hintsSent.Where(hint => hint.Position == i).Count() > 0);
            return new HintLetter(i, word.TheWord[i]);
        }

        internal async Task MakeGuess(Player player, string guess)
        {
            if (!playersGuessing.Contains(player))
            {
                ChatMessage cm = new ChatMessage(ChatMessageType.AlreadyGuessed, player.Name, CensorMessage(guess));
                chatLog.Add(cm);
                await room.SendAll("ChatMessage", cm);
            }
            else
            {
                if (guess.Trim().Equals(word.TheWord, StringComparison.InvariantCultureIgnoreCase))
                {
                    playersGuessing.Remove(player);
                    playerResults.Add(new(player, timer.TimeRemaining));
                    double timeRemaining;
                    if (playerResults.Count <= 2)
                    {
                        timeRemaining = timer.Change(0.75);
                    }
                    else
                    {
                        timeRemaining = timer.TimeRemaining;
                    }
                    int turnTimeLeft = (int)(timeRemaining / 1000);
                    ChatMessage cm = new ChatMessage(ChatMessageType.CorrectGuess, player.Name, player.Name + " guessed correctly.");
                    chatLog.Add(cm);
                    await room.SendAllExcept(player, "CorrectGuess", player.ToPlayerDTO(), turnTimeLeft, null, cm);
                    await room.SendPlayer(player, "CorrectGuess", player.ToPlayerDTO(), turnTimeLeft, word.ToWordDTO(), cm);
                    if (playersGuessing.Count == 0)
                    {
                        EndTurn();
                    }
                }
                else
                {
                    ChatMessage cm = new ChatMessage(ChatMessageType.Guess, player.Name, guess);
                    chatLog.Add(cm);
                    await room.SendAll("ChatMessage", cm);
                }
            }
        }

        internal async Task ChangeBackgroundColor(Player player, string color)
        {
            if (activePlayer.Equals(player))
            {
                await room.SendAllExcept(player, "ChangeBackgroundColor", color);
                currentBackgroundColor = color;
            }
            else
            {
                logger.Warn("Someone else (" + player.Name + ") than active player (" + player.Name + ") tried to draw.");
            }
        }

        internal async Task DrawLine(Player player, DrawLineEventArgs e)
        {
            if (activePlayer.Equals(player))
            {
                await room.SendAllExcept(player, "DrawLine", e);
                drawCommandLog.Add(e);
            }
            else
            {
                logger.Warn("Someone else (" + player.Name + ") than active player (" + player.Name + ") tried to draw.");
            }
        }

        internal async Task Fill(Player player, FillEventArgs e)
        {
            if (activePlayer.Equals(player))
            {
                await room.SendAllExcept(player, "Fill", e);
                drawCommandLog.Add(e);
            }
            else
            {
                logger.Warn("Someone else (" + player.Name + ") than active player (" + player.Name + ") tried to draw.");
            }
        }

        internal async Task ClearCanvas(Player player)
        {
            if (activePlayer.Equals(player))
            {
                await room.SendAllExcept(player, "ClearCanvas");
                drawCommandLog.Add(new CommandClearCanvas(currentBackgroundColor));
            }
            else
            {
                logger.Warn("Someone else (" + player.Name + ") than active player (" + player.Name + ") tried to draw.");
            }
        }


        internal async Task Undo(Player player)
        {
            if (activePlayer.Equals(player))
            {
                await room.SendAllExcept(player, "Undo");
                drawCommandLog.Undo();
            }
            else
            {
                logger.Warn("Someone else (" + player.Name + ") than active player (" + player.Name + ") tried to draw.");
            }
        }

        private void EndTurn()
        {
            lock (this)
            {
                if (!turnEnded)
                {
                    turnEnded = true;
                    timer.Dispose();
                    hintTimer.Dispose();
                    ChatMessage cm = new ChatMessage(ChatMessageType.GameFlow, null, "The word was \"" + word.TheWord + "\".");
                    chatLog.Add(cm);
                    _ = room.SendAll("ChatMessage", cm);
                    room.RoomState = new RoomStateScoring(activePlayer, room, word.Difficulty, playerResults, playersGuessing, roomStatePlayerTurn);
                }
            }
        }

        private string CensorMessage(string guess)
        {
            return guess.Replace(word.TheWord, "gobbledygook", true, null);
        }
    }
}