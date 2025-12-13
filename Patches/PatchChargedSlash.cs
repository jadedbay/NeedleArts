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
   }
   
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
   [HarmonyPrefix]
   private static bool IsNeedleArtEquipped() {
      return NeedleArtsPlugin.NeedleArts.Values.Any(art => art.ToolItem.IsEquipped);
   }
  
   [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
   [HarmonyPostfix]
   private static void SetChargedSlash(HeroController __instance) {
      __instance.CurrentConfigGroup.ChargeSlash =
         NeedleArtsPlugin.NeedleArts.Values.FirstOrDefault(art => art.ToolItem.IsEquipped)?.ChargedSlash;
   }
   
   [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
   [HarmonyPostfix]
   private static void UseNeedleArtTools(PlayMakerFSM __instance) {
      if (__instance is not { name: "Hero_Hornet(Clone)", FsmName: "Nail Arts" }) return;
   
      var anticType = __instance.Fsm.GetState("Antic Type");
      
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
