using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchChargedSlash {
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
   [HarmonyPostfix]
   private static void CacheChargedSlash(HeroController __instance) {
      var attacks = __instance.transform.Find("Attacks");

      foreach (var needleArt in NeedleArtsPlugin.NeedleArts.Values) {
         needleArt.ChargedSlash = attacks.Find(needleArt.ChargedSlashName).gameObject;
      }

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
         ..artClips.ToArray(),
      ];

      foreach (var crest in ToolItemManager.Instance.crestList) {
         if (crest.HeroConfig is not { } config
             || config.name is "Cloakless")
            continue;
         
         config.heroAnimOverrideLib.clips = [
            ..config.heroAnimOverrideLib.clips,
            ..artClips.ToArray(),
         ];
      }
      
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.NeedleArts.Values.Any(art => art.ToolItem.IsEquipped);
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPostfix]
   private static void SetChargedSlash(HeroController __instance) {
      var artEquipped = NeedleArtsPlugin.NeedleArts.FirstOrDefault(art => art.Value.ToolItem.IsEquipped);

      __instance.CurrentConfigGroup.ChargeSlash = artEquipped.Value.ChargedSlash;

      var hcConfig = __instance.Config;
      var artConfig = __instance.configs[artEquipped.Value.ConfigId].Config;

      hcConfig.chargeSlashRecoils = artConfig.ChargeSlashRecoils;
      hcConfig.chargeSlashChain = artConfig.ChargeSlashChain;
      hcConfig.chargeSlashLungeSpeed = artConfig.ChargeSlashLungeSpeed;
      hcConfig.chargeSlashLungeDeceleration = artConfig.ChargeSlashLungeDeceleration;
      
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
