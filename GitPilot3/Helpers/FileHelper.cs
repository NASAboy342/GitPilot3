using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GitPilot3.Helpers;

public class FileHelper
{
    internal static IImage GetBitMap(string assetPath)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"avares://Gitpilot3/Assets/{assetPath}")));
    }

    internal static List<string> ReadFromFile(string path)
    {
        var lines = new List<string>();
        try
        {
            using (var reader = new System.IO.StreamReader(path))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }    
        }
        catch(Exception ex)
        {
            throw;
        }
        
        return lines;
    }
}
