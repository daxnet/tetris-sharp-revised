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
using System.Text;
using Microsoft.Xna.Framework;

namespace TetrisSharp.Blocks;

/// <summary>
///     Represents a rotation definition of a block.
/// </summary>
public sealed class BlockRotation
{
    #region Private Fields

    private string? _rotationDefinitions;

    #endregion Private Fields

    #region Public Properties

    public IEnumerable<Point> BottomEdge
    {
        get
        {
            var result = new List<Point>();
            for (var tileX = 0; tileX < Width; tileX++)
            {
                for (var tileY = Height - 1; tileY >= 0; tileY--)
                {
                    if (Matrix is null || Matrix[tileX, tileY] != 1)
                    {
                        continue;
                    }

                    result.Add(new Point(tileX, tileY));
                    break;
                }
            }

            return result;
        }
    }

    public int Height { get; private set; }

    public byte[,]? Matrix { get; private set; }

    public string? RotationDefinition
    {
        get => _rotationDefinitions;
        set
        {
            _rotationDefinitions = value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var splitted = value.Split(' ');
            if (splitted.Length <= 0)
            {
                return;
            }

            Width = splitted[0].Length;
            Height = splitted.Length;
            Matrix = new byte[Width, Height];
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    Matrix[x, y] = Convert.ToByte(splitted[y][x].ToString());
                }
            }
        }
    }

    public int Width { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public override string? ToString()
    {
        if (Matrix == null || Width == 0 || Height == 0)
        {
            return base.ToString();
        }

        var sb = new StringBuilder();
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                sb.Append(Matrix[x, y]);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    #endregion Public Methods
}