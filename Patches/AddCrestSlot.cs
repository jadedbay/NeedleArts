using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class AddCrestSlot {
    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.Awake))]
    [HarmonyPostfix]
    private static void AddNeedleArtSlot(ToolItemManager __instance) {
        foreach (var crest in __instance.crestList) {
            if (crest.Slots.IsNullOrEmpty()) continue;

            var minY = crest.Slots.Min(s => s.Position.y);
            var minIndex = Array.FindIndex(crest.Slots, x => x.Position.y == minY);
            crest.Slots[minIndex].NavDownIndex = crest.Slots.Length;

            var slots = crest.Slots.ToList();
            slots.Add(new ToolCrest.SlotInfo {
                IsLocked = false,
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
            });

            crest.slots = slots.ToArray();
        }
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
    }}