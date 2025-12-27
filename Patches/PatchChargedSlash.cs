using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using NeedleArts.Actions;
using NeedleArts.Managers;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;
using UnityEngine;

namespace NeedleArts.Patches;
[HarmonyPatch]
internal class PatchChargedSlash {
   // Patch config values to return equipped needle art values
   [HarmonyPatch(typeof(GetHeroAttackObject), nameof(GetHeroAttackObject.GetGameObject))]
   [HarmonyPostfix]
   private static void PatchGetGameObject(GetHeroAttackObject __instance, ref GameObject __result) {
      if ((GetHeroAttackObject.AttackObjects)__instance.Attack.Value != GetHeroAttackObject.AttackObjects.ChargeSlash) return;
      
      if (NeedleArtManager.Instance.GetActiveNeedleArt() is { } activeNeedleArt) {
         __result = activeNeedleArt.GetChargeSlash();
      } 
   }

   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashRecoils), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashRecoils(ref bool __result) {
      if (NeedleArtManager.Instance.GetActiveNeedleArt() is { } activeNeedleArt) {
         __result = activeNeedleArt.GetConfig().chargeSlashRecoils;
      } 
   }
   
   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashChain), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashChain(ref int __result) {
      if (NeedleArtManager.Instance.GetActiveNeedleArt() is { } activeNeedleArt) {
         __result = activeNeedleArt.GetConfig().chargeSlashChain;
      } 
   } 
   
   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashLungeSpeed), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashLungeSpeed(ref float __result) {
      if (NeedleArtManager.Instance.GetActiveNeedleArt() is { } activeNeedleArt) {
         __result = activeNeedleArt.GetConfig().chargeSlashLungeSpeed;
      } 
   }
   
   [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.ChargeSlashLungeDeceleration), MethodType.Getter)]
   [HarmonyPostfix]
   private static void PatchChargeSlashLungeDeceleration(ref float __result) {
      if (NeedleArtManager.Instance.GetActiveNeedleArt() is { } activeNeedleArt) {
         __result = activeNeedleArt.GetConfig().chargeSlashLungeDeceleration;
      } 
   }
   
   [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
   [HarmonyPostfix]
   private static void PatchNailArtsFSM(PlayMakerFSM __instance) {
      if (__instance is not { name: "Hero_Hornet(Clone)", FsmName: "Nail Arts" }) return;
      
      __instance.AddStringVariable("NeedleArtName");
      __instance.AddStringVariable("ClipName");
      
      var getChargeSlash = __instance.GetState("Get Charge Slash");
      getChargeSlash.InsertAction(0, new SetNeedleArt());
      
      var anticType = __instance.GetState("Antic Type");
      anticType.RemoveActionsOfType<CheckIfCrestEquipped>();
      
      foreach (var needleArt in NeedleArtManager.Instance.GetAllNeedleArts()) {
         needleArt.EditFsm(__instance);
      }

      var regainPartialControl = __instance.GetState("Regain Full Control");
      regainPartialControl.AddAction(new DelegateAction<NeedleArtManager> {
         Arg = NeedleArtManager.Instance,
         Method = manager => {
            manager.ResetActiveNeedleArt();
            NeedleArtsPlugin.Log.LogInfo("RESET");
            NeedleArtsPlugin.Log.LogInfo(manager);
         }
      });
   }
}