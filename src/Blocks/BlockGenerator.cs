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

using Microsoft.Xna.Framework.Graphics;
using TetrisSharp.Scenes;

namespace TetrisSharp.Blocks;

/// <summary>
///     Represents the block generator which generates a tetris block.
/// </summary>
/// <param name="fileName">
///     The name of the file from which the tetris block
///     definition is loaded.
/// </param>
internal sealed class BlockGenerator(string fileName)
{
    #region Private Fields

    private readonly BlockDefinitionCollection _blockDefinitions = BlockDefinitionCollection.LoadFromFile(fileName);

    #endregion Private Fields

    #region Public Properties

    public int BlockDefinitionCount => _blockDefinitions.Definitions.Count;

    #endregion Public Properties

    #region Public Methods

    /// <summary>
    ///     Creates a <see cref="Block" /> instance based on the index of the definition.
    /// </summary>
    /// <param name="scene">The <see cref="IGameScene" /> to which the block is added.</param>
    /// <param name="tileTextures">A list of <see cref="Texture2D" /> objects that represents the textures of the block.</param>
    /// <param name="index">The index of the block definition in the definition list.</param>
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
    public Block Create(IGameScene scene, Texture2D[] tileTextures, int index, int boardX, int boardY,
        bool isActiveBlock = true, int? x = null,
        int? y = null) =>
        new(scene, tileTextures[index % Constants.TileTextureCount], _blockDefinitions.Definitions[index], boardX,
            boardY, isActiveBlock, x, y)
        {
            // Disable the collision detection on the block object.
            Collidable = false,
            Layer = int.MaxValue
        };

    #endregion Public Methods
}