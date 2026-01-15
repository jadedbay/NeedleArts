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
    private const float SlotPosX = -0.5f;
    private const float SlotPosY = -3.85f;
    private const float SlotXGap = 1.0f;

    private static GameObject upSlot;
    private static GameObject neutralSlot;
    private static GameObject downSlot;
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Awake))]
    [HarmonyPostfix]
    private static void SpawnSlot(InventoryFloatingToolSlots __instance) {
        upSlot = Object.Instantiate(
            __instance.transform.Find("Defend Slot").gameObject,
            __instance.transform
        );
        upSlot.name = "NeedleArt Slot";
        upSlot.SetActive(false);
        upSlot.transform.localScale = new Vector3(0.6f, 0.6f);

        neutralSlot = Object.Instantiate(upSlot, __instance.transform);
        neutralSlot.name = "NeedleArt Slot";
        downSlot = Object.Instantiate(upSlot, __instance.transform);
        downSlot.name = "NeedleArt Slot";
        
        var upSlotObject = upSlot.GetComponent<InventoryToolCrestSlot>();
        var neutralSlotObject = neutralSlot.GetComponent<InventoryToolCrestSlot>();
        var downSlotObject = downSlot.GetComponent<InventoryToolCrestSlot>();
        
        var cursedArtSlot = Object.Instantiate(
            __instance.transform.Find("Cursed Socket Top").gameObject,
            __instance.transform
        );
        cursedArtSlot.transform.SetLocalPosition2D(SlotPosX, SlotPosY);
        
        var upConfigSlot = new InventoryFloatingToolSlots.Slot {
            SlotObject = upSlotObject,
            Type = NeedleArtsPlugin.ToolType(),
            Id = "NeedleArtsSlot_Up",
            CursedSlot = cursedArtSlot,
        };
        
        var neutralConfigSlot = new InventoryFloatingToolSlots.Slot {
            SlotObject = neutralSlotObject,
            Type = NeedleArtsPlugin.ToolType(),
            Id = "NeedleArtsSlot_Neutral",
            CursedSlot = cursedArtSlot,
        };
        
        var downConfigSlot = new InventoryFloatingToolSlots.Slot {
            SlotObject = downSlotObject,
            Type = NeedleArtsPlugin.ToolType(),
            Id = "NeedleArtsSlot_Down",
            CursedSlot = cursedArtSlot,
        };

        var test = new PlayerDataTest.Test {
            FieldName = "hasChargeSlash",
            Type = PlayerDataTest.TestType.Bool,
            BoolValue = true
        };
        
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
                Slots = [..config.Slots, upConfigSlot, neutralConfigSlot, downConfigSlot],
                Brackets = [..config.Brackets],
                PositionOffset = config.PositionOffset,
                Condition = condition,
            });
        }

        var baseConfig = new InventoryFloatingToolSlots.Config {
            Slots = [upConfigSlot, neutralConfigSlot, downConfigSlot],
            Brackets = [],
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
    private static void SetSlotValues(InventoryFloatingToolSlots __instance) {
        if (neutralSlot == null) return;

        var attackSlots = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.name == "Attack Slot(Clone)" && go.transform.parent.name == "Hunter")
            .ToList();
        
        SetAnimator(attackSlots, upSlot, AttackToolBinding.Up);
        upSlot.GetComponent<InventoryToolCrestSlot>().slotInfo.AttackBinding = AttackToolBinding.Up;
        SetAnimator(attackSlots, downSlot, AttackToolBinding.Down);
        upSlot.GetComponent<InventoryToolCrestSlot>().slotInfo.AttackBinding = AttackToolBinding.Down;
        
        neutralSlot.transform.Find("Background Group/Background").GetComponent<Animator>().runtimeAnimatorController =
            __instance.transform.Find("Defend Slot/Background Group/Background").GetComponent<Animator>()
                .runtimeAnimatorController;
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
            slot.transform.SetLocalPosition2D(SlotPosX + SlotXGap * index, SlotPosY + yOffset);
            index++;
        }
    }
}

