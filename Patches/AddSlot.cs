using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class AddSlot {
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Awake))]
    [HarmonyPostfix]
    private static void SpawnSlot(InventoryFloatingToolSlots __instance) {
        var defendSlot = __instance.transform.Find("Defend Slot").gameObject;
        var artSlot = Object.Instantiate(defendSlot, __instance.transform);
        artSlot.name = "NeedleArt Slot";
        artSlot.GetComponent<InventoryToolCrestSlot>().slotInfo.Type = NeedleArtsPlugin.ToolType();
        artSlot.SetActive(false);
        
        var cursedSlot = __instance.transform.Find("Cursed Socket Top").gameObject;
        var cursedArtSlot = Object.Instantiate(cursedSlot, __instance.transform);
        
        /**
        foreach (var config in __instance.configs) {
            config.Slots = [..config.Slots, new InventoryFloatingToolSlots.Slot {
                SlotObject = artSlot.GetComponent<InventoryToolCrestSlot>(),
                Type = NeedleArtsPlugin.ToolType(),
                Id = "NeedleArtsSlot",
                CursedSlot = cursedArtSlot,
            }];
        }
        **/
        
        var existingData = PlayerData.instance.ExtraToolEquips.GetData("NeedleArtsSlot");
        if (string.IsNullOrEmpty(existingData.EquippedTool)) {
            PlayerData.instance.ExtraToolEquips.SetData("NeedleArtsSlot", new ToolCrestsData.SlotData {
                EquippedTool = null,
            });
        }
    }
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Evaluate))]
    [HarmonyPostfix]
    private static void FixAnimatorAfterEvaluate(InventoryFloatingToolSlots __instance) {
        if (__instance.transform.Find("NeedleArt Slot") is not { } artSlot) return;
        if (__instance.transform.Find("Defend Slot") is not { } defendSlot) return;
        
        artSlot.Find("Background Group/Background")
                .GetComponent<Animator>().runtimeAnimatorController =
            defendSlot.Find("Background Group/Background")
                .GetComponent<Animator>().runtimeAnimatorController;
    }
}

