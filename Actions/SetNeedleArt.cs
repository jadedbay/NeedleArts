using HutongGames.PlayMaker;
using NeedleArts.Managers;
using Silksong.FsmUtil;

namespace NeedleArts.Actions;

public class SetNeedleArt : FsmStateAction {
    public override void OnEnter() {
        var needleArt = NeedleArtManager.Instance.SetActiveNeedleArt();
        fsm.GetStringVariable("NeedleArtName").Value = needleArt.Name;
        fsm.GetStringVariable("ClipName").Value = needleArt.AnimName;
        
        Finish();
    }
}