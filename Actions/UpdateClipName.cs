using HutongGames.PlayMaker;
using Silksong.FsmUtil;

namespace NeedleArts.Actions;

public class UpdateClipName : FsmStateAction {
    [RequiredField] public FsmString needleArtName;
    [RequiredField] public FsmString animName;

    public override void Reset() {
        needleArtName = null;
        animName = null;
    }

    public override void OnEnter() {
        if (NeedleArtsPlugin.GetCurrentNeedleArt() is not { } needleArt) return;
        
        if (needleArt.Name == needleArtName.Value) {
            fsm.GetStringVariable("ClipName").Value = animName.Value;
        }
        
        Finish();
    }
}