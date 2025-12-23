using System.Linq;
using HutongGames.PlayMaker;

namespace NeedleArts.Actions;

public class UnlockNeedleArts : FsmStateAction {
    public override void OnEnter() {
        ToolItemManagerUtil.AutoEquip("Hunter", NeedleArtsPlugin.GetNeedleArtByName("HunterArt").ToolItem);
       
        // Unlock art if all crest slots unlocked
        foreach (var (crestName, artName) in CrestArtUtil.GetAllPairs()) {
            if (ToolItemManager.GetCrestByName(crestName).Slots.All(slot => !slot.IsLocked)) {
                NeedleArtsPlugin.GetNeedleArtByName(artName).ToolItem.Unlock();
            }
        }
        
        Finish();
    }
}
