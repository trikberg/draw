using System;

namespace Draw.Client.Services
{
    internal interface IKeyboardCommandService
    {
        public event EventHandler<KeyboardShortcuts> KeyboardShortCutHit;
        public void KeyPressed(string key, bool ctrlKey);
    }
}
