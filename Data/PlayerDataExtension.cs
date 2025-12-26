using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace NeedleArts.Data;

public class PlayerDataExtension : PlayerDataBase {
    public List<PlayerDataBase> DataList { get;  } = [];

    public PlayerDataExtension(params PlayerDataBase[] dataSources) {
        DataList.AddRange(dataSources);
    }
}

[HarmonyPatch]
public class PatchPlayerDataTest {
    [HarmonyPatch(typeof(PlayerDataTest.TestGroup), nameof(PlayerDataTest.TestGroup.IsFulfilled))]
    [HarmonyPrefix]
    private static bool CheckPlayerData(PlayerDataTest.TestGroup __instance, PlayerDataBase playerData, ref bool __result) {
        if (playerData is not PlayerDataExtension dataExt) return true;

        foreach (var test in __instance.Tests) {
            if (dataExt.DataList.All(data => !test.IsFulfilled(data)) && !test.IsFulfilled(PlayerData.instance)) {
                __result = false;
                return false;
            }
        }

        __result = true;
        return false;
    }
}
