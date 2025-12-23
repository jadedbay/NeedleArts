using System.Collections;
using System.Linq;
using HarmonyLib;
using NeedleArts.Actions;
using Silksong.FsmUtil;

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
            if (__instance.Crest.CrestData.Slots.Any(slot => slot.IsLocked)) return;
            if (!PlayerData.instance.hasChargeSlash) return;

            if (CrestArtUtil.GetArtName(__instance.Crest.name) is { } artName) {
                NeedleArtsPlugin.GetNeedleArtByName(artName).ToolItem.Unlock();
            }
        }
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void UnlockAtPinstress(PlayMakerFSM __instance) {
        if (__instance is not { name: "Pinstress Interior Ground Sit", FsmName: "Behaviour" }) return;
        
        __instance.GetState("Save").AddActionAtIndex(new UnlockNeedleArts(), 4);
    }
}