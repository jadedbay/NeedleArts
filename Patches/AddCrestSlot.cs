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
                AttackBinding = AttackToolBinding.Neutral,
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

    [HarmonyPatch(typeof(InventoryToolCrestSlot), nameof(InventoryToolCrestSlot.OnEnable))]
    [HarmonyPostfix]
    private static void DisableNeedleArtSlot(InventoryToolCrestSlot __instance) {
        if (__instance.Type == NeedleArtsPlugin.NeedleArtsToolType.Type && !PlayerData.instance.hasChargeSlash) {
            __instance.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.Awake))]
    [HarmonyPostfix]
    private static void ChangeHeaderSprite(InventoryItemToolManager __instance) {
        var header = __instance.listSectionHeaders[(int)NeedleArtsPlugin.NeedleArtsToolType.Type];
        
        var texture = Util.LoadTextureFromAssembly("NeedleArts.Resources.NeedleArtUIHeading.png");
        var sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, texture.width, texture.height), 
            new Vector2(0.5f, 0.5f), 
            64f
        );

        header.Sprite = sprite;
    }
}