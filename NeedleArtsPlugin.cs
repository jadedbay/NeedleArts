using System.Collections.Generic;
using System.Linq;
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
    private Harmony harmony = new(Id);
    internal static ManualLogSource Log;
    
    public static List<NeedleArt> NeedleArts = [];
    
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
        AddNeedleArt("HunterArt", "FINISHED", "Antic", "Hunter_Anim", 0);
        AddNeedleArt("ReaperArt", "REAPER", "Antic Rpr", "Reaper_Anim", 2);
        AddNeedleArt("WandererArt", "WANDERER", "Wanderer Antic", "Wanderer_Anim", 4);
        AddNeedleArt("BeastArt", "WARRIOR", "Warrior Antic", "Warrior_Anim", 3);
        AddNeedleArt("WitchArt", "WITCH", "Antic", "Whip_Anim", 6);
        AddNeedleArt("ArchitectArt", "TOOLMASTER", "Antic Drill", "Toolmaster_Anim", 5);
        AddNeedleArt("ShamanArt", "SHAMAN", "Antic", "Shaman_Anim", 7);
    }

    public static void AddNeedleArt(string name, string eventName, string anticName, string animName, int configId) {
        NeedleArts.Add(new NeedleArt(name, eventName, anticName, animName, configId)); 
       
        var texture = Util.LoadTextureFromAssembly($"NeedleArts.Resources.{name}Icon.png");
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

    public static NeedleArt? GetNeedleArtByName(string name) {
        return NeedleArts.FirstOrDefault(art => art.Name == name);
    }
    
    public class NeedleArt(string name, string eventName, string anticName, string animName, int configId) {
        public readonly string Name = name; 
        
        public readonly string EventName = eventName;
        public readonly string AnticName = anticName;
        public readonly string AnimName = animName;
        public readonly int ConfigId = configId;
        
        public ToolItem ToolItem;
    }
}