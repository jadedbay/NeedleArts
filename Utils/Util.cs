using System.IO;
using System.Linq;
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

    public static tk2dSpriteAnimationClip CopyClip(tk2dSpriteAnimationClip clip, string newName) {
        return new tk2dSpriteAnimationClip {
            name = newName,
            frames = clip.frames,
            fps = clip.fps,
            loopStart = clip.loopStart,
            wrapMode = clip.wrapMode
        };
    }
}