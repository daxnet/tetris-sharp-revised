using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TetrisSharp
{
    public sealed class TetrisGameSettings
    {
        public const string DefaultSettingsFileName = "tetrissharp.json";

        public static readonly TetrisGameSettings Default = new()
        {
            KeySettings = new()
            {
                { "Down", "key.S" },
                { "Left", "key.A" },
                { "Right", "key.D" },
                { "Rotate", "key.J" },
                { "Drop", "key.K" },
                { "Pause", "key.Space" }
            },
            HighestScore = 0
        };

        public int LastScore { get; set; }
        
        public int LastLevel { get; set; }

        public int LastLines { get; set; }

        public int LastBlocks { get; set; }

        public string? LastGameBoardValuesBase64 { get; set; }

        public int HighestScore { get; set; }

        public Dictionary<string, string>? KeySettings { get; set; }

        public static void SaveSettings(string fileName, TetrisGameSettings settings)
        {
            using var ms = new MemoryStream();
            JsonSerializer.Serialize(ms, settings);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            File.WriteAllText(fileName, json);
        }

        public static TetrisGameSettings LoadSettings(string fileName)
        {
            using var fileStream = File.OpenRead(fileName);
            return JsonSerializer.Deserialize<TetrisGameSettings>(fileStream) ??
                   throw new TetrisSharpException("Unable to load settings from file.");
        }

    }
}
