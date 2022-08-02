using Microsoft.AspNetCore.Components.Web;
using Draw.Shared.Draw;
using System;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    internal class ToolboxService : IToolboxService, IDisposable
    {
        public event EventHandler? BrushColorChanged;
        public event EventHandler? BrushSizeChanged;
        public event EventHandler<bool>? BackgroundColorChanged;
        public event EventHandler<Tool>? ActiveToolChanged;

        private IGameService gameService;
        private IKeyboardCommandService keyboardCommandService;
        private bool isActivePlayer = false;
        private bool isMouseDown = false;
        private Point2D? mousePoint = null;

        private (int y, int x) activeColor = (4, 0);
        private string[,] colors = new string[,] {{ "#ffffff", "#fcab88", "#9090ff", "#90ffff", "#90ff90", "#ffff90", "#ffc699", "#ff9090", "#ff90ff", },
                                                  { "#c0c0c0", "#dd8858", "#4d4dff", "#4dffff", "#4dff4d", "#ffff4d", "#ff9f4c", "#ff4d4d", "#ff4dff", },
                                                  { "#808080", "#975528", "#0000ff", "#00ffff", "#00ff00", "#ffff00", "#fc6c00", "#ff0000", "#ff00ff", },
                                                  { "#404040", "#622f06", "#0000be", "#00bebe", "#00be00", "#bebe00", "#bf5900", "#be0000", "#be00be", },
                                                  { "#000000", "#431b02", "#000080", "#008080", "#008000", "#808000", "#7f3b00", "#800000", "#800080", }};
        private int brushSize = CanvasSettings.DEFAULT_BRUSH_SIZE;
        private string backgroundColor = CanvasSettings.DEFAULT_BACKGROUND_COLOR;
        private Tool activeTool = Tool.Brush;

        public ToolboxService(IGameService gameService, IKeyboardCommandService keyboardCommandService)
        {
            this.gameService = gameService;
            this.keyboardCommandService = keyboardCommandService;
            gameService.BackgroundColorChanged += OnBackgroundColorChanged;
            gameService.GameState.ActivePlayerDrawStarted += OnActivePlayerDrawStarted;
            gameService.GameState.PlayerDrawStarted += OnPlayerDrawStarted;
            keyboardCommandService.KeyboardShortCutHit += KeyboardShortCutHit;
        }

        public void Dispose()
        {
            gameService.BackgroundColorChanged -= OnBackgroundColorChanged;
            gameService.GameState.ActivePlayerDrawStarted -= OnActivePlayerDrawStarted;
            gameService.GameState.PlayerDrawStarted -= OnPlayerDrawStarted;
            keyboardCommandService.KeyboardShortCutHit -= KeyboardShortCutHit;
        }

        public int BrushSize
        {
            get { return brushSize; }
            set
            {
                if (brushSize != value)
                {
                    brushSize = value;
                    BrushSizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Tool ActiveTool
        {
            get { return activeTool; }
            set
            {
                if (activeTool != value)
                {
                    activeTool = value;
                    ActiveToolChanged?.Invoke(this, activeTool);
                }
            }
        }

        private void OnPlayerDrawStarted(object? sender, PlayerDrawEventArgs _)
        {
            isActivePlayer = false;
            ResetTools();
        }

        private void OnActivePlayerDrawStarted(object? sender, EventArgs _)
        {
            isActivePlayer = true;
            isMouseDown = false;
            ResetTools();
        }

        private async void KeyboardShortCutHit(object? sender, KeyboardShortcuts shortcut)
        {
            switch (shortcut)
            {
                case KeyboardShortcuts.Undo:
                    await Undo();
                    break;
                case KeyboardShortcuts.Brush:
                    ActiveTool = Tool.Brush;
                    break;
                case KeyboardShortcuts.Fill:
                    ActiveTool = Tool.Fill;
                    break;
                case KeyboardShortcuts.Erase:
                    ActiveTool = Tool.Erase;
                    break;
            }
        }

        private void ResetTools()
        {
            ActiveTool = Tool.Brush;
            BrushSize = CanvasSettings.DEFAULT_BRUSH_SIZE;
            activeColor = (4, 0);
            BrushColorChanged?.Invoke(this, EventArgs.Empty);
            backgroundColor = CanvasSettings.DEFAULT_BACKGROUND_COLOR;
            BackgroundColorChanged?.Invoke(this, false);
        }

        private void OnBackgroundColorChanged(object? sender, string color)
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
            BrushColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsActiveBrushColor(int x, int y)
        {
            return x == activeColor.x && y == activeColor.y;
        }

        public string GetBackgroundColor()
        {
            return backgroundColor;
        }

        public async Task SetBackgroundColor(string color, bool sendToServer, bool addToUndoStack)
        {
            if (addToUndoStack)
            {
                gameService.GameState.BackgroundColorChanged(color);
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
            if (isMouseDown &&
                mousePoint != null &&
                (activeTool == Tool.Brush || activeTool == Tool.Erase))
            {
                isMouseDown = false;
                Point2D currentPoint = new Point2D(e.OffsetX, e.OffsetY);
                string color = activeTool switch
                {
                    Tool.Brush => colors[activeColor.y, activeColor.x],
                    Tool.Erase => "#00000000",
                    _ => "#000000"
                };
                DrawLineEventArgs args = new DrawLineEventArgs(mousePoint, currentPoint, brushSize, color, true);
                Task sendTask = gameService.DrawLine(args);
                gameService.GameState.DrawLine(args);
                mousePoint = null;
                await sendTask;
            }
        }

        public void OnMouseEnter(MouseEventArgs e, int canvasWidth, int canvasHeight)
        {
            if (e.Buttons == 1 && (activeTool == Tool.Brush || activeTool == Tool.Erase))
            {
                double mouseX = e.OffsetX;
                double mouseY = e.OffsetY;
                if (mouseX <= mouseY && mouseX <= (canvasWidth - mouseX) && mouseX <= (canvasHeight - mouseY))
                {
                    mouseX = 0.0;
                }
                else if (mouseY <= (canvasWidth - mouseX) && mouseY <= (canvasHeight - mouseY))
                {
                    mouseY = 0.0;
                }
                else if ((canvasWidth - mouseX) <= (canvasHeight - mouseY))
                {
                    mouseX = canvasWidth;
                }
                else
                {
                    mouseY = canvasHeight;
                }

                mousePoint = new Point2D(mouseX, mouseY);
                isMouseDown = true;
            }
        }

        public async Task OnMouseMove(MouseEventArgs e)
        {
            if (isMouseDown &&
                mousePoint != null &&
                (activeTool == Tool.Brush || activeTool == Tool.Erase))
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
                gameService.GameState.DrawLine(args);
                mousePoint = currentPoint;
                await sendTask;
            }
        }

        public async Task OnMouseUp(MouseEventArgs e)
        {
            if (isMouseDown &&
                mousePoint != null &&
                (activeTool == Tool.Brush || activeTool == Tool.Erase))
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
                gameService.GameState.DrawLine(args);
                mousePoint = null;
                isMouseDown = false;
                await sendTask;
            }
            else if (isMouseDown && activeTool == Tool.Fill)
            {
                Point2D currentPoint = new Point2D(e.OffsetX, e.OffsetY);
                FillEventArgs args = new FillEventArgs(currentPoint, colors[activeColor.y, activeColor.x]);
                Task sendTask = gameService.Fill(args);
                gameService.GameState.Fill(args);
                mousePoint = null;
                isMouseDown = false;
                await sendTask;
            }
            return;
        }

        public async Task Undo()
        {
            if (isActivePlayer)
            {
                Task sendTask = gameService.Undo();
                gameService.GameState.Undo();
                await sendTask;
            }
        }

        public async Task ClearCanvas()
        {
            if (isActivePlayer)
            {
                gameService.GameState.ClearCanvas(backgroundColor);
                Task sendTask = gameService.ClearCanvas(backgroundColor);
                await sendTask;
            }
        }
    }
}
