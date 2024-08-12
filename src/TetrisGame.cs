using System.IO;
using Mfx.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetrisSharp.Scenes;

namespace TetrisSharp
{
    public class TetrisGame : MfxGame
    {
        public TetrisGame()
            : base(MfxGameWindowOptions.FromDefault("Tetris#", MfxGameWindowOptions.NormalScreenFixedSizeShowMouse,
                new Point(820, 768)))
        {
            AddScene<TitleScene>();
            AddScene<GameScene>();
            AddScene<InputSettingScene>();
            StartFrom<TitleScene>();
        }

        public bool CanContinue { get; set; } = false;

        public TetrisGameSettings? Settings { get; set; }

        public bool CanLoadGame =>
            Settings is not null &&
            !string.IsNullOrEmpty(Settings.LastGameBoardValuesBase64);

        protected override void LoadContent()
        {
            // Load Settings
            if (!File.Exists(TetrisGameSettings.DefaultSettingsFileName))
            {
                Settings = TetrisGameSettings.Default;
                TetrisGameSettings.SaveSettings(TetrisGameSettings.DefaultSettingsFileName, Settings);
            }
            else
            {
                Settings = TetrisGameSettings.LoadSettings(TetrisGameSettings.DefaultSettingsFileName);
            }

            base.LoadContent();
        }
    }
}
