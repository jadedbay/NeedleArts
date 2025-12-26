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

        needleArt.EditToolItem();
    }
}