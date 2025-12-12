using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class AddCrestSlot {
    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.Awake))]
    [HarmonyPostfix]
    public static void AddNeedleArtSlot(ToolItemManager __instance) {
        foreach (var crest in __instance.crestList) {
            var slots = crest.Slots.ToList();
            slots.Add(new ToolCrest.SlotInfo {
                IsLocked = false,
                Position = new Vector2(0.0f, -4.43f),
                NavUpFallbackIndex = -1,
                NavDownFallbackIndex = -1,
                NavLeftFallbackIndex = -1,
                NavRightFallbackIndex = -1,
                NavDownIndex = -1,
                NavLeftIndex = 1,
                NavRightIndex = 2,
                Type = NeedleArtsPlugin.NeedleArtsToolType.Type,
            });
            
            crest.slots = slots.ToArray();
        }
    }
}
