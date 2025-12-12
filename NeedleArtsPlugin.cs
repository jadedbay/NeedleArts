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

    public static Dictionary<string, NeedleArt> needleArts = new();
    
    public static Dictionary<string, ToolItem> needleArtToolItems = new();

    public static readonly string[] ChargeSlashNames = new[] {
        "Charge Slash Basic",
        "Charge Slash Scythe",
        "Charge Slash Wanderer"
    };
    
    public static readonly ColorData NeedleArtsToolType = NeedleforgePlugin.AddToolColor(
        "NeedleArts",
        new Color(123.0f / 255.0f, 193.0f / 255.0f, 126.0f / 255.0f)
    ); 
    
    private void Awake() {
        Log = Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
        
        InitializeNeedleArtTools();
    }

    private static void InitializeNeedleArtTools() {
        foreach (var slashName in ChargeSlashNames) {
           var tool = NeedleforgePlugin.AddTool(
               slashName,
               NeedleArtsToolType.Type,
               new LocalisedString { Key = $"{slashName}_Tool", Sheet = $"Mods.{Id}" },
               new LocalisedString { Key = $"{slashName}_ToolDesc", Sheet = $"Mods.{Id}" }
           ); 
           
           needleArts.Add(slashName, new NeedleArt { Tool = tool });
        }
        
        
    }
    
    public class NeedleArt {
        public ToolData Tool { get; set; }
        public GameObject ChargeSlash { get; set; }
    }
}