using System.Linq;
using HutongGames.PlayMaker;
using NeedleArts.Managers;

namespace NeedleArts.Actions;

public class UnlockNeedleArts : FsmStateAction {
    public FsmGameObject manager;

    public override void Reset() {
        manager = new FsmGameObject();
    }

    public override void OnEnter() {
        ToolItemManagerUtil.AutoEquip("Hunter", NeedleArtManager.Instance.GetNeedleArtByName("HunterArt").ToolItem);
       
        // Unlock art if all crest slots unlocked
        foreach (var crest in manager.Value.GetComponent<InventoryItemToolManager>().crestList.crests) {
            if (crest.GetSlots().All(slot => !slot.IsLocked)) {
                NeedleArtManager.Instance.GetNeedleArtByName(CrestArtUtil.GetArtName(crest.name)).ToolItem.Unlock();
            }
            
        }
        
        Finish();
    }
}
