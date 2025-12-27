using System.Linq;
using HarmonyLib;
using NeedleArts.Actions;
using Silksong.FsmUtil;
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
                NeedleArtsPlugin.GetNeedleArtByName(CrestArtUtil.GetArtName(__instance.Crest.name)).ToolItem.Unlock();
            }
        }
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void UnlockAtPinstress(PlayMakerFSM __instance) {
        if (__instance is not { name: "Pinstress Interior Ground Sit", FsmName: "Behaviour" }) return;
        
        __instance.GetState("Save").AddActionAtIndex(new UnlockNeedleArts {
            manager = new(Resources
                .FindObjectsOfTypeAll<InventoryItemToolManager>()
                .FirstOrDefault(m => m.gameObject.scene.IsValid()).gameObject)
        }, 4);
    }
    
    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.AutoEquip), 
        typeof(ToolCrest), typeof(bool), typeof(bool))]
    [HarmonyPostfix]
    private static void AutoEquipOnUnlock(ToolCrest crest) {
        if (CrestArtUtil.GetArtName(crest.name) is { } artName) {
            ToolItemManager.AutoEquip(NeedleArtsPlugin.GetNeedleArtByName(artName).ToolItem);
        }
    }
    
    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.UnequipTool))]
    [HarmonyPostfix]
    private static void AutoEquip(InventoryItemToolManager __instance, InventoryToolCrestSlot slot) {
        if (slot.Type != NeedleArtsPlugin.NeedleArtsToolType.Type) return;

        var neutralSlot = ToolItemManager.GetCrestByName(PlayerData.instance.CurrentCrestID).Slots
            .Zip(slot.Crest.GetSlots(), (info, data) => (info, data))
            .Single(s => s.info.AttackBinding == AttackToolBinding.Neutral && s.data == slot);
        
        neutralSlot.data.SetEquipped(CrestArtUtil.GetCrestArt().ToolItem, isManual: true, refreshTools: true);
    }
}
