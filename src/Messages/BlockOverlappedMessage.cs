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

using Mfx.Core.Messaging;
using TetrisSharp.Blocks;

namespace TetrisSharp.Messages;

/// <summary>
///     Represents the message which notifies that the block has been overlapped.
/// </summary>
/// <param name="block">The block that triggered the message.</param>
/// <param name="checkmate">A <see cref="bool" /> value which indicates if the game is a checkmate.</param>
internal sealed class BlockOverlappedMessage(Block block, bool checkmate) : Message
{
    #region Public Properties

    /// <summary>
    ///     Gets the instance of the <see cref="Block" /> that triggered the message.
    /// </summary>
    public Block Block { get; } = block;

    /// <summary>
    ///     Gets a <see cref="bool" /> value which indicates if the game is a checkmate.
    /// </summary>
    public bool Checkmate { get; } = checkmate;

    #endregion Public Properties
}