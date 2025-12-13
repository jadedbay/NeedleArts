using System.IO;
using System.Reflection;
using UnityEngine;

namespace NeedleArts;

public static class Util {
    public static Texture2D LoadTextureFromAssembly(string resourceName) {
        var asm = Assembly.GetExecutingAssembly();

        using Stream? stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null) {
            NeedleArtsPlugin.Log.LogError($"Resource not found: {resourceName}");
            return null;
        }

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        byte[] data = ms.ToArray();

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return tex;
    }
}