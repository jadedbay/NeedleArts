using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class AddCrestSlot {
    private static bool _hasAddedSlots;

    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.Awake))]
    [HarmonyPostfix]
    private static void AddNeedleArtSlot(ToolItemManager __instance) {
        if (_hasAddedSlots) return; 
        
        foreach (var crest in __instance.crestList) {
            if (crest.Slots.IsNullOrEmpty()) continue;

            var minY = crest.Slots.Min(s => s.Position.y);
            var minIndex = Array.FindIndex(crest.Slots, x => x.Position.y == minY);
            crest.Slots[minIndex].NavDownIndex = crest.Slots.Length;

            var slot = new ToolCrest.SlotInfo {
                IsLocked = true,
                Position = new Vector2(0.0f, -4.43f),
                NavUpFallbackIndex = -1,
                NavDownFallbackIndex = -1,
                NavLeftFallbackIndex = -1,
                NavRightFallbackIndex = -1,
                NavUpIndex = minIndex,
                NavDownIndex = crest.Slots.Length,
                NavLeftIndex = -1,
                NavRightIndex = -1,
                Type = NeedleArtsPlugin.NeedleArtsToolType.Type,
            };

            crest.slots = [..crest.Slots, slot];
        }

        _hasAddedSlots = true;
    }
    
    [HarmonyPatch(typeof(InventoryItemSelectableDirectional), nameof(InventoryItemSelectableDirectional.GetNextSelectable), typeof(InventoryItemManager.SelectionDirection))]
    [HarmonyPrefix]
    private static void FixChangeCrestNav(
        ref InventoryItemManager.SelectionDirection direction,
        InventoryItemSelectableDirectional __instance
    ) {
        if (__instance.name != "Change Crest Button") return;
    
        direction = direction switch {
            InventoryItemManager.SelectionDirection.Up => InventoryItemManager.SelectionDirection.Left,
            InventoryItemManager.SelectionDirection.Right => InventoryItemManager.SelectionDirection.Up,
            _ => direction
        };
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.CanUnlockSlot), MethodType.Getter)]
    [HarmonyPostfix]
    private static void DisableSlotUnlock(InventoryItemToolManager __instance, ref bool __result) {
        if ((__instance.CurrentSelected as InventoryToolCrestSlot) is not { } slot) return;
        
        if (slot.Type == NeedleArtsPlugin.NeedleArtsToolType.Type) {
            __result = false;
        }
    }
}