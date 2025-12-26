using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using NeedleArts.Actions;
using NeedleArts.Managers;
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
                NeedleArtManager.Instance.GetNeedleArtByName(CrestArtUtil.GetArtName(__instance.Crest.name))?.ToolItem.Unlock();
            }
        }
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void UnlockAtPinstress(PlayMakerFSM __instance) {
        if (__instance is not { name: "Pinstress Interior Ground Sit", FsmName: "Behaviour" }) return;
        
        __instance.GetState("Save").InsertAction(4, 
            new UnlockNeedleArts {
                manager = new FsmGameObject(Resources
                    .FindObjectsOfTypeAll<InventoryItemToolManager>()
                    .FirstOrDefault(m => m.gameObject.scene.IsValid()).gameObject)
            }
        );
    }
    
    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.AutoEquip), 
        typeof(ToolCrest), typeof(bool), typeof(bool))]
    [HarmonyPostfix]
    private static void AutoEquipOnUnlock(ToolCrest crest) {
        if (CrestArtUtil.GetArtName(crest.name) is { } artName) {
            ToolItemManager.AutoEquip(NeedleArtManager.Instance.GetNeedleArtByName(artName).ToolItem);
        }
    }
    
    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.UnequipTool))]
    [HarmonyPostfix]
    private static void AutoEquip(InventoryItemToolManager __instance, InventoryToolCrestSlot slot) {
        if (slot.Type == NeedleArtsPlugin.NeedleArtsToolType.Type) {
            var artTool = NeedleArtManager.Instance.GetNeedleArtByName(CrestArtUtil.GetArtName(PlayerData.instance.CurrentCrestID));
            __instance.TryPickupOrPlaceTool(artTool.ToolItem);
        }
    }
}
