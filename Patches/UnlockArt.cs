using System.Linq;
using HarmonyLib;
using NeedleArts.Managers;
using NeedleArts.Utils;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class UnlockArt {
    [HarmonyPatch(typeof(InventoryToolCrestSlot), nameof(InventoryToolCrestSlot.SaveData), MethodType.Setter)]
    [HarmonyPrefix]
    private static void UnlockNeedleArtPre(InventoryToolCrestSlot __instance, out bool __state) {
        __state = __instance.SaveData.IsUnlocked;
    }
    
    [HarmonyPatch(typeof(InventoryToolCrestSlot), nameof(InventoryToolCrestSlot.SaveData), MethodType.Setter)]
    [HarmonyPostfix]
    private static void UnlockNeedleArtPost(InventoryToolCrestSlot __instance, bool __state) {
        if (!__state && __instance.SaveData.IsUnlocked) {
            if (!PlayerData.instance.hasChargeSlash) return;
            
            if (__instance.Crest.GetSlots().All(slot => !slot.IsLocked)) {
                NeedleArtManager.Instance.GetNeedleArtByName(CrestArtUtil.GetArtName(__instance.Crest.name))?.ToolItem.Unlock();
            }
        }
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void UnlockAtPinstress(PlayMakerFSM __instance) {
        if (__instance is not { name: "Pinstress Interior Ground Sit", FsmName: "Behaviour" }) return;
        
        __instance.GetState("Save").InsertAction(4, new DelegateAction<InventoryItemToolManager> {
            Arg = Resources.FindObjectsOfTypeAll<InventoryItemToolManager>().FirstOrDefault(m => m.gameObject.scene.IsValid()),
            Method = manager => {
                ToolItemManagerUtil.AutoEquip("Hunter", NeedleArtManager.Instance.GetNeedleArtByName("HunterArt").ToolItem);
       
                // Unlock art if all crest slots unlocked
                foreach (var crest in manager.GetComponent<InventoryItemToolManager>().crestList.crests) {
                    if (crest.GetSlots().All(slot => !slot.IsLocked)) {
                        NeedleArtManager.Instance.GetNeedleArtByName(CrestArtUtil.GetArtName(crest.name)).ToolItem.Unlock();
                    }
                }
            }
        });
    }
    
    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.AutoEquip), 
        typeof(ToolCrest), typeof(bool), typeof(bool))]
    [HarmonyPostfix]
    private static void AutoEquipOnUnlock(ToolCrest crest) {
        if (!PlayerData.instance.hasChargeSlash) return;
        
        AutoEquipArt(CrestArtUtil.GetCrestArt().ToolItem);
    }
   
    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.UnequipTool))]
    [HarmonyPostfix]
    private static void AutoEquip(InventoryItemToolManager __instance, InventoryToolCrestSlot slot) {
        if (slot.Type != NeedleArtsPlugin.ToolType()) return;

        AutoEquipArt(CrestArtUtil.GetCrestArt().ToolItem);
    }

    private static void AutoEquipArt(ToolItem needleArt) {
        var manager = Resources.FindObjectsOfTypeAll<InventoryItemToolManager>()
            .FirstOrDefault(m => m.gameObject.scene.IsValid());

        foreach (var floatingSlot in manager.extraSlots.GetSlots()) {
            if (floatingSlot.Type != NeedleArtsPlugin.ToolType()) continue;
            floatingSlot.SetEquipped(needleArt, isManual: true, refreshTools: true);
        }
    }
}
