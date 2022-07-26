using System;

namespace Draw.Client.Services
{
    internal class KeyboardCommandService : IKeyboardCommandService, IDisposable
    {
        private readonly IGameService gameService;
        private bool isActivePlayer = false;

        public event EventHandler<KeyboardShortcuts>? KeyboardShortCutHit;

        public KeyboardCommandService(IGameService gameService)
        {
            this.gameService = gameService;
            gameService.GameState.ActivePlayerDrawStarted += OnActivePlayerDrawStarted;
            gameService.GameState.PlayerDrawStarted += OnPlayerDrawStarted;
        }

        public void Dispose()
        {
            gameService.GameState.ActivePlayerDrawStarted -= OnActivePlayerDrawStarted;
            gameService.GameState.PlayerDrawStarted += OnPlayerDrawStarted;
        }

        public void KeyPressed(string key, bool ctrlKey)
        {
            if (isActivePlayer)
            { 
                switch (key)
                {
                    case "KeyZ":
                        KeyboardShortCutHit?.Invoke(this, KeyboardShortcuts.Undo);
                        break;
                    case "KeyB":
                        KeyboardShortCutHit?.Invoke(this, KeyboardShortcuts.Brush);
                        break;
                    case "KeyF":
                        KeyboardShortCutHit?.Invoke(this, KeyboardShortcuts.Fill);
                        break;
                    case "KeyE":
                        KeyboardShortCutHit?.Invoke(this, KeyboardShortcuts.Erase);
                        break;
                }

            }
        }

        private void OnActivePlayerDrawStarted(object? sender, EventArgs _)
        {
            isActivePlayer = true;
        }

        private void OnPlayerDrawStarted(object? sender, PlayerDrawEventArgs _)
        {
            isActivePlayer = false;
        }
    }
}
