using System;
using UnityEngine;

namespace NeedleArts.ArtTools;

public abstract class NeedleArt(string name) {
    public string Name { get; } = name;
    public ToolItem ToolItem { get; set; }

    public Func<string, bool> DataUnlockTest;

    public abstract GameObject GetChargeSlash();
    public abstract HeroControllerConfig GetConfig();
    public abstract void EditFsm(PlayMakerFSM fsm);
    public abstract void EditToolItem();
}