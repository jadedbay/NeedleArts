using HarmonyLib;
using NeedleArts.Utils;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class ChangeHeader {
    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.Awake))]
    [HarmonyPostfix]
    private static void ChangeHeaderSprite(InventoryItemToolManager __instance) {
        var header = __instance.listSectionHeaders[(int)NeedleArtsPlugin.ToolType()];
        
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