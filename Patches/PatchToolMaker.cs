using HarmonyLib;
using NeedleArts.ArtTools;
using Needleforge.Makers;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchToolMaker {
    [HarmonyPatch(typeof(ToolMaker), nameof(ToolMaker.AddCustomTool), typeof(ToolItem))]
    [HarmonyPostfix]
    private static void AddToolItem(ToolItem toolItem) {
        var needleArt = NeedleArtsPlugin.GetNeedleArtByName(toolItem.name);
        needleArt.ToolItem = toolItem;

        if (NeedleArtsPlugin.SimpleUnlock.Value) {
            (needleArt as CrestArt)?.AddSimpleUnlockTest();
        }
    }
}