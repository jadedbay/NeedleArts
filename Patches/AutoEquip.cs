using HarmonyLib;
using NeedleArts.Managers;
using NeedleArts.Utils;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class AutoEquip {
    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Awake))]
    [HarmonyPostfix]
    private static void EquipAtPinstress(PlayMakerFSM __instance) {
        if (__instance is not { name: "Pinstress Interior Ground Sit", FsmName: "Behaviour" }) return;
        
        __instance.GetState("Save").InsertAction(4, new DelegateAction<object> {
            Arg = new object(),
            Method = _ => {
                NeedleArtManager.AutoEquipArt(CrestArtUtil.GetCrestArt().ToolItem);
            }
        });
    }
}