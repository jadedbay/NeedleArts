using HarmonyLib;
using Needleforge.Makers;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchToolMaker {
    [HarmonyPatch(typeof(ToolMaker), nameof(ToolMaker.AddCustomTool), typeof(ToolItem))]
    [HarmonyPostfix]
    private static void AddToolItem(ToolItem toolItem) {
        if (NeedleArtsPlugin.GetNeedleArtByName(toolItem.name) is { } needleArt) {
            needleArt.ToolItem = toolItem;
        }
    }
}