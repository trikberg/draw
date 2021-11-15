using Draw.Shared.Draw;
using Draw.Shared.Game;
using System;
using System.Collections.Generic;

namespace Draw.Client.Services
{
    public class GameState
    {
        public event EventHandler RoundStarted;
        public event EventHandler<IDrawEventArgs> DrawEventReceived;
        public event EventHandler<IEnumerable<IDrawCommand>> UndoEventReceived;
        public event EventHandler<ChatMessage> ChatMessageReceived;

        private CommandList undoStack;
        private List<ChatMessage> chatLog = new List<ChatMessage>();

        public IEnumerable<ChatMessage> ChatLog => chatLog;
        public int CurrentRound { get; private set; }
        public int RoundCount { get; private set; }

        internal void NewRoundStarted(int currentRound, int roundCount, ChatMessage chatMessage)
        {
            CurrentRound = currentRound;
            RoundCount = roundCount;
            RoundStarted?.Invoke(this, EventArgs.Empty);
            if (chatMessage != null)
            {
                AddChatMessage(chatMessage);
            }
        }

        #region Drawing Commands
        internal void ResetUndoStack(string backgroundColor)
        {
            undoStack = new CommandList(backgroundColor);
        }

        internal void DrawLine(DrawLineEventArgs e)
        {
            undoStack.Add(e);
            DrawEventReceived?.Invoke(this, e);
        }

        internal void Fill(FillEventArgs e)
        {
            undoStack.Add(e);
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
        }

        internal void Undo()
        {
            IEnumerable<IDrawCommand> commands = undoStack.Undo();
            UndoEventReceived?.Invoke(this, commands);
        }

        internal IEnumerable<IDrawCommand> GetDrawBacklog()
        {
            if (undoStack == null)
            {
                return null;
            }
            return undoStack.GetDrawCommands();
        }

        internal void BackgroundColorChanged(string color)
        {
            undoStack.Add(new CommandBackground(color));
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
