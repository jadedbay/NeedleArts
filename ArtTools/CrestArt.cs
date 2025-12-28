using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.ArtTools;

public class CrestArt(string name, string eventName, string anticName, string animName, int configId, string? crestDataField)
    : NeedleArt(name, animName) {
    public string EventName { get; } = eventName;
    public int ConfigId { get; } = configId;
    public string AnticName { get; } = anticName;
    public string? CrestDataField { get; } = crestDataField;

    public override GameObject GetChargeSlash() {
        return HeroController.instance.configs[ConfigId].ChargeSlash;
    }

    public override HeroControllerConfig GetConfig() {
        return HeroController.instance.configs[ConfigId].Config;
    }

    public override void EditFsm(PlayMakerFSM fsm) {
        fsm.GetState("Antic Type").AddAction(new StringCompare {
            stringVariable = fsm.GetStringVariable("NeedleArtName"),
            compareTo = Name,
            equalEvent = FsmEvent.GetFsmEvent(EventName),
        });

        fsm.GetState(AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
            .clipName = fsm.GetStringVariable("ClipName");
    }

    public void AddSimpleUnlockTest() {
        PlayerDataTest.TestGroup[] testGroups = [
            ..ToolItem.alternateUnlockedTest.TestGroups,
            new() {
                Tests = [
                    ..CrestDataField != null ? new [] {
                        new PlayerDataTest.Test {
                            Type = PlayerDataTest.TestType.Bool,
                            FieldName = CrestDataField,
                            BoolValue = true,
                        }
                    } : [],
                    new PlayerDataTest.Test {
                        Type = PlayerDataTest.TestType.Bool,
                        FieldName = "hasChargeSlash",
                        BoolValue = true,
                    }
                ]
            }
        ];

        ToolItem.alternateUnlockedTest = new PlayerDataTest {
            TestGroups = testGroups
        };
    }

    public void RemoveSimpleUnlockTest() {
        ToolItem.alternateUnlockedTest = new PlayerDataTest();
    }
}