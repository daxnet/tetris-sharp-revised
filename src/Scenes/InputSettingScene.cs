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
using Mfx.Core.Elements.InputConfiguration;
using Mfx.Core.Elements.Menus;
using Mfx.Core.Scenes;
using Mfx.Extended.FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TetrisSharp.Scenes;

internal sealed class InputSettingScene(TetrisGame game, string name)
    : Scene(game, name, Color.FromNonPremultiplied(0, 130, 190, 255))
{
    #region Private Fields

    private readonly FontSystem _fontSystem = new();
    private InputConfigPanel? _inputConfigPanel;
    private DynamicSpriteFont? _inputConfigPanelFont;
    private Menu? _menu;
    private DynamicSpriteFont? _menuFont;
    private DynamicSpriteFont? _titleFont;
    private Label? _titleLabel;

    #endregion Private Fields

    #region Public Methods

    public override void Enter(object? args = null)
    {
        var settings = GameAs<TetrisGame>().Settings;
        if (settings?.KeySettings is not null && _inputConfigPanel?.KeyMappings is not null)
        {
            _inputConfigPanel.KeyMappings = settings.KeySettings;
            _inputConfigPanel.Reset();
        }
    }

    public override void Load(ContentManager contentManager)
    {
        _fontSystem.AddFont(File.ReadAllBytes(@"res\main.ttf"));
        _titleFont = _fontSystem.GetFont(56);
        _inputConfigPanelFont = _fontSystem.GetFont(36);
        _menuFont = _fontSystem.GetFont(30);

        _titleLabel = new Label("Input Settings", this, new FontStashSharpAdapter(_titleFont),
            new Label.RenderingOptions(Color.White, false, true), 0, 10);

        _inputConfigPanel = new InputConfigPanel(this, new FontStashSharpAdapter(_inputConfigPanelFont),
            [
                Constants.VkDown, Constants.VkLeft,
                Constants.VkRight, Constants.VkRotate,
                Constants.VkDrop, Constants.VkPause
            ],
            180, 100, 450, Color.White, Color.Brown);

        _menu = new Menu(this, new FontStashSharpAdapter(_menuFont), [
            new MenuItem("mnuReset", "Reset"),
            new MenuItem("mnuSave", "Save"),
            new MenuItem("mnuCancel", "Cancel")
        ], 340, 350, Color.Yellow, Color.YellowGreen, Color.Gray, 5);

        Add(_titleLabel);
        Add(_inputConfigPanel);
        Add(_menu);

        Subscribe<MenuItemClickedMessage>(HandleMenuItemClick);
    }

    #endregion Public Methods

    #region Private Methods

    private void HandleMenuItemClick(object sender, MenuItemClickedMessage message)
    {
        if (sender.Equals(_menu))
        {
            switch (message.MenuItemName)
            {
                case "mnuReset":
                    _inputConfigPanel?.Reset();
                    break;

                case "mnuSave":
                    SaveSettings();
                    Game.Transit<TitleScene>();
                    break;

                case "mnuCancel":
                    Game.Transit<TitleScene>();
                    break;
            }
        }
    }

    private void SaveSettings()
    {
        var settings = GameAs<TetrisGame>().Settings;
        if (settings?.KeySettings is not null && _inputConfigPanel?.KeyMappings is not null)
        {
            foreach (var kvp in _inputConfigPanel.KeyMappings)
            {
                settings.KeySettings[kvp.Key] = kvp.Value;
            }

            TetrisGameSettings.SaveSettings(TetrisGameSettings.DefaultSettingsFileName, settings);
        }
    }

    #endregion Private Methods
}