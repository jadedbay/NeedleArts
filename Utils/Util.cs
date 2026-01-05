namespace NeedleArts.Utils;

public static class Util {
    public static tk2dSpriteAnimationClip CopyClip(tk2dSpriteAnimationClip clip, string newName) {
        return new tk2dSpriteAnimationClip {
            name = newName,
            frames = clip.frames,
            fps = clip.fps,
            loopStart = clip.loopStart,
            wrapMode = clip.wrapMode,
        };
    }
}