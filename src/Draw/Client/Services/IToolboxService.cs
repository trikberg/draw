using Microsoft.AspNetCore.Components.Web;
using Draw.Shared.Draw;
using System;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    public enum Tool
    {
        Brush,
        Erase,
        Fill
    }

    internal interface IToolboxService
    {
        public event EventHandler BrushColorChanged;
        public event EventHandler BrushSizeChanged;
        public event EventHandler<bool> BackgroundColorChanged;
        public event EventHandler<Tool> ActiveToolChanged;

        public int BrushSize { get; set; }
        public Tool ActiveTool { get; set; }

        public string GetBrushColor(int x, int y);
        public void SetActiveBrushColor(int x, int y);
        public bool IsActiveBrushColor(int x, int y);
        public string GetBackgroundColor();
        public Task SetBackgroundColor(string color, bool sendToServer, bool addToUndoStack);

        public Task OnMouseMove(MouseEventArgs e);
        public void OnMouseDown(MouseEventArgs e);
        public Task OnMouseUp(MouseEventArgs e);
        public Task OnMouseLeave(MouseEventArgs e);

        public Task Undo();
        public Task ClearCanvas();
    }
}
