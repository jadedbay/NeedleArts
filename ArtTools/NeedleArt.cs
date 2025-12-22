using UnityEngine;

namespace NeedleArts.ArtTools;

public abstract class NeedleArt(string name) {
    public string Name { get; } = name;
    public ToolItem ToolItem { get; set; }

    public abstract GameObject GetChargeSlash();
    public abstract HeroControllerConfig GetConfig();
    public abstract void EditFsm(PlayMakerFSM fsm);
}