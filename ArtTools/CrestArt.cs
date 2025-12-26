using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using NeedleArts.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace NeedleArts.ArtTools;

public class CrestArt(string name, string eventName, string anticName, string animName, int configId, string? crestDataField, bool useDynamicClipUpdate = false)
    : NeedleArt(name) {
    public string EventName { get; } = eventName;
    public int ConfigId { get; } = configId;
    public string AnticName { get; } = anticName;
    public string AnimName { get; } = animName;
    public string? CrestDataField { get; } = crestDataField;

    public override GameObject GetChargeSlash() {
        return HeroController.instance.configs[ConfigId].ChargeSlash;
    }

    public override HeroControllerConfig GetConfig() {
        return HeroController.instance.configs[ConfigId].Config;
    }

    public override void EditFsm(PlayMakerFSM fsm) {
        var anticType = fsm.GetState("Antic Type");
        
        anticType.AddAction(new CheckIfToolEquipped {
            Tool = new FsmObject { Value = ToolItem },
            trueEvent = FsmEvent.GetFsmEvent(EventName),
            storeValue = false,
        });

        if (useDynamicClipUpdate) {
            fsm.GetState(AnticName).AddActionAtIndex(new UpdateClipName {
                needleArtName = Name,
                animName = AnimName,
            }, 0);
        }
        else {
            fsm.GetState(AnticName).GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()
                .clipName = AnimName;
        }
    }
    
    public override void EditToolItem() {
        PlayerDataTest.TestGroup[] testGroups = [
            ..ToolItem.alternateUnlockedTest.TestGroups,
            new() {
                Tests = [
                    new PlayerDataTest.Test {
                        Type = PlayerDataTest.TestType.Bool,
                        FieldName = "SimpleUnlock",
                        BoolValue = true,
                    },
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

        ToolItem.alternateUnlockedTest = new PlayerDataTest(NeedleArtsPlugin.PlayerDataExt) {
            TestGroups = testGroups,
        };
    }
}