using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Needleforge;
using Needleforge.Data;
using TeamCherry.Localization;
using UnityEngine;

namespace NeedleArts;

[BepInAutoPlugin(id: "io.github.jadedbay.needlearts")]
[BepInDependency("org.silksong-modding.i18n")]
public partial class NeedleArtsPlugin : BaseUnityPlugin {
    private Harmony harmony { get; } = new(Id);
    internal static ManualLogSource Log;
    
    public static Dictionary<string, NeedleArt> NeedleArts = new() {
        {"HunterArt", new NeedleArt("FINISHED", "Charge Slash Basic")},
        {"ReaperArt", new NeedleArt("REAPER", "Charge Slash Scythe")},
        {"WandererArt", new NeedleArt("WANDERER", "Charge Slash Wanderer")},
        {"BeastArt", new NeedleArt("WARRIOR", "Charge Slash Warrior")}, // NOT WORKING, NOT TO DO WITH FSM
        {"WitchArt", new NeedleArt("WITCH", "Charge Slash Witch")},
        {"ArchitectArt", new NeedleArt("TOOLMASTER", "Charge Slash Toolmaster")},
        {"ShamanArt", new NeedleArt("SHAMAN", "Charge Slash Shaman")},
    };
    
    public static readonly ColorData NeedleArtsToolType = NeedleforgePlugin.AddToolColor(
        "NeedleArts",
        new Color(123.0f / 255.0f, 183.0f / 255.0f, 126.0f / 255.0f)
    ); 
    
    private void Awake() {
        Log = Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
        
        InitializeNeedleArtTools();
    }

    private static void InitializeNeedleArtTools() {
        foreach (var needleArt in NeedleArts) {
           NeedleforgePlugin.AddTool(
               needleArt.Key,
               NeedleArtsToolType.Type,
               new LocalisedString { Key = $"{needleArt.Key}_Tool", Sheet = $"Mods.{Id}" },
               new LocalisedString { Key = $"{needleArt.Key}_ToolDesc", Sheet = $"Mods.{Id}" }
           ); 
        }
    }

    public static void AddNeedleArt(string name, NeedleArt needleArt) {
        NeedleArts.Add(name, needleArt);
    }
    
    public class NeedleArt(string eventName, string chargedSlashName) {
        public readonly string EventName = eventName;
        public readonly string ChargedSlashName = chargedSlashName;
        
        public ToolItem ToolItem;
        public GameObject ChargedSlash;
    }
}