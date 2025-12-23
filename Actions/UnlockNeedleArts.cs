using System.Linq;
using HutongGames.PlayMaker;

namespace NeedleArts.Actions;

public class UnlockNeedleArts : FsmStateAction {
    public override void OnEnter() {
        foreach (var (crestName, artName) in CrestArtUtil.GetAllPairs()) {
            if (ToolItemManager.GetCrestByName(crestName).Slots.Any(slot => slot.IsLocked)) continue;
        
            NeedleArtsPlugin.GetNeedleArtByName(artName).ToolItem.Unlock();
        }
        
        Finish();
    }
}