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
      return NeedleArtsPlugin.NeedleArts.Any(art => art.ToolItem.IsEquipped);
   }
 
   // Update config values to match equipped needle art
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPostfix]
   private static void SetConfigPost(HeroController __instance) {
      var artEquipped = NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped);
      if (artEquipped == null) return;
      
      if (artEquipped.Name is "HunterArt" or "WitchArt" or "ShamanArt") {
         var fsm = PlayMakerFSM.FindFsmOnGameObject(__instance.gameObject, "Nail Arts");
         fsm.GetState(artEquipped.AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
            .clipName = artEquipped.AnimName;
      } 
   }

   [HarmonyPatch(typeof(GetHeroAttackObject), nameof(GetHeroAttackObject.GetGameObject))]
   [HarmonyPostfix]
   private static void PatchGetGameObject(GetHeroAttackObject __instance, ref GameObject __result) {
      if ((GetHeroAttackObject.AttackObjects)__instance.Attack.Value != GetHeroAttackObject.AttackObjects.ChargeSlash) return;
      
      if (NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped) is { } artEquipped) {
         __result = HeroController.instance.configs[artEquipped.ConfigId].ChargeSlash;
      } 
   }

   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashRecoils), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashRecoils(ref bool __result) {
      if (NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped) is { } artEquipped) {
         __result = HeroController.instance.configs[artEquipped.ConfigId].Config.chargeSlashRecoils;
      } 
   }
   
   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashChain), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashChain(ref int __result) {
      if (NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped) is { } artEquipped) {
         __result = HeroController.instance.configs[artEquipped.ConfigId].Config.chargeSlashChain;
      } 
   } 
   
   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashLungeSpeed), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashLungeSpeed(ref float __result) {
      if (NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped) is { } artEquipped) {
         __result = HeroController.instance.configs[artEquipped.ConfigId].Config.chargeSlashLungeSpeed;
      } 
   }
   
   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashLungeDeceleration), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashLungeDeceleration(ref float __result) {
      if (NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped) is { } artEquipped) {
         __result = HeroController.instance.configs[artEquipped.ConfigId].Config.chargeSlashLungeDeceleration;
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
      
      foreach (var needleArt in NeedleArtsPlugin.NeedleArts) {
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
}