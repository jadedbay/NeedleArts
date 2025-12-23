using System.Collections;
using System.Linq;
using HarmonyLib;

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
            
            var artName = __instance.Crest.name switch {
                "Hunter" => "HunterArt",
                "Reaper" => "ReaperArt",
                "Wanderer" => "WandererArt",
                "Warrior" => "BeastArt",
                "Witch" => "WitchArt",
                "Toolmaster" => "ArchitectArt",
                "Spell" => "ShamanArt",
                _ => null,
            };
            if (artName == null) return;
            
            NeedleArtsPlugin.GetNeedleArtByName(artName).ToolItem.Unlock();
        }
    }
}