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

using System.Collections.Generic;

namespace TetrisSharp.Blocks;

/// <summary>
///     Represents a block definition.
/// </summary>
public sealed class BlockDefinition
{
    #region Public Properties

    /// <summary>
    ///     Gets or sets the description of the block definition.
    /// </summary>
    /// <remarks>The description can be empty.</remarks>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the name of the block definition.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets a list of <see cref="BlockRotation" />s which
    ///     represents the available rotation variants of the current block.
    /// </summary>
    public List<BlockRotation> Rotations { get; set; } = [];

    #endregion Public Properties

    #region Public Methods

    /// <inheritdoc />
    public override string? ToString() => Name;

    #endregion Public Methods
}