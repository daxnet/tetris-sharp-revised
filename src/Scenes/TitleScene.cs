// =============================================================================
//               __
//              / _|
//    _ __ ___ | |___  __
//   | '_ ` _ \|  _\ \/ /
//   | | | | | | |  >  <
//   |_| |_| |_|_| /_/\_\
//
// MIT License
//
// Copyright (c) 2024 Sunny Chen (daxnet)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// =============================================================================

using System.IO;
using FontStashSharp;
using Mfx.Core.Elements;
using Mfx.Core.Elements.Menus;
using Mfx.Core.Scenes;
using Mfx.Core.Sounds;
using Mfx.Extended.FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TetrisSharp.Scenes;

internal sealed class TitleScene(TetrisGame game, string name) : Scene(game, name, Color.Black)
{
    #region Private Fields

    private readonly FontSystem _fontSystem = new();

    private BackgroundMusic? _bgm;
    private Song? _bgmSong;
    private bool _disposed;
    private Menu? _menu;
    private DynamicSpriteFont? _menuFont;

    #endregion Private Fields

    #region Public Methods

    public override void Enter(object? args = null)
    {
        var continueMenuItem = _menu?.GetMenuItem("mnuContinue");
        if (continueMenuItem is not null)
        {
            continueMenuItem.Enabled = GameAs<TetrisGame>().CanContinue;
        }

        var loadGameMenuItem = _menu?.GetMenuItem("mnuLoad");
        if (loadGameMenuItem is not null)
        {
            loadGameMenuItem.Enabled = GameAs<TetrisGame>().CanLoadGame;
        }

        _bgm?.Play();
    }

    public override void Leave(bool closing = false)
    {
        Mouse.SetCursor(MouseCursor.Arrow);
        _bgm?.Stop();
    }

    public override void Load(ContentManager contentManager)
    {
        // Fonts
        _fontSystem.AddFont(File.ReadAllBytes(@"res\main.ttf"));
        _menuFont = _fontSystem.GetFont(30);

        // Background music
        _bgmSong = contentManager.Load<Song>(@"sounds\opening");
        _bgm = new BackgroundMusic([_bgmSong], .2f);

        // Background images
        var backgroundImageTexture = contentManager.Load<Texture2D>("images\\title");
        Add(new Image(this, backgroundImageTexture));

        _menu = new Menu(this, new FontStashSharpAdapter(_menuFont), [
                new MenuItem("mnuNewGame", "New Game"),
                new MenuItem("mnuContinue", "Continue") { Enabled = false },
                new MenuItem("mnuLoad", "Load") { Enabled = false },
                new MenuItem("mnuInputOptions", "Input Settings"),
                new MenuItem("mnuExit", "Exit")
            ], 510, 230, Color.FromNonPremultiplied(0, 130, 190, 255), Color.Brown, Color.Gray,
            alignment: Menu.Alignment.Right)
        {
            Layer = int.MaxValue // Put the menu on top
        };

        Add(_menu);
        Add(_bgm);

        SubscribeMessages();
    }

    #endregion Public Methods

    #region Protected Methods

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _bgm?.Stop();
                _bgmSong?.Dispose();
                _fontSystem.Dispose();
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private void SubscribeMessages()
    {
        Subscribe<MenuItemClickedMessage>((_, message) =>
        {
            switch (message.MenuItemName)
            {
                case "mnuNewGame":
                    Game.Transit<GameScene>(Constants.NewGameFlag);
                    break;

                case "mnuContinue":
                    Game.Transit<GameScene>(Constants.ContinueGameFlag);
                    break;

                case "mnuLoad":
                    Game.Transit<GameScene>(Constants.LoadGameFlag);
                    break;

                case "mnuInputOptions":
                    Game.Transit<InputSettingScene>();
                    break;

                case "mnuExit":
                    Game.Exit();
                    break;
            }
        });
    }

    #endregion Private Methods
}