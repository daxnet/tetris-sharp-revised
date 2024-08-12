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

using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using Mfx.Core.Elements;
using Mfx.Core.Input;
using Mfx.Core.Scenes;
using Mfx.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TetrisSharp.Blocks;
using TetrisSharp.Messages;

namespace TetrisSharp.Scenes;

internal sealed class GameScene(TetrisGame game, string name)
    : Scene(game, name, Color.FromNonPremultiplied(0, 130, 190, 255)), IGameScene
{

    #region Private Fields

    private const float KeyDelay = 0.1f;
    private static readonly Random _rnd = new(DateTime.Now.Millisecond);

    private readonly BlockGenerator _blockGenerator = new("blocks.txt");
    private readonly FontSystem _mainFontSystem = new();
    private readonly Queue<int> _tetrisQueue = new();
    private readonly Texture2D[] _tileTextures = new Texture2D[Constants.TileTextureCount];
    private BackgroundMusic? _bgm;
    private Song? _bgmSong;
    private Block? _block;
    private int _blocks;
    private int _boardX;
    private int _boardY;
    private Label? _copyrightLabel;
    private bool _disposed;
    private Texture2D? _fixedTileTexture;
    private Texture2D? _gameboardTexture;
    private bool _gameOver;
    private DynamicSpriteFont? _gameOverFont;
    private Sound? _gameOverSound;
    private SoundEffect? _gameOverSoundEffect;
    private bool _gamePaused;
    private int _level;
    private int _lines;
    private Sound? _mergeSound;
    private SoundEffect? _mergeSoundEffect;
    private Block? _nextBlock;
    private DynamicSpriteFont? _pauseTextFont;
    private Sound? _removeRowSound;
    private SoundEffect? _removeRowSoundEffect;
    private int _score;
    private Rectangle _scoreBoardBoundingBox;
    private DynamicSpriteFont? _scoreBoardFont;
    private TetrisGameSettings? _settings;
    private float _timeSinceLastKeyPress;

    #endregion Private Fields

    #region Public Properties

    public GameBoard? GameBoard { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawScoreBoard(spriteBatch);

        base.Draw(gameTime, spriteBatch);

        if (_gameOver)
        {
            spriteBatch.DrawString(_gameOverFont, Constants.GameOverText, new Vector2(_boardX + 12, _boardY + 150),
                Color.OrangeRed);
        }

        if (_gamePaused)
        {
            spriteBatch.DrawString(_pauseTextFont, Constants.PauseText, new Vector2(_boardX + 75, _boardY + 150),
                Color.RosyBrown);
        }
    }

    public override void Enter(object? args = null)
    {
        GameAs<TetrisGame>().CanContinue = true;
        _settings = GameAs<TetrisGame>().Settings;

        if (args is not string sArgs)
        {
            return;
        }

        switch (sArgs)
        {
            case Constants.NewGameFlag:
                InitializeGame();
                _bgm?.Play();
                break;

            case Constants.ContinueGameFlag:
                if (!_gamePaused)
                {
                    _bgm?.Resume();
                }
                break;

            case Constants.LoadGameFlag:
                InitializeGame(_settings);
                _bgm?.Play();
                break;
        }
    }

    public override void Leave(bool closing = false)
    {
        _bgm?.Pause();
        SaveSettings();
        GameAs<TetrisGame>().CanContinue = !_gameOver;
    }

    public override void Load(ContentManager contentManager)
    {
        // Board and coordinates
        _boardX = 30;
        _boardY = (Viewport.Height - 25 * Constants.NumberOfTilesY) / 2;
        _scoreBoardBoundingBox = new Rectangle(_boardX + Constants.NumberOfTilesX * 25 + 30, _boardY,
            Viewport.Width - Constants.NumberOfTilesX * 25 - 30 - 2 * _boardX, Viewport.Height - 2 * _boardY);

        // Sound & music
        _gameOverSoundEffect = contentManager.Load<SoundEffect>(@"sounds\gameover");
        _gameOverSound = new Sound(_gameOverSoundEffect, Constants.SoundVolume);
        _mergeSoundEffect = contentManager.Load<SoundEffect>(@"sounds\merge");
        _mergeSound = new Sound(_mergeSoundEffect, Constants.SoundVolume);
        _removeRowSoundEffect = contentManager.Load<SoundEffect>(@"sounds\remove_row");
        _removeRowSound = new Sound(_removeRowSoundEffect, Constants.SoundVolume);
        _bgmSong = contentManager.Load<Song>(@"sounds\bgm");
        _bgm = new BackgroundMusic([_bgmSong], Constants.BgmVolume);

        // Fonts & static texts
        _mainFontSystem.AddFont(File.ReadAllBytes(@"res\main.ttf"));
        _scoreBoardFont = _mainFontSystem.GetFont(38);
        _gameOverFont = _mainFontSystem.GetFont(70);
        _pauseTextFont = _mainFontSystem.GetFont(70);
        var arialFont = contentManager.Load<SpriteFont>(@"fonts\arial");
        var copyrightTextSize = arialFont.MeasureString(Constants.CopyrightText);
        var copyrightTextX = _scoreBoardBoundingBox.X + _scoreBoardBoundingBox.Width - copyrightTextSize.X;
        var copyrightTextY = _scoreBoardBoundingBox.Bottom - copyrightTextSize.Y;
        _copyrightLabel = new Label(Constants.CopyrightText, this, arialFont, copyrightTextX, copyrightTextY,
            Color.White);

        // Load block tile textures
        for (var i = 1; i <= Constants.TileTextureCount; i++)
        {
            _tileTextures[i - 1] = contentManager.Load<Texture2D>($@"tiles\{i}");
        }

        _fixedTileTexture = contentManager.Load<Texture2D>(@"tiles\tile_fixed");

        // Set up the game board
        _gameboardTexture = CreateGameBoardTexture();
        GameBoard = new GameBoard(this, _gameboardTexture, _fixedTileTexture, Constants.NumberOfTilesX,
            Constants.NumberOfTilesY, _boardX, _boardY);

        // Prepare the tetris queue
        _tetrisQueue.Enqueue(_rnd.Next(_blockGenerator.BlockDefinitionCount));
        _tetrisQueue.Enqueue(_rnd.Next(_blockGenerator.BlockDefinitionCount));

        Subscribe<BlockOverlappedMessage>(BlockOverlappedHandler);

        Add(GameBoard);
        Add(_bgm);
        Add(_copyrightLabel);
    }

    public override void Update(GameTime gameTime)
    {
        var keyState = Keyboard.GetState();
        if (keyState.HasPressedOnce(Keys.Escape))
        {
            Game.Transit<TitleScene>();
        }

        if (_settings?.KeySettings is not null &&
            VirtualInput.HasPressedOnce(_settings.KeySettings[Constants.VkPause]) &&
            !_gameOver)
        {
            _gamePaused = !_gamePaused;
            ToggleBackgroundMusic(!_gamePaused);
        }

        // If the game is paused, return.
        if (_gamePaused)
        {
            return;
        }

        // If game has ended, suppress the response
        // to all key board or joystick events and simply return.
        if (_gameOver)
        {
            return;
        }

        var seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timeSinceLastKeyPress += seconds;
        if (_timeSinceLastKeyPress > KeyDelay && _settings?.KeySettings is not null)
        {
            if (VirtualInput.IsVirtualKeyPressed(_settings.KeySettings[Constants.VkLeft]))
            {
                _block?.MoveLeft();
            }
            if (VirtualInput.IsVirtualKeyPressed(_settings.KeySettings[Constants.VkRight]))
            {
                _block?.MoveRight();
            }
            if (VirtualInput.IsVirtualKeyPressed(_settings.KeySettings[Constants.VkDown]))
            {
                _block?.MoveDown();
            }
            if (VirtualInput.HasPressedOnce(_settings.KeySettings[Constants.VkRotate]))
            {
                _block?.Rotate();
            }
            if (VirtualInput.HasPressedOnce(_settings.KeySettings[Constants.VkDrop]))
            {
                _block?.Drop();
            }

            _timeSinceLastKeyPress = 0;
        }

        base.Update(gameTime);
    }

    #endregion Public Methods

    #region Protected Methods

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _gameboardTexture?.Dispose();
                _fixedTileTexture?.Dispose();
                foreach (var t in _tileTextures)
                {
                    t.Dispose();
                }

                _gameOverSound?.Stop();
                _gameOverSoundEffect?.Dispose();
                _mergeSound?.Stop();
                _mergeSoundEffect?.Dispose();
                _removeRowSound?.Stop();
                _removeRowSoundEffect?.Dispose();
                _bgm?.Stop();
                _bgmSong?.Dispose();
                _mainFontSystem.Dispose();
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private void AddBlockToBoard()
    {
        if (_nextBlock is not null)
        {
            Remove(_nextBlock);
        }

        // Dequeue the index of the current block, create the block based
        // on the index and add it to the game board.
        var index = _tetrisQueue.Dequeue();
        _block = _blockGenerator.Create(this, _tileTextures, index, _boardX, _boardY);
        _block.FallingInterval = TimeSpan.FromMilliseconds(Math.Max(1000 - (_level - 1) * 50, 1));

        Add(_block);
        _blocks++;

        // In the meanwhile, prepare for the next block.
        var next = _tetrisQueue.Peek();
        _nextBlock = _blockGenerator.Create(this, _tileTextures, next, _boardX, _boardY, false,
            _scoreBoardBoundingBox.X, 435);

        Add(_nextBlock);

        // Enqueue the index of the next block.
        _tetrisQueue.Enqueue(_rnd.Next(_blockGenerator.BlockDefinitionCount));
    }

    private void BlockOverlappedHandler(object sender, BlockOverlappedMessage message)
    {
        if (message.Checkmate)
        {
            _bgm?.Stop();
            _gameOverSound?.Play();
            _gameOver = true;
        }
        else
        {
            var block = message.Block;

            // Merge the block with the game board.
            GameBoard?.Merge(block.CurrentRotation, (int)block.X, (int)block.Y, () => _mergeSound?.Play());
            _score++;

            var rows = GameBoard?.CleanupFilledRows(_ => _removeRowSound?.Play());
            _score += (rows ?? 0) * 50;
            _lines += rows ?? 0;

            // Recalculate level
            if (_score > _level * Constants.ScoreForLevelUp)
            {
                _level++;
            }

            // Setting high score
            if (_settings is not null)
            {
                _settings.HighestScore = _settings.HighestScore > _score ? _settings.HighestScore : _score;
            }

            // Remove the current block sprite from the scene.
            Remove(block);

            // And add a new block to the game board.
            AddBlockToBoard();
        }
    }

    private Texture2D CreateGameBoardTexture()
    {
        var gameboardTexture = new Texture2D(Game.GraphicsDevice, 25 * Constants.NumberOfTilesX,
            25 * Constants.NumberOfTilesY);
        var gameboardColorData = new Color[25 * Constants.NumberOfTilesX * 25 * Constants.NumberOfTilesY];
        for (var i = 0; i < gameboardColorData.Length; i++)
        {
            gameboardColorData[i] = Color.FromNonPremultiplied(0, 0, 0, 50);
        }

        gameboardTexture.SetData(gameboardColorData);
        return gameboardTexture;
    }

    private void DrawScoreBoard(SpriteBatch spriteBatch)
    {
        // Score
        spriteBatch.DrawString(_scoreBoardFont, "Score", new Vector2(_scoreBoardBoundingBox.X, _boardY), Color.White);
        spriteBatch.DrawString(_scoreBoardFont, _score.ToString().PadLeft(9, '0'),
            new Vector2(_scoreBoardBoundingBox.X, _boardY + 30), Color.YellowGreen * 0.9f);

        // Level
        spriteBatch.DrawString(_scoreBoardFont, "Level", new Vector2(_scoreBoardBoundingBox.X, _boardY + 90),
            Color.White);
        spriteBatch.DrawString(_scoreBoardFont, _level.ToString().PadLeft(2, '0'),
            new Vector2(_scoreBoardBoundingBox.X, _boardY + 120), Color.YellowGreen * 0.9f);

        // Blocks
        spriteBatch.DrawString(_scoreBoardFont, "Blocks", new Vector2(_scoreBoardBoundingBox.X, _boardY + 180),
            Color.White);
        spriteBatch.DrawString(_scoreBoardFont, _blocks.ToString().PadLeft(5, '0'),
            new Vector2(_scoreBoardBoundingBox.X, _boardY + 210), Color.YellowGreen * 0.9f);

        // Lines
        spriteBatch.DrawString(_scoreBoardFont, "Lines", new Vector2(_scoreBoardBoundingBox.X, _boardY + 270),
            Color.White);
        spriteBatch.DrawString(_scoreBoardFont, _lines.ToString().PadLeft(5, '0'),
            new Vector2(_scoreBoardBoundingBox.X, _boardY + 300), Color.YellowGreen * 0.9f);

        // Next
        spriteBatch.DrawString(_scoreBoardFont, "Next", new Vector2(_scoreBoardBoundingBox.X, _boardY + 360),
            Color.White);

        // High Score
        spriteBatch.DrawString(_scoreBoardFont, "Hi-Score", new Vector2(_scoreBoardBoundingBox.X, _boardY + 480),
            Color.White);
        spriteBatch.DrawString(_scoreBoardFont, _settings?.HighestScore.ToString().PadLeft(9, '0'),
            new Vector2(_scoreBoardBoundingBox.X, _boardY + 510), Color.YellowGreen * 0.9f);
    }

    private void InitializeGame(TetrisGameSettings? settings = null)
    {
        if (settings is null)
        {
            _score = 0;
            _level = 1;
            _lines = 0;
            _blocks = 0;
            GameBoard?.Reset();
        }
        else
        {
            _score = settings.LastScore;
            _level = settings.LastLevel;
            _lines = settings.LastLines;

            // Minus 1 because later there will be another
            // block being added to the board, which will
            // add this 1 back.
            _blocks = settings.LastBlocks - 1;

            if (!string.IsNullOrEmpty(settings.LastGameBoardValuesBase64))
            {
                GameBoard?.Deserialize(settings.LastGameBoardValuesBase64);
            }
            else
            {
                GameBoard?.Reset();
            }
        }

        _gameOver = false;
        _gamePaused = false;
        if (_block is not null)
        {
            Remove(_block);
        }

        if (_nextBlock is not null)
        {
            Remove(_nextBlock);
        }

        AddBlockToBoard();
    }

    private void SaveSettings()
    {
        if (_settings is not null)
        {
            _settings.LastBlocks = _blocks;
            _settings.LastGameBoardValuesBase64 = GameBoard?.Serialize();
            _settings.LastLevel = _level;
            _settings.LastLines = _lines;
            _settings.LastScore = _score;
            TetrisGameSettings.SaveSettings(TetrisGameSettings.DefaultSettingsFileName, _settings);
        }
    }
    private void ToggleBackgroundMusic(bool play)
    {
        if (play && _bgm?.State == MediaState.Playing)
        {
            return;
        }

        if (play && _bgm?.State != MediaState.Playing)
        {
            _bgm?.Resume();
        }
        else if (_bgm?.State != MediaState.Paused)
        {
            _bgm?.Pause();
        }
    }

    #endregion Private Methods

}