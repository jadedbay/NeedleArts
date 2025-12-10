using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace NeedleArts.Patches;

[HarmonyPatch]
internal class PatchChargedSlash {
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
   [HarmonyPostfix]
   private static void CacheChargedSlash(HeroController __instance) {
      var attacks = __instance.transform.Find("Attacks");
    
      foreach (var slashName in NeedleArtsPlugin.ChargeSlashNames) {
         NeedleArtsPlugin.cachedChargeSlashes.Add(slashName, attacks?.Find(slashName)?.gameObject);
      }
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.needleArtTools.Any(tool => tool.Value.IsEquipped);
   }

   [HarmonyPatch(typeof(GetHeroAttackObject), nameof(GetHeroAttackObject.GetGameObject))]
   [HarmonyPostfix]
   private static void GetNeedleArt(ref GameObject __result) {
      if (__result != HeroController.instance.CurrentConfigGroup.ChargeSlash) return;
    
      var equippedTool = NeedleArtsPlugin.needleArtTools.FirstOrDefault(tool => tool.Value.IsEquipped);
    
      if (equippedTool.Value != null && NeedleArtsPlugin.cachedChargeSlashes.TryGetValue(equippedTool.Key, out var slashObject)) {
         __result = slashObject;
      }
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