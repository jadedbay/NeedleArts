using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NeedleArts.Patches;

[HarmonyPatch]
public class AddSlot {
    private const float SlotPosX = -0.5f;
    private const float SlotPosY = -3.85f;
    private const float SlotXGap = 1.0f;

    private static List<NeedleArtSlot> slots;
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Awake))]
    [HarmonyPostfix]
    private static void SpawnSlot(InventoryFloatingToolSlots __instance) {
        slots = [
            new NeedleArtSlot(__instance, AttackToolBinding.Up),
            new NeedleArtSlot(__instance, AttackToolBinding.Neutral),
            new NeedleArtSlot(__instance, AttackToolBinding.Down)
        ];
        
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
                Slots = [..config.Slots, ..slots.Select(s => s.slot)],
                Brackets = [..config.Brackets],
                PositionOffset = config.PositionOffset,
                Condition = condition,
            });
        }

        var baseConfig = new InventoryFloatingToolSlots.Config {
            Slots = [..slots.Select(s => s.slot)],
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
    }
    
    [HarmonyPatch(typeof(InventoryFloatingToolSlots), nameof(InventoryFloatingToolSlots.Evaluate))]
    [HarmonyPostfix]
    private static void SetSlotValues(InventoryFloatingToolSlots __instance) {
        if (slots.IsNullOrEmpty() || slots[0].gameObject == null) return;

        var attackSlots = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.name == "Attack Slot(Clone)" && go.transform.parent.name == "Hunter")
            .ToList();
        attackSlots.Add(__instance.transform.Find("Defend Slot").gameObject);

        foreach (var slot in slots) slot.Evaluate(attackSlots);
    }

    [HarmonyPatch(typeof(InventoryItemGrid), nameof(InventoryItemGrid.PositionGridItems))]
    [HarmonyPostfix]
    private static void SetNeedleArtSlotPositions(InventoryItemGrid __instance, List<InventoryItemSelectableDirectional> childItems) {
        if (__instance.gameObject.name != "Floating Slots") return;
        var data = PlayerData.instance;
        var yOffset = data.UnlockedExtraYellowSlot && !data.UnlockedExtraBlueSlot ? 1.73f : 0.0f;

        var index = 0;
        foreach (var slot in childItems.Where(slot => slot.name == "NeedleArt Slot")) {
            slot.transform.SetLocalPosition2D(SlotPosX + SlotXGap * index, SlotPosY + yOffset);
            index++;
        }
    }
    
    [HarmonyPatch(typeof(InventoryItemGrid), nameof(InventoryItemGrid.LinkGridSelectables))]
    [HarmonyPostfix]
    private static void FixNav(InventoryItemGrid __instance, List<InventoryItemSelectableDirectional> childItems)
    {
        if (__instance.gameObject.name != "Floating Slots") return;
    
        var needleArtSlots = childItems.Where(slot => slot.name == "NeedleArt Slot").ToList();
        if (needleArtSlots.Count == 0) return;

        var upSelectable = needleArtSlots[0].Selectables[0];

        for (var i = 0; i < needleArtSlots.Count; i++) {
            var slot = needleArtSlots[i];
            
            slot.Selectables[0] = upSelectable;
            slot.Selectables[1] = null;

            slot.Selectables[2] = i > 0 ? needleArtSlots[i - 1] : null;
            slot.Selectables[3] = i < needleArtSlots.Count - 1 ? needleArtSlots[i + 1] : null;
        }
    }

    [HarmonyPatch(typeof(InventoryItemSelectableDirectional), nameof(InventoryItemSelectableDirectional.GetNextSelectable), 
        typeof(InventoryItemManager.SelectionDirection), typeof(bool))]
    [HarmonyPrefix]
    private static void UseParentNav(
        InventoryItemSelectableDirectional __instance, 
        InventoryItemManager.SelectionDirection direction, 
        ref bool allowAutoNavOnFirst
    ) {
        if (__instance.name != "NeedleArt Slot") return;
        if (__instance.Selectables[(int)direction] != null) return;
        
        allowAutoNavOnFirst = false;
    }
}

internal class NeedleArtSlot {
    public GameObject gameObject;
    public InventoryFloatingToolSlots.Slot slot;
    public AttackToolBinding binding;
    
    public NeedleArtSlot(InventoryFloatingToolSlots inventoryFloatingToolSlots, AttackToolBinding binding) {
        this.binding = binding;
        
        gameObject = Object.Instantiate(
            inventoryFloatingToolSlots.transform.Find("Defend Slot").gameObject,
            inventoryFloatingToolSlots.transform
        );
        gameObject.name = "NeedleArt Slot";
        gameObject.SetActive(false);
        gameObject.transform.localScale = new Vector3(0.6f, 0.6f);
        
        slot = new InventoryFloatingToolSlots.Slot {
            SlotObject = gameObject.GetComponent<InventoryToolCrestSlot>(),
            Type = NeedleArtsPlugin.ToolType(),
            Id = $"NeedleArtsSlot_{binding.ToString()}",
            CursedSlot = Object.Instantiate(
                inventoryFloatingToolSlots.transform.Find("Cursed Socket Top").gameObject,
                inventoryFloatingToolSlots.transform
            )
        };
    }

    public void Evaluate(List<GameObject> attackSlots) {
        gameObject.GetComponent<InventoryToolCrestSlot>().slotInfo.AttackBinding = binding;
        var attackSlot = attackSlots.First(s => s.GetComponent<InventoryToolCrestSlot>().slotInfo.AttackBinding == binding);
        
        gameObject.transform.Find("Background Group/Background").GetComponent<Animator>().runtimeAnimatorController =
            attackSlot.transform.Find("Background Group/Background").GetComponent<Animator>()
                .runtimeAnimatorController;
    }
}