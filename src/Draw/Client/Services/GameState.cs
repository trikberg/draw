using Draw.Shared.Draw;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Draw.Client.Services
{
    public class GameState
    {
        public event EventHandler? RoundStarted;
        public event EventHandler? ActivePlayerDrawStarted;
        public event EventHandler<PlayerDrawEventArgs>? PlayerDrawStarted;
        public event EventHandler? HintLetterReceived;
        public event EventHandler? CorrectWordReceived;
        public event EventHandler<(List<PlayerScore> scores, WordDTO word, int timeout)>? TurnScores;
        public event EventHandler<IDrawEventArgs>? DrawEventReceived;
        public event EventHandler<IEnumerable<IDrawCommand>>? UndoEventReceived;
        public event EventHandler<ChatMessage>? ChatMessageReceived;
        public event EventHandler? ClearCanvasReceived;

        private CommandList? undoStack;
        private List<ChatMessage> chatLog = new List<ChatMessage>();

        public IEnumerable<ChatMessage> ChatLog => chatLog;
        public int CurrentRound { get; private set; } = 0;
        public int RoundCount { get; private set; } = 0;
        public TurnTimer TurnTimer { get; } = new TurnTimer();

        public WordDTO? Word { get; private set; } = null;
        public string? WordHint { get; private set; } = null;

        internal GameState()
        {
        }

        internal void NewRoundStart(int currentRound, int roundCount, ChatMessage chatMessage)
        {
            CurrentRound = currentRound;
            RoundCount = roundCount;
            RoundStarted?.Invoke(this, EventArgs.Empty);
            if (chatMessage != null)
            {
                AddChatMessage(chatMessage);
            }
        }

        internal void ActivePlayerDrawStart(WordDTO word, int time)
        {
            WordHint = WordDTO.GetHintWord(word.Word);
            Word = word;
            ResetUndoStack(CanvasSettings.DEFAULT_BACKGROUND_COLOR);
            TurnTimer.StartTimer(time);
            ActivePlayerDrawStarted?.Invoke(this, EventArgs.Empty);
        }

        internal void PlayerDrawStart(PlayerDTO player, string wordHint, int time)
        {
            Word = null;
            WordHint = wordHint;
            ResetUndoStack(CanvasSettings.DEFAULT_BACKGROUND_COLOR);
            TurnTimer.StartTimer(time);
            PlayerDrawStarted?.Invoke(this, new PlayerDrawEventArgs(player, time));
        }

        internal void HintLetter(HintLetter hint)
        {
            StringBuilder sb = new StringBuilder(WordHint);
            sb[hint.Position] = hint.Letter;
            WordHint = sb.ToString();
            StringBuilder wordHintMessage = new StringBuilder("Hint:");
            foreach (char c in WordHint)
            {
                wordHintMessage.Append(' ');
                wordHintMessage.Append(c);
            }
            AddChatMessage(new ChatMessage(ChatMessageType.GameFlow, null, wordHintMessage.ToString()));
            HintLetterReceived?.Invoke(this, EventArgs.Empty);
        }

        internal void CorrectWord(WordDTO correctWord)
        {
            Word = correctWord;
            CorrectWordReceived?.Invoke(this, EventArgs.Empty);
        }

        internal void TurnScoresReceived(List<PlayerScore> scores, WordDTO word, int timeout)
        {
            Word = word;
            TurnTimer.Stop();
            TurnScores?.Invoke(this, (scores, word, timeout));
        }

        #region Drawing Commands
        internal void ResetUndoStack(string backgroundColor)
        {
            undoStack = new CommandList(backgroundColor);
        }

        internal void DrawLine(DrawLineEventArgs e)
        {
            (undoStack ??= new CommandList(CanvasSettings.DEFAULT_BACKGROUND_COLOR)).Add(e);
            DrawEventReceived?.Invoke(this, e);
        }

        internal void Fill(FillEventArgs e)
        {
            (undoStack ??= new CommandList(CanvasSettings.DEFAULT_BACKGROUND_COLOR)).Add(e);
            DrawEventReceived?.Invoke(this, e);
        }

        internal void ClearCanvas(string backgroundColor)
        {
            if (undoStack == null)
            {
                undoStack = new CommandList(backgroundColor);
            }
            else
            {
                undoStack.Add(new CommandClearCanvas(backgroundColor));
            }
            ClearCanvasReceived?.Invoke(this, EventArgs.Empty);
        }

        internal void Undo()
        {
            IEnumerable<IDrawCommand> commands = (undoStack ??= new CommandList(CanvasSettings.DEFAULT_BACKGROUND_COLOR)).Undo();
            UndoEventReceived?.Invoke(this, commands);
        }

        internal IEnumerable<IDrawCommand>? GetDrawBacklog()
        {
            if (undoStack == null)
            {
                return null;
            }
            return undoStack.GetDrawCommands();
        }

        internal void BackgroundColorChanged(string color)
        {
            (undoStack ??= new CommandList(color)).Add(new CommandBackground(color));
        }
        #endregion Drawing Commands

        #region Chat Messages
        internal void AddChatMessage(ChatMessage cm)
        {
            if (chatLog.Count >= 50)
            {
                chatLog.RemoveRange(0, chatLog.Count - 49);
            }
            chatLog.Add(cm);
            ChatMessageReceived?.Invoke(this, cm);
        }

        #endregion Chat Message
    }
}
