using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.Patches;
[HarmonyPatch]
internal class AddNeedleArts {
   private static GameObject _prevChargeSlash;
   private static HeroControllerConfig _prevConfig;
  
   // Add all needle art animations to all config groups
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
   [HarmonyPostfix]
   private static void CopyAnims(HeroController __instance) {
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
   }
 
   // Disable charged slash if no needle art equipped
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.NeedleArts.Values.Any(art => art.ToolItem.IsEquipped);
   }

   // Set config values back
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPrefix]
   private static void SetConfigPre(HeroController __instance) {
      if (_prevChargeSlash != null) __instance.CurrentConfigGroup.ChargeSlash = _prevChargeSlash;
      if (_prevConfig != null) __instance.CurrentConfigGroup.Config = _prevConfig;
   }
 
   // Update config values to match equipped needle art
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPostfix]
   private static void SetConfigPost(HeroController __instance) {
      var artEquipped = NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.Value.ToolItem.IsEquipped);
      if (artEquipped.Value == null) return;
      
      _prevChargeSlash = __instance.CurrentConfigGroup.ChargeSlash;
      _prevConfig = Object.Instantiate(__instance.Config);
    
      var artConfig = __instance.configs[artEquipped.Value.ConfigId];
      __instance.CurrentConfigGroup.ChargeSlash = artConfig.ChargeSlash;
      
      var hcConfig = __instance.Config;
      hcConfig.chargeSlashRecoils = artConfig.Config.ChargeSlashRecoils;
      hcConfig.chargeSlashChain = artConfig.Config.ChargeSlashChain;
      hcConfig.chargeSlashLungeSpeed = artConfig.Config.ChargeSlashLungeSpeed;
      hcConfig.chargeSlashLungeDeceleration = artConfig.Config.ChargeSlashLungeDeceleration;
     
      if (artEquipped.Key is "HunterArt" or "WitchArt" or "ShamanArt") {
         var fsm = PlayMakerFSM.FindFsmOnGameObject(__instance.gameObject, "Nail Arts");
         fsm.GetState(artEquipped.Value.AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
            .clipName = artEquipped.Value.AnimName;
      } 
   }   
   
   [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
   [HarmonyPostfix]
   private static void UseNeedleArtTools(PlayMakerFSM __instance) {
      if (__instance is not { name: "Hero_Hornet(Clone)", FsmName: "Nail Arts" }) return;
   
      var anticType = __instance.GetState("Antic Type");
      
      anticType.Actions = anticType.Actions
         .Where(action => action.GetType() != typeof(CheckIfCrestEquipped))
         .ToArray();
      
      foreach (var needleArt in NeedleArtsPlugin.NeedleArts.Values) {
         anticType.AddAction(new CheckIfToolEquipped {
            Tool = new FsmObject { Value = needleArt.ToolItem },
            trueEvent = FsmEvent.GetFsmEvent(needleArt.EventName),
            storeValue = false,
         });

         __instance.GetState(needleArt.AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
            .clipName = needleArt.AnimName;
      }
      
      //anticType.ChangeTransition("WARRIOR", "Warrior Antic");
   }

   [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMainMenu))]
   [HarmonyPostfix]
   private static void MainMenu() => ResetVariables();
   
   [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMainMenuNoSave))]
   [HarmonyPostfix]
   private static void MainMenuNoSave() => ResetVariables();
   
   [HarmonyPatch(typeof(GameManager), nameof(GameManager.EmergencyReturnToMenu))]
   [HarmonyPostfix]
   private static void MainMenuEmergency() => ResetVariables();

   private static void ResetVariables() {
      _prevChargeSlash = null;
      _prevConfig = null;
   }
}