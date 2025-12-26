using HutongGames.PlayMaker;
using Silksong.FsmUtil;

namespace NeedleArts.Actions;

public class SetNeedleArt : FsmStateAction {
    public override void OnEnter() {
        var needleArt = NeedleArtsPlugin.GetSelectedNeedleArt();
        fsm.GetStringVariable("NeedleArtName").Value = needleArt.Name;
        fsm.GetStringVariable("ClipName").Value = needleArt.AnimName;
        
        Finish();
    }
}