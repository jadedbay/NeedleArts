using System.Collections.Generic;
using System.Linq;
using GlobalSettings;
using HarmonyLib;
using NeedleArts.Managers;
using NeedleArts.Utils;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class AddSlot {
    private const float slotPosX = 0.4f;
    private const float slotPosY = -3.45f;
    private const float bracketOffset = 0.87f;
    private const float slotYGap = -0.7f;
    
    private static Transform _bracket1;
    private static Transform _bracket2;

    private static GameObject neutralSlot;
    private static GameObject upSlot;
    private static GameObject downSlot;
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Awake))]
    [HarmonyPostfix]
    private static void SpawnSlot(InventoryFloatingToolSlots __instance) {
        neutralSlot = Object.Instantiate(
            __instance.transform.Find("Defend Slot").gameObject,
            __instance.transform
        );
        neutralSlot.name = "NeedleArt Slot";
        neutralSlot.SetActive(false);
        neutralSlot.transform.localScale = new Vector3(0.6f, 0.6f);

        upSlot = Object.Instantiate(neutralSlot, __instance.transform);
        upSlot.name = "NeedleArt Slot";
        downSlot = Object.Instantiate(neutralSlot, __instance.transform);
        downSlot.name = "NeedleArt Slot";
        
        var neutralSlotObject = neutralSlot.GetComponent<InventoryToolCrestSlot>();
        var upSlotObject = upSlot.GetComponent<InventoryToolCrestSlot>();
        var downSlotObject = downSlot.GetComponent<InventoryToolCrestSlot>();
        
        var cursedArtSlot = Object.Instantiate(
            __instance.transform.Find("Cursed Socket Top").gameObject,
            __instance.transform
        );
        
        cursedArtSlot.transform.SetLocalPosition2D(slotPosX, slotPosY);
        var neutralConfigSlot = new InventoryFloatingToolSlots.Slot {
            SlotObject = neutralSlotObject,
            Type = NeedleArtsPlugin.ToolType(),
            Id = "NeedleArtsSlot",
            CursedSlot = cursedArtSlot,
        };
        
        var upConfigSlot = new InventoryFloatingToolSlots.Slot {
            SlotObject = upSlotObject,
            Type = NeedleArtsPlugin.ToolType(),
            Id = "NeedleArtsSlot",
            CursedSlot = cursedArtSlot,
        };
        
        var downConfigSlot = new InventoryFloatingToolSlots.Slot {
            SlotObject = downSlotObject,
            Type = NeedleArtsPlugin.ToolType(),
            Id = "NeedleArtsSlot",
            CursedSlot = cursedArtSlot,
        };

        var test = new PlayerDataTest.Test {
            FieldName = "hasChargeSlash",
            Type = PlayerDataTest.TestType.Bool,
            BoolValue = true
        };
        
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
                        Tests = [..config.Condition.TestGroups[0].Tests, test]
                    }
                ]
             };
            
            needleArtConfigs.Add(new InventoryFloatingToolSlots.Config {
                Slots = [..config.Slots, neutralConfigSlot, upConfigSlot, downConfigSlot],
                Brackets = [..config.Brackets, _bracket1.gameObject, _bracket2.gameObject],
                PositionOffset = config.PositionOffset,
                Condition = condition,
                
            });
        }

        var baseConfig = new InventoryFloatingToolSlots.Config {
            Slots = [neutralConfigSlot, upConfigSlot, downConfigSlot],
            Brackets = [_bracket1.gameObject, _bracket2.gameObject],
            PositionOffset = new Vector2(0.0f, 0.0f),
            Condition = new PlayerDataTest {
                TestGroups = [
                    new PlayerDataTest.TestGroup {
                        Tests = [test]
                    }
                ]
            }
        };

        __instance.configs = [baseConfig, ..__instance.configs, ..needleArtConfigs];
        
        var data = PlayerData.instance.ExtraToolEquips.GetData("NeedleArtsSlot");
        if (string.IsNullOrEmpty(data.EquippedTool)) {
            PlayerData.instance.ExtraToolEquips.SetData("NeedleArtsSlot", new ToolCrestsData.SlotData());
        }
    }
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Evaluate))]
    [HarmonyPostfix]
    private static void RepositionVesticrest(InventoryFloatingToolSlots __instance) {
        var origPos = __instance.transform.localPosition;
        __instance.transform.localPosition = new Vector3(origPos.x, origPos.y + 2.0f, origPos.z);
    }
    
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Evaluate))]
    [HarmonyPostfix]
    private static void SetSlotValues(InventoryFloatingToolSlots __instance) {
        if (neutralSlot == null) return;

        var attackSlots = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.name == "Attack Slot(Clone)" && go.transform.parent.name == "Hunter")
            .ToList();
        
        neutralSlot.transform.Find("Background Group/Background").GetComponent<Animator>().runtimeAnimatorController =
            __instance.transform.Find("Defend Slot/Background Group/Background").GetComponent<Animator>()
                .runtimeAnimatorController;
        
        SetAnimator(attackSlots, upSlot, AttackToolBinding.Up);
        SetAnimator(attackSlots, downSlot, AttackToolBinding.Down);
    }

    private static void SetAnimator(List<GameObject> attackSlots, GameObject artSlot, AttackToolBinding attackBinding) {
        var attackSlot = attackSlots.First(s =>
            s.GetComponent<InventoryToolCrestSlot>().slotInfo.AttackBinding == attackBinding);
        
        artSlot.transform.Find("Background Group/Background").GetComponent<Animator>().runtimeAnimatorController =
            attackSlot.transform.Find("Background Group/Background").GetComponent<Animator>()
                .runtimeAnimatorController;
    }

    [HarmonyPatch(typeof(InventoryItemGrid), nameof(InventoryItemGrid.PositionGridItems))]
    [HarmonyPostfix]
    private static void SetNeedleArtSlotPosition(InventoryItemGrid __instance, List<InventoryItemSelectableDirectional> childItems) {
        if (__instance.gameObject.name != "Floating Slots") return;
        var data = PlayerData.instance;
        var yOffset = data.UnlockedExtraYellowSlot && !data.UnlockedExtraBlueSlot ? 1.73f : 0.0f;

        var index = 0;
        foreach (var slot in childItems.Where(slot => slot.name == "NeedleArt Slot")) {
            slot.transform.SetLocalPosition2D(slotPosX, slotPosY + yOffset + slotYGap * index);
            index++;
        }
        _bracket1.SetLocalPosition2D(slotPosX - bracketOffset, slotPosY + yOffset);
        _bracket2.SetLocalPosition2D(slotPosX + bracketOffset, slotPosY + yOffset);
    }
}

