using HarmonyLib;
using NeedleArts.Data;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class PatchPlayerDataTest {
    [HarmonyPatch(typeof(PlayerDataTest.TestGroup), nameof(PlayerDataTest.TestGroup.IsFulfilled))]
    [HarmonyPrefix]
    private static bool CheckPlayerData(PlayerDataTest.TestGroup __instance, PlayerDataBase playerData, ref bool __result) {
        if (playerData is not PlayerDataExt) return true;

        foreach (var test in __instance.Tests) {
            if (!test.IsFulfilled(playerData) && !test.IsFulfilled(PlayerData.instance)) {
                __result = false;
                return false;
            }
        }

        __result = true;
        return false;
    }
}