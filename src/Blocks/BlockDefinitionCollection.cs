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

namespace TetrisSharp.Blocks;

public sealed class BlockDefinitionCollection
{
    #region Public Properties

    public List<BlockDefinition> Definitions { get; set; } = [];

    #endregion Public Properties

    #region Public Methods

    public static BlockDefinitionCollection LoadFromFile(string fileName)
    {
        using var fileStream = File.OpenRead(fileName);
        using var streamReader = new StreamReader(fileStream);
        var result = new BlockDefinitionCollection();
        var curBlockName = string.Empty;
        var rotationDefinitions = new List<string>();
        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith("block", StringComparison.InvariantCultureIgnoreCase))
                {
                    var lineValues = line.Split(' ');
                    if (lineValues.Length < 2)
                    {
                        throw new TetrisSharpException($"Unable to parse block definition file {fileName}.");
                    }

                    curBlockName = lineValues[1].Trim('"');
                }
                else if (string.Equals(line, "end block", StringComparison.InvariantCultureIgnoreCase))
                {
                    var definition = new BlockDefinition
                    {
                        Name = curBlockName
                    };

                    foreach (var rotationDefinition in rotationDefinitions)
                    {
                        definition.Rotations.Add(new BlockRotation { RotationDefinition = rotationDefinition });
                    }

                    result.Definitions.Add(definition);
                    rotationDefinitions.Clear();
                }
                else
                {
                    rotationDefinitions.Add(line.Trim());
                }
            }
        }

        return result;
    }

    #endregion Public Methods
}