using Draw.Shared.Draw;
using System;
using System.Collections.Generic;

namespace Draw.Client.Services
{
    public class GameState
    {
        public event EventHandler<IDrawEventArgs> DrawEventReceived;
        public event EventHandler<IEnumerable<IDrawCommand>> UndoEventReceived;

        private CommandList undoStack;

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
    }
}
