using System.IO;
using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D SaveAsPng(this Texture2D tex, string folderPath)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes($"{folderPath}/{tex.name}_tex.png", bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + folderPath);
        return tex;
    }

    public static Texture2D SaveAsJpg(this Texture2D tex, string folderPath)
    {
        byte[] bytes = tex.EncodeToJPG();
        File.WriteAllBytes($"{folderPath}/{tex.name}_tex.jpg", bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + folderPath);
        return tex;
    }
}