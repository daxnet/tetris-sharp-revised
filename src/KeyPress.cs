using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisSharp
{
    internal class KeyPress
    {
        private static KeyboardState _state;

        private readonly Keys _key;
        private bool _isHeld;

        public KeyPress(Keys key)
        {
            _key = key;
            _isHeld = false;
        }

        public bool IsPressed
        {
            get
            {

                if (_state.IsKeyDown(_key))
                {
                    if (_isHeld) return false;
                    _isHeld = true;
                    return true;
                }

                if (_isHeld) _isHeld = false;
                return false;
            }
        }

        public static void Update()
        {
            _state = Keyboard.GetState();
        }
    }
}
