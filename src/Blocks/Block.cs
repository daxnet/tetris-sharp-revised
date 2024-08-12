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
using System.Runtime.CompilerServices;
using Mfx.Core.Scenes;
using Mfx.Core.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetrisSharp.Messages;
using TetrisSharp.Scenes;

namespace TetrisSharp.Blocks;

/// <summary>
///     Represents a Block in the Tetris game.
/// </summary>
internal sealed class Block : Sprite
{
    #region Private Fields

    private readonly BlockDefinition _blockDefinition;
    private readonly int _boardX;
    private readonly int _boardY;
    private readonly IGameScene _gameScene;
    private readonly bool _isActiveBlock;
    private readonly int _tileSize;
    private readonly Texture2D _tileTexture;
    private int _currentRotationIndex;
    private TimeSpan _fallingTimeCounter = TimeSpan.Zero;
    private int _x;
    private int _y;

    #endregion Private Fields

    #region Public Constructors

    /// <summary>
    ///     Initializes a new instance of the <c>Block</c> class.
    /// </summary>
    /// <param name="gameScene">The <see cref="IGameScene" /> to which the block is added.</param>
    /// <param name="tileTexture">The texture to be used for the current block.</param>
    /// <param name="blockDefinition">The <see cref="BlockDefinition" /> which defines the current block.</param>
    /// <param name="boardX">The X coordinate of the game board on the scene.</param>
    /// <param name="boardY">The Y coordinate of the game board on the scene.</param>
    /// <param name="isActiveBlock">A <see cref="bool" /> value which indicates if the block is an active block.</param>
    /// <param name="x">The X position to where the block is initially placed.</param>
    /// <param name="y">The Y position to where the block is initilaly placed.</param>
    /// <returns>The created block.</returns>
    /// <remarks>
    ///     If the block is an active block (isActiveBlock set to <c>true</c>), the <c>x</c> and <c>y</c> parameters represent
    ///     the coordinate on the game board, transformed with the game board coordination system. If the block is not an
    ///     active
    ///     block (isActiveBlock set to <c>false</c>), the <c>x</c> and <c>y</c> parameters represent the absolute coordinate
    ///     on the scene.
    /// </remarks>
    public Block(IGameScene gameScene, Texture2D tileTexture, BlockDefinition blockDefinition, int boardX, int boardY,
        bool isActiveBlock = true, int? x = null, int? y = null)
        : base(gameScene, tileTexture, 0, 0)
    {
        _tileTexture = tileTexture;
        _blockDefinition = blockDefinition;
        _gameScene = gameScene;
        _tileSize = tileTexture.Width;
        _boardX = boardX;
        _boardY = boardY;
        _isActiveBlock = isActiveBlock;

        if (x is null && y is null)
        {
            _x = (Constants.NumberOfTilesX - blockDefinition.Rotations[0].Width) / 2;
            _y = 0;
        }
        else
        {
            if (x is not null)
            {
                _x = x.Value;
            }

            if (y is not null)
            {
                _y = y.Value;
            }
        }

        Layer = int.MaxValue - 1;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>
    ///     Gets the current rotation of the block. If no rotations have been defined
    ///     in the block definition, <see cref="TetrisSharpException" /> will be thrown.
    /// </summary>
    public BlockRotation CurrentRotation => _blockDefinition.Rotations[_currentRotationIndex] ??
                                            throw new TetrisSharpException(
                                                "A block definition should have at least one rotation definition.");

    /// <summary>
    ///     Gets or sets the interval of the falling of the block.
    /// </summary>
    public TimeSpan FallingInterval { get; set; } = TimeSpan.FromMilliseconds(1000);

    public override float X
    {
        get => _x;
        set => _x = (int)value;
    }

    public override float Y
    {
        get => _y;
        set => _y = (int)value;
    }

    #endregion Public Properties

    #region Private Properties

    private bool CanMoveLeft
    {
        get
        {
            if (_x == 0)
            {
                return false;
            }

            for (var y = 0; y < CurrentRotation.Height; y++)
            {
                if (_x > 0 &&
                    CurrentRotation.Matrix is not null &&
                    CurrentRotation.Matrix[0, y] == 1 &&
                    _gameScene.GameBoard?.BoardMatrix[_x - 1, _y + y] == 1)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private bool CanMoveRight
    {
        get
        {
            if (_x == Constants.NumberOfTilesX - CurrentRotation.Width)
            {
                return false;
            }

            for (var y = 0; y < CurrentRotation.Height; y++)
            {
                if (_x < Constants.NumberOfTilesX - 1 &&
                    CurrentRotation.Matrix is not null &&
                    CurrentRotation.Matrix[CurrentRotation.Width - 1, y] == 1 &&
                    _gameScene.GameBoard?.BoardMatrix[_x + CurrentRotation.Width, _y + y] == 1)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private bool CanRotate
    {
        get
        {
            var nextRotation =
                _blockDefinition.Rotations[(_currentRotationIndex + 1) % _blockDefinition.Rotations.Count];
            var rotatedX = _x;
            var rotatedY = _y;
            if (rotatedX + nextRotation.Width > Constants.NumberOfTilesX)
            {
                rotatedX = Constants.NumberOfTilesX - nextRotation.Width;
            }

            if (rotatedY + nextRotation.Height > Constants.NumberOfTilesY)
            {
                rotatedY = Constants.NumberOfTilesY - nextRotation.Height;
            }

            for (var y = 0; y < nextRotation.Height; y++)
            {
                for (var x = 0; x < nextRotation.Width; x++)
                {
                    if (nextRotation.Matrix is not null &&
                        nextRotation.Matrix[x, y] == 1 &&
                        _gameScene.GameBoard?.BoardMatrix[rotatedX + x, rotatedY + y] == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    #endregion Private Properties

    #region Public Methods

    /// <summary>
    ///     Drops the block to the game board.
    /// </summary>
    public void Drop()
    {
        if (!_isActiveBlock)
        {
            return;
        }

        // If the current block doesn't overlap with the game
        // board, then continuously adding its Y position until
        // the block overlaps with the game board.
        while (!IsOverlapped())
        {
            Y++;
        }

        // Then publishes the message indicating that the block
        // has overlapped with the game board.
        Publish(new BlockOverlappedMessage(this, false));
    }

    public void MoveDown()
    {
        if (!_isActiveBlock)
        {
            return;
        }

        if (IsOverlapped())
        {
            Publish(new BlockOverlappedMessage(this, false));
        }

        Y++;
    }

    public void MoveLeft()
    {
        if (_isActiveBlock && CanMoveLeft)
        {
            X--;
        }
    }

    public void MoveRight()
    {
        if (_isActiveBlock && CanMoveRight)
        {
            X++;
        }
    }

    public override void OnAddedToScene(IScene scene)
    {
        // If when adding the block to the scene, it collides with the board, this means that the game is over.
        if (IsOverlapped())
        {
            Publish(new BlockOverlappedMessage(this, true));
        }
    }

    public void Rotate()
    {
        if (!_isActiveBlock)
        {
            return;
        }

        if (!CanRotate)
        {
            return;
        }

        _currentRotationIndex = ++_currentRotationIndex % _blockDefinition.Rotations.Count;

        if (_x + CurrentRotation.Width > Constants.NumberOfTilesX)
        {
            _x = Constants.NumberOfTilesX - CurrentRotation.Width;
        }

        if (_y + CurrentRotation.Height > Constants.NumberOfTilesY)
        {
            _y = Constants.NumberOfTilesY - CurrentRotation.Height;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!_isActiveBlock)
        {
            return;
        }

        base.Update(gameTime);
        _fallingTimeCounter += gameTime.ElapsedGameTime;
        if (_fallingTimeCounter >= FallingInterval)
        {
            if (IsOverlapped())
            {
                Publish(new BlockOverlappedMessage(this, false));
            }

            Y++;
            _fallingTimeCounter = TimeSpan.Zero;
        }
    }

    #endregion Public Methods

    #region Protected Methods

    protected override void ExecuteDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var rotation = CurrentRotation;
        if (rotation.Matrix is null)
        {
            return;
        }

        //spriteBatch.Begin();
        for (var tileY = 0; tileY < rotation.Height; tileY++)
        {
            for (var tileX = 0; tileX < rotation.Width; tileX++)
            {
                if (rotation.Matrix[tileX, tileY] != 1)
                {
                    continue;
                }

                int posX, posY;
                if (_isActiveBlock)
                {
                    posX = (_x + tileX) * _tileSize + _boardX;
                    posY = (_y + tileY) * _tileSize + _boardY;
                }
                else
                {
                    posX = _x + tileX * _tileSize;
                    posY = _y + tileY * _tileSize;
                }

                spriteBatch.Draw(_tileTexture, new Vector2(posX, posY), Color.White);

                if (!_isActiveBlock)
                {
                    // If the current block is not an active block, or the block doesn't
                    // require to show the navigator, then skip rendering the navigator.
                    continue;
                }

                var navYPos = (GetNavigatorYPos() + tileY) * _tileSize + _boardY;
                spriteBatch.Draw(_tileTexture, new Vector2(posX, navYPos), Color.White * 0.15f);
            }
        }

        //spriteBatch.End();
    }

    #endregion Protected Methods

    #region Private Methods

    private int GetNavigatorYPos()
    {
        if (_blockDefinition.Rotations is null)
        {
            throw new TetrisSharpException(
                "A block definition should have at least one rotation definition.");
        }

        var minY = int.MaxValue;
        foreach (var point in CurrentRotation.BottomEdge)
        {
            for (var y = _y; y < Constants.NumberOfTilesY; y++)
            {
                if (IsColliding(point.X + _x, point.Y + y))
                {
                    minY = minY < y ? minY : y;
                }
            }
        }

        return minY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsColliding(int x, int y) =>
        y + 1 >= Constants.NumberOfTilesY ||
        _gameScene.GameBoard?.BoardMatrix[x, y + 1] == 1;

    private bool IsOverlapped()
    {
        if (!_isActiveBlock)
        {
            return false;
        }

        if (_blockDefinition.Rotations is null)
        {
            throw new TetrisSharpException(
                "A block definition should have at least one rotation definition.");
        }

        foreach (var point in CurrentRotation.BottomEdge)
        {
            if (IsColliding(point.X + _x, point.Y + _y))
            {
                return true;
            }
        }

        return false;
    }

    #endregion Private Methods
}