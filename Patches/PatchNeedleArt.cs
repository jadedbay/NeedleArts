using HarmonyLib;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchNeedleArt {
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.needleArtTool.IsEquipped;
   }
}