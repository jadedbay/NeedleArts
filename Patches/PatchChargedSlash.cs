using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.Patches;
[HarmonyPatch]
internal class PatchChargedSlash {
   private static GameObject _prevChargeSlash;
   private static HeroControllerConfig _prevConfig; 
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
   [HarmonyPostfix]
   private static void CopyAnims(HeroController __instance) {
      List<tk2dSpriteAnimationClip> artClips = [];

      var animator = __instance.GetComponent<tk2dSpriteAnimator>();
      artClips.Add(Util.CopyClip(animator.Library.GetClipByName("Slash_Charged"), "HunterArt"));

      var crestList = ToolItemManager.Instance.crestList;
      foreach (var needleArt in NeedleArtsPlugin.NeedleArts) {
         if (needleArt.Key == "HunterArt") continue;
         
         artClips.Add(
            Util.CopyClip(
               crestList.GetByName(needleArt.Value.CrestName).HeroConfig.heroAnimOverrideLib.clips[needleArt.Value.ClipId], // Get clip by index cause using GetClipByName breaks the animation.
               needleArt.Key)
         );
      }

      animator.Library.clips = [
         ..animator.Library.clips,
         ..artClips,
      ];

      foreach (var crest in ToolItemManager.Instance.crestList) {
         if (crest.HeroConfig is not { } config
             || config.name is "Cloakless")
            continue;
         
         config.heroAnimOverrideLib.clips = [
            ..config.heroAnimOverrideLib.clips,
            ..artClips,
         ];
      }
      
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.NeedleArts.Values.Any(art => art.ToolItem.IsEquipped);
   }

   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPrefix]
   private static void SetConfigPre(HeroController __instance) {
      if (_prevChargeSlash != null) __instance.CurrentConfigGroup.ChargeSlash = _prevChargeSlash;
      if (_prevConfig != null) __instance.CurrentConfigGroup.Config = _prevConfig;
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPostfix]
   private static void SetConfigPost(HeroController __instance) {
      _prevChargeSlash = __instance.CurrentConfigGroup.ChargeSlash;
      _prevConfig = Object.Instantiate(__instance.Config);
      
      var artEquipped = NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.Value.ToolItem.IsEquipped);
      if (artEquipped.Value == null) return;
      
      var artConfig = __instance.configs[artEquipped.Value.ConfigId];
      __instance.CurrentConfigGroup.ChargeSlash = artConfig.ChargeSlash;

      var hcConfig = __instance.Config;
      hcConfig.chargeSlashRecoils = artConfig.Config.ChargeSlashRecoils;
      hcConfig.chargeSlashChain = artConfig.Config.ChargeSlashChain;
      hcConfig.chargeSlashLungeSpeed = artConfig.Config.ChargeSlashLungeSpeed;
      hcConfig.chargeSlashLungeDeceleration = artConfig.Config.ChargeSlashLungeDeceleration;
      
      // Set clipName here since Hunter/Shaman share same antic state
      // TODO: Set clipName in startup and just change here if shaman/hunter
      var fsm = PlayMakerFSM.FindFsmOnGameObject(__instance.gameObject, "Nail Arts");
      fsm.GetState(artEquipped.Value.AnticName).GetAction<Tk2dPlayAnimationWithEvents>(artEquipped.Value.ActionId)
         .clipName = artEquipped.Key;
   }
   
   [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
   [HarmonyPostfix]
   private static void UseNeedleArtTools(PlayMakerFSM __instance) {
      if (__instance is not { name: "Hero_Hornet(Clone)", FsmName: "Nail Arts" }) return;
   
      var anticType = __instance.GetState("Antic Type");
      
      anticType.Actions = anticType.Actions
         .Where(action => action.GetType() != typeof(CheckIfCrestEquipped))
         .ToArray();

      foreach (var needleArt in NeedleArtsPlugin.NeedleArts) {
         anticType.AddAction(new CheckIfToolEquipped {
            Tool = new FsmObject { Value = needleArt.Value.ToolItem },
            trueEvent = FsmEvent.GetFsmEvent(needleArt.Value.EventName),
            storeValue = false,
         });
      }
      
      anticType.ChangeTransition("WARRIOR", "Warrior Antic");
   }

}