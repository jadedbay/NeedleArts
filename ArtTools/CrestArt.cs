using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using NeedleArts.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.ArtTools;

public class CrestArt(string name, string eventName, string anticName, string animName, int configId, bool useDynamicClipUpdate = false)
    : NeedleArt(name) {
    public string EventName { get; } = eventName;
    public int ConfigId { get; } = configId;
    public string AnticName { get; } = anticName;
    public string AnimName { get; } = animName;

    public override GameObject GetChargeSlash() {
        return HeroController.instance.configs[ConfigId].ChargeSlash;
    }

    public override HeroControllerConfig GetConfig() {
        return HeroController.instance.configs[ConfigId].Config;
    }

    public override void EditFsm(PlayMakerFSM fsm) {
        var anticType = fsm.GetState("Antic Type");
        
        anticType.AddAction(new CheckIfToolEquipped {
            Tool = new FsmObject { Value = ToolItem },
            trueEvent = FsmEvent.GetFsmEvent(EventName),
            storeValue = false,
        });

        if (useDynamicClipUpdate) {
            fsm.GetState(AnticName).AddActionAtIndex(new UpdateClipName {
                needleArtName = Name,
                animName = AnimName,
            }, 0);
        }
        else {
            fsm.GetState(AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
                .clipName = AnimName;
        }
    }
}