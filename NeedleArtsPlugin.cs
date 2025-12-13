using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Needleforge;
using Needleforge.Data;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeedleArts;

[BepInAutoPlugin(id: "io.github.jadedbay.needlearts")]
[BepInDependency("org.silksong-modding.i18n")]
public partial class NeedleArtsPlugin : BaseUnityPlugin {
    private Harmony harmony = new(Id);
    internal static ManualLogSource Log;
    
    public static Dictionary<string, NeedleArt> NeedleArts = new();
    
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
        AddNeedleArt("HunterArt", "FINISHED", "Charge Slash Basic", "HunterArtIcon");
        AddNeedleArt("ReaperArt", "REAPER", "Charge Slash Scythe", "ReaperArtIcon");
        AddNeedleArt("WandererArt", "WANDERER", "Charge Slash Wanderer", "WandererArtIcon");
        AddNeedleArt("BeastArt", "WARRIOR", "Charge Slash Warrior Old", "BeastArtIcon");
        AddNeedleArt("WitchArt", "WITCH", "Charge Slash Witch", "WitchArtIcon");
        AddNeedleArt("ArchitectArt", "TOOLMASTER", "Charge Slash Toolmaster", "ArchitectArtIcon");
        AddNeedleArt("ShamanArt", "SHAMAN", "Charge Slash Shaman", "ShamanArtIcon");
    }

    public static void AddNeedleArt(string name, string eventName, string chargedSlashName, string textureName) {
        NeedleArts.Add(name, new NeedleArt(eventName, chargedSlashName)); 
       
        var texture = Util.LoadTextureFromAssembly($"NeedleArts.Resources.{textureName}.png");
        var sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, texture.width, texture.height), 
            new Vector2(0.5f, 0.5f), 
            260f
        );
        
       NeedleforgePlugin.AddTool(
           name,
           NeedleArtsToolType.Type,
           new LocalisedString { Key = $"{name}_Tool", Sheet = $"Mods.{Id}" },
           new LocalisedString { Key = $"{name}_ToolDesc", Sheet = $"Mods.{Id}" },
           sprite
       ); 
    }
    
    public class NeedleArt(string eventName, string chargedSlashName) {
        public readonly string EventName = eventName;
        public readonly string ChargedSlashName = chargedSlashName;
        
        public ToolItem ToolItem;
        public GameObject ChargedSlash;
    }
}