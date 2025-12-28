using HarmonyLib;
using NeedleArts.ArtTools;
using NeedleArts.Managers;
using Needleforge.Makers;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchToolMaker {
    [HarmonyPatch(typeof(ToolMaker), nameof(ToolMaker.AddCustomTool), typeof(ToolItem))]
    [HarmonyPostfix]
    private static void AddToolItem(ToolItem toolItem) {
        var needleArt = NeedleArtManager.Instance.GetNeedleArtByName(toolItem.name);
        needleArt.ToolItem = toolItem;

        if (NeedleArtsPlugin.SimpleUnlock.Value) {
            (needleArt as CrestArt)?.AddSimpleUnlockTest();
        }
    }

    [HarmonyPatch(typeof(ToolItem), nameof(ToolItem.IsCounted), MethodType.Getter)]
    [HarmonyPostfix]
    private static void RemovePercentage(ToolItem __instance, ref bool __result) {
        if (__instance.Type == NeedleArtsPlugin.NeedleArtsToolType.Type) {
            __result = false;
        }
    }
}