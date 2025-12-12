using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchChargedSlash {
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
   [HarmonyPostfix]
   private static void CacheChargedSlash(HeroController __instance) {
      var attacks = __instance.transform.Find("Attacks");
    
      foreach (var slashName in NeedleArtsPlugin.ChargeSlashNames) {
         if (NeedleArtsPlugin.needleArts.TryGetValue(slashName, out var needleArt)) {
            needleArt.ChargeSlash = attacks?.Find(slashName)?.gameObject;
         }
      }
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.needleArts.Values.Any(art => art.Tool.IsEquipped);
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPostfix]
   private static void SetChargedSlash(HeroController __instance) {
      if (NeedleArtsPlugin.needleArts.IsNullOrEmpty()) return;
      
      __instance.CurrentConfigGroup.ChargeSlash =
         NeedleArtsPlugin.needleArts.Values.FirstOrDefault(art => art.Tool.IsEquipped).ChargeSlash;
   }

   [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
   [HarmonyPostfix]
   private static void UseNeedleArtTools(PlayMakerFSM __instance) {
      if (__instance is not { name: "Hero_Hornet(Clone)", FsmName: "Nail Arts" })
         return;
   
      /**
      FsmState anticType = __instance.Fsm.GetState("Antic Type");
      anticType.Actions = anticType.Actions
         .Where(action => action.GetType() != typeof(CheckIfCrestEquipped))
         .ToArray();
         **/
      
   }
}