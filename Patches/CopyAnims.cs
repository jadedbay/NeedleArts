using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class CopyAnims {
   private static bool _hasAddedAnims;
   
   // Add all needle art animations to all config groups
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
   [HarmonyPostfix]
   private static void CopyAnimations(HeroController __instance) {
      if (_hasAddedAnims) return;
      
      List<tk2dSpriteAnimationClip> artClips = [];

      var animator = __instance.GetComponent<tk2dSpriteAnimator>();
      artClips.Add(Util.CopyClip(animator.Library.clips.FirstOrDefault(c => c.name == "Slash_Charged"), "Hunter_Anim"));
      
      foreach (var config in __instance.configs) {
         if (config.Config.heroAnimOverrideLib == null) continue;
         
         var clip = config.Config.heroAnimOverrideLib.clips.FirstOrDefault(c => c.name == "Slash_Charged");
         if (clip == null) continue;
         
         artClips.Add(Util.CopyClip(clip, $"{config.Config.name}_Anim"));
         
         var beastClip = config.Config.heroAnimOverrideLib.clips.FirstOrDefault(c => c.name == "NeedleArt Dash");
         if (beastClip == null) continue;
         artClips.Add(Util.CopyClip(beastClip, "NeedleArt Dash"));
         
         var witchLoopClip = config.Config.heroAnimOverrideLib.clips.FirstOrDefault(c => c.name == "Slash_Charged_Loop");
         if (witchLoopClip == null) continue;
         artClips.Add(Util.CopyClip(beastClip, "Slash_Charged_Loop"));
      }

      animator.Library.clips = [
         ..animator.Library.clips,
         ..artClips,
      ];
      
      foreach (var config in __instance.configs) {
         if (config.Config.heroAnimOverrideLib == null) continue;

         config.Config.heroAnimOverrideLib.clips = [
            ..config.Config.heroAnimOverrideLib.clips,
            ..artClips,
         ];
      }
      
      _hasAddedAnims = true;
   }
}