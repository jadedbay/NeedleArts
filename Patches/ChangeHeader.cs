using System.Reflection;
using HarmonyLib;
using Silksong.UnityHelper.Util;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class ChangeHeader {
    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.Awake))]
    [HarmonyPostfix]
    private static void ChangeHeaderSprite(InventoryItemToolManager __instance) {
        var header = __instance.listSectionHeaders[(int)NeedleArtsPlugin.ToolType()];
        
        var sprite = SpriteUtil.LoadEmbeddedSprite(Assembly.GetExecutingAssembly(),
            "NeedleArts.Resources.Sprites.NeedleArtUIHeading.png", 
            64f
        );

        header.Sprite = sprite;
    }
}