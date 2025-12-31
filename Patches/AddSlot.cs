using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class AddSlot {
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Awake))]
    [HarmonyPostfix]
    private static void SpawnSlot(InventoryFloatingToolSlots __instance) {
        var defendSlot = __instance.transform.Find("Defend Slot").gameObject;
        var artSlot = Object.Instantiate(defendSlot, __instance.transform);
        artSlot.name = "NeedleArt Slot";
        artSlot.GetComponent<InventoryToolCrestSlot>().slotInfo.Type = NeedleArtsPlugin.ToolType();
        artSlot.SetActive(false);
        
        var cursedSlot = __instance.transform.Find("Cursed Socket Top").gameObject;
        var cursedArtSlot = Object.Instantiate(cursedSlot, __instance.transform);
        
        var needleArtConfigs = new List<InventoryFloatingToolSlots.Config>();
        foreach (var config in __instance.configs) {
            var condition = new PlayerDataTest {
                TestGroups = [
                    new PlayerDataTest.TestGroup {
                        Tests = [..config.Condition.TestGroups[0].Tests, 
                            new PlayerDataTest.Test {
                                FieldName = "hasChargeSlash",
                                Type = PlayerDataTest.TestType.Bool,
                                BoolValue = true
                            }
                        ]
                    }
                ]
             };
            
            needleArtConfigs.Add(new InventoryFloatingToolSlots.Config {
                Slots = [..config.Slots, new InventoryFloatingToolSlots.Slot {
                    SlotObject = artSlot.GetComponent<InventoryToolCrestSlot>(),
                    Type = NeedleArtsPlugin.ToolType(),
                    Id = "NeedleArtsSlot",
                    CursedSlot = cursedArtSlot,
                }],
                Brackets = config.Brackets,
                PositionOffset = config.PositionOffset,
                Condition = condition,
            });
        }

        __instance.configs = [..__instance.configs, ..needleArtConfigs];
        
        var data = PlayerData.instance.ExtraToolEquips.GetData("NeedleArtsSlot");
        if (string.IsNullOrEmpty(data.EquippedTool)) {
            PlayerData.instance.ExtraToolEquips.SetData("NeedleArtsSlot", new ToolCrestsData.SlotData());
        }
    }
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Evaluate))]
    [HarmonyPostfix]
    private static void FixAnimatorAfterEvaluate(InventoryFloatingToolSlots __instance) {
        if (__instance.transform.Find("NeedleArt Slot") is not { } artSlot) return;
        if (__instance.transform.Find("Defend Slot") is not { } defendSlot) return;
        
        artSlot.Find("Background Group/Background")
                .GetComponent<Animator>().runtimeAnimatorController =
            defendSlot.Find("Background Group/Background")
                .GetComponent<Animator>().runtimeAnimatorController;
    }
}

