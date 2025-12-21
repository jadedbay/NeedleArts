using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.ArtTools;

public class CrestArt(string name, string eventName, string anticName, string animName, int configId)
    : NeedleArt(name) {
    private string EventName { get; } = eventName;
    private string AnticName { get; } = anticName;
    private string AnimName { get; } = animName;
    private int ConfigId { get; } = configId;

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
         
        fsm.GetState(AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
            .clipName = AnimName;
    }

    public override void UpdateFsm(PlayMakerFSM fsm) {
        fsm.GetStringVariable("ClipName").Value = AnimName;
    } 
}
