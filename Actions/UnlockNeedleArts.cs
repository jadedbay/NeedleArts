using System.Linq;
using HutongGames.PlayMaker;

namespace NeedleArts.Actions;

public class UnlockNeedleArts : FsmStateAction {
    public override void OnEnter() {
        // Equip art associated with crest
        var equippedCrest = PlayerData.instance.CurrentCrestID;
        foreach (var crest in ToolItemManager.GetAllCrests().Where(c => c.IsUnlocked)) {
            ToolItemManager.SetEquippedCrest(crest.name);
            ToolItemManager.AutoEquip(NeedleArtsPlugin.GetNeedleArtByName(CrestArtUtil.GetArtName(crest.name)).ToolItem);
        }
        ToolItemManager.SetEquippedCrest(equippedCrest);
       
        // Unlock art if all crest slots unlocked
        foreach (var (crestName, artName) in CrestArtUtil.GetAllPairs()) {
            if (ToolItemManager.GetCrestByName(crestName).Slots.All(slot => !slot.IsLocked)) {
                NeedleArtsPlugin.GetNeedleArtByName(artName).ToolItem.Unlock();
            }
        }
        
        Finish();
    }
}
