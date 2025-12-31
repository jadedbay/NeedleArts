using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class AddSlot {
    private static Transform _bracket1;
    private static Transform _bracket2;
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Awake))]
    [HarmonyPostfix]
    private static void SpawnSlot(InventoryFloatingToolSlots __instance) {
        var artSlot = Object.Instantiate(
            __instance.transform.Find("Defend Slot").gameObject,
            __instance.transform
        );
        artSlot.name = "NeedleArt Slot";
        artSlot.SetActive(false);
        
        artSlot.GetComponent<InventoryToolCrestSlot>().slotInfo.Type = 
            NeedleArtsPlugin.ToolType();
        
        var cursedSlot = __instance.transform.Find("Cursed Socket Top").gameObject;
        var cursedArtSlot = Object.Instantiate(cursedSlot, __instance.transform);
        
        var bracket = __instance.transform.Find("Brackets/1 Slot Brackets/Bottom Bracket");
        _bracket1 = Object.Instantiate(bracket, bracket.parent.parent);
        _bracket1.SetRotation2D(270f);
        _bracket1.gameObject.SetActive(false);
        _bracket2 = Object.Instantiate(bracket, bracket.parent.parent);
        _bracket2.SetRotation2D(90f);
        _bracket2.gameObject.SetActive(false);
        
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
                Brackets = [..config.Brackets, _bracket1.gameObject, _bracket2.gameObject],
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
    private static void SetAnimator(InventoryFloatingToolSlots __instance) {
        if (__instance.transform.Find("NeedleArt Slot") is not { } artSlot) return;
        if (__instance.transform.Find("Defend Slot") is not { } defendSlot) return;
        
        artSlot.Find("Background Group/Background")
                .GetComponent<Animator>().runtimeAnimatorController =
            defendSlot.Find("Background Group/Background")
                .GetComponent<Animator>().runtimeAnimatorController;
    }

    [HarmonyPatch(typeof(InventoryItemGrid), nameof(InventoryItemGrid.PositionGridItems))]
    [HarmonyPostfix]
    private static void SetNeedleArtSlotPosition(InventoryItemGrid __instance, List<InventoryItemSelectableDirectional> childItems) {
        if (__instance.gameObject.name != "Floating Slots") return;
        foreach (var slot in childItems.Where(slot => slot.name == "NeedleArt Slot")) {
            slot.transform.SetPosition2D(-10f, -3.62f);
        }
        _bracket1.SetPosition2D(-11.26f, -3.62f);
        _bracket2.SetPosition2D(-8.74f, -3.62f);
    }
}

