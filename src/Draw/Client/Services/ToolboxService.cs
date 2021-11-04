using Microsoft.AspNetCore.Components.Web;
using Draw.Shared.Draw;
using System;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    internal class ToolboxService : IToolboxService, IDisposable
    {
        public event EventHandler<IDrawEventArgs> DrawEvent;
        public event EventHandler ClearCanvasEvent;
        public event EventHandler UndoEvent;
        public event EventHandler BrushColorChanged;
        public event EventHandler BrushSizeChanged;
        public event EventHandler<bool> BackgroundColorChanged;
        public event EventHandler<Tool> ActiveToolChanged;

        private IGameService gameService;
        private bool isActivePlayer = false;
        private bool isMouseDown = false;
        private Point2D mousePoint = null;

        private (int y, int x) activeColor = (4, 0);
        private string[,] colors = new string[,] {{ "#ffffff", "#fcab88", "#9090ff", "#90ffff", "#90ff90", "#ffff90", "#ffc699", "#ff9090", "#ff90ff", },
                                                  { "#c0c0c0", "#dd8858", "#4d4dff", "#4dffff", "#4dff4d", "#ffff4d", "#ff9f4c", "#ff4d4d", "#ff4dff", },
                                                  { "#808080", "#975528", "#0000ff", "#00ffff", "#00ff00", "#ffff00", "#fc6c00", "#ff0000", "#ff00ff", },
                                                  { "#404040", "#622f06", "#0000be", "#00bebe", "#00be00", "#bebe00", "#bf5900", "#be0000", "#be00be", },
                                                  { "#000000", "#431b02", "#000080", "#008080", "#008000", "#808000", "#7f3b00", "#800000", "#800080", }};
        private int brushSize = 2;
        private string backgroundColor = "#dddddd";
        private Tool activeTool = Tool.Brush;

        public ToolboxService(IGameService gameService)
        {
            this.gameService = gameService;
            gameService.BackgroundColorChanged += OnBackgroundColorChanged;
            gameService.ActivePlayerDrawStarted += OnActivePlayerDrawStarted;
            gameService.PlayerDrawStarted += OnPlayerDrawStarted;
        }

        public void Dispose()
        {
            gameService.BackgroundColorChanged -= OnBackgroundColorChanged;
            gameService.ActivePlayerDrawStarted -= OnActivePlayerDrawStarted;
        }

        private void OnPlayerDrawStarted(object sender, PlayerDrawEventArgs e)
        {
            isActivePlayer = false;
            ResetTools();
        }

        private void OnActivePlayerDrawStarted(object sender, ActivePlayerDrawEventArgs e)
        {
            isActivePlayer = true;
            ResetTools();
        }

        private void ResetTools()
        {
            activeTool = Tool.Brush;
            activeColor = (4, 0);
            brushSize = 2;
            backgroundColor = "#dddddd";
            ActiveToolChanged?.Invoke(this, activeTool);
            BrushColorChanged?.Invoke(this, null);
            BrushSizeChanged?.Invoke(this, null);
            BackgroundColorChanged?.Invoke(this, false);
        }

        private void OnBackgroundColorChanged(object sender, string color)
        {
            _ = SetBackgroundColor(color, false, true);
        }

        public string GetBrushColor(int x, int y)
        {
            return colors[y,x];
        }

        public void SetActiveBrushColor(int x, int y)
        {
            activeColor = (y, x);
            BrushColorChanged?.Invoke(this, null);
        }

        public bool IsActiveBrushColor(int x, int y)
        {
            return x == activeColor.x && y == activeColor.y;
        }

        public int GetBrushSize()
        {
            return brushSize;
        }

        public void SetBrushSize(int size)
        {
            brushSize = size;
            BrushSizeChanged?.Invoke(this, null);
        }

        public string GetBackgroundColor()
        {
            return backgroundColor;
        }

        public async Task SetBackgroundColor(string color, bool sendToServer, bool addToUndoStack)
        {
            if (addToUndoStack)
            {

            }
            backgroundColor = color;
            BackgroundColorChanged?.Invoke(this, addToUndoStack);
            if (sendToServer)
            {
                await gameService.ChangeBackgroundColor(color);
            }
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == 0)
            {
                mousePoint = new Point2D(e.OffsetX, e.OffsetY);
                isMouseDown = true;
            }
        }

        public async Task OnMouseLeave(MouseEventArgs e)
        {
            if (isMouseDown && (activeTool == Tool.Brush || activeTool == Tool.Erase))
            {
                isMouseDown = false;
                Point2D currentPoint = new Point2D(e.OffsetX, e.OffsetY);
                string color = activeTool switch
                {
                    Tool.Brush => colors[activeColor.y, activeColor.x],
                    Tool.Erase => "#00000000",
                    _ => "#000000"
                };
                DrawLineEventArgs args = new DrawLineEventArgs(mousePoint, currentPoint, brushSize, color, false);
                Task sendTask = gameService.DrawLine(args);
                DrawEvent?.Invoke(this, args);
                mousePoint = null;
                await sendTask;
            }
        }

        public async Task OnMouseMove(MouseEventArgs e)
        {
            if (isMouseDown && (activeTool == Tool.Brush || activeTool == Tool.Erase))
            {
                Point2D currentPoint = new Point2D(e.OffsetX, e.OffsetY);
                if (currentPoint.Distance(mousePoint) < 5.0)
                {
                    return;
                }
                string color = activeTool switch
                {
                    Tool.Brush => colors[activeColor.y, activeColor.x],
                    Tool.Erase => "#00000000",
                    _ => "#000000"
                };
                DrawLineEventArgs args = new DrawLineEventArgs(mousePoint, currentPoint, brushSize, color, false);
                Task sendTask = gameService.DrawLine(args);
                DrawEvent?.Invoke(this, args);
                mousePoint = currentPoint;
                await sendTask;
            }
        }

        public async Task OnMouseUp(MouseEventArgs e)
        {
            if (isMouseDown && (activeTool == Tool.Brush || activeTool == Tool.Erase))
            {
                Point2D currentPoint = new Point2D(e.OffsetX, e.OffsetY);
                string color = activeTool switch
                {
                    Tool.Brush => colors[activeColor.y, activeColor.x],
                    Tool.Erase => "#00000000",
                    _ => "#000000"
                };
                DrawLineEventArgs args = new DrawLineEventArgs(mousePoint, currentPoint, brushSize, color, true);
                Task sendTask = gameService.DrawLine(args);
                DrawEvent?.Invoke(this, args);
                mousePoint = null;
                isMouseDown = false;

                await sendTask;
            }
            else if (isMouseDown && activeTool == Tool.Fill)
            {
                Point2D currentPoint = new Point2D(e.OffsetX, e.OffsetY);
                FillEventArgs args = new FillEventArgs(currentPoint, colors[activeColor.y, activeColor.x]);
                DrawEvent?.Invoke(this, args);
                Task sendTask = gameService.Fill(args);
                mousePoint = null;
                isMouseDown = false;

                //await sendTask;
            }
            return;
        }

        public Tool GetActiveTool()
        {
            return activeTool;
        }

        public void SetActiveTool(Tool tool)
        {
            activeTool = tool;
            ActiveToolChanged?.Invoke(this, activeTool);
        }

        public async Task Undo()
        {
            if (isActivePlayer)
            {
                Task sendTask = gameService.Undo();
                UndoEvent?.Invoke(this, null);
                await sendTask;
            }
        }

        public async Task ClearCanvas()
        {
            if (isActivePlayer)
            {
                Task sendTask = gameService.ClearCanvas();
                ClearCanvasEvent?.Invoke(this, null);
                await sendTask;
            }
        }
    }
}
