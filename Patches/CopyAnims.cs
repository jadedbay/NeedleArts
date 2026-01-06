using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NeedleArts.Utils;

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

      var animator = __instance.AnimCtrl.animator;
      artClips.Add(Util.CopyClip(animator.Library.clips.FirstOrDefault(c => c.name == "Slash_Charged"), "Hunter_Anim"));
      
      foreach (var config in __instance.configs) {
         if (config.Config.heroAnimOverrideLib == null) continue;
         if (config.Config.name == $"DuelistCrest_{NeedleArtsPlugin.Id}") continue;
         
         var clip = config.Config.heroAnimOverrideLib.clips.FirstOrDefault(c => c.name == "Slash_Charged");
         if (clip == null) continue;
         
         artClips.Add(Util.CopyClip(clip, $"{config.Config.name}_Anim"));
         
         var beastClip = config.Config.heroAnimOverrideLib.clips.FirstOrDefault(c => c.name == "NeedleArt Dash");
         if (beastClip != null) {
            artClips.Add(Util.CopyClip(beastClip, "NeedleArt Dash"));
         }
        
         var witchLoopClip = config.Config.heroAnimOverrideLib.clips.FirstOrDefault(c => c.name == "Slash_Charged_Loop");
         if (witchLoopClip != null) {
            artClips.Add(Util.CopyClip(witchLoopClip, "Witch_Loop"));
         }
      }

      animator.Library.clips = [
         ..animator.Library.clips,
         ..artClips,
      ];
      
      foreach (var config in __instance.configs) {
         if (config.Config.heroAnimOverrideLib == null) continue;
         if (config.Config.name == $"DuelistCrest_{NeedleArtsPlugin.Id}") continue;

         config.Config.heroAnimOverrideLib.clips = [
            ..config.Config.heroAnimOverrideLib.clips,
            ..artClips,
         ];
      }

      animator.Library.isValid = false;
      animator.Library.ValidateLookup();
      
      _hasAddedAnims = true;
   }
}