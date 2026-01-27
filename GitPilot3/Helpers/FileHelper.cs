using System;
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
}
