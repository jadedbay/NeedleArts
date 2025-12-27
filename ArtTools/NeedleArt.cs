using UnityEngine;

namespace NeedleArts.ArtTools;

public abstract class NeedleArt(string name, string animName) {
    public string Name { get; } = name;
    public ToolItem ToolItem { get; set; }
    
    public string AnimName { get; } = animName;
    
    public abstract GameObject GetChargeSlash();
    public abstract HeroControllerConfig GetConfig();
    public abstract void EditFsm(PlayMakerFSM fsm);
    public abstract void EditToolItem();
}