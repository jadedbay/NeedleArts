using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NeedleArts.ArtTools;
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
        new Color(0.966f, 0.6f, 0.29f)
    );
   
    // -----Config-----
    public static ConfigEntry<bool> UnlockNeedleArts;
    // ----------------
    
    private void Awake() {
        Log = Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
        
        InitializeConfig();
        
        InitializeNeedleArtTools();
    }

    private void InitializeConfig() {
        UnlockNeedleArts = Config.Bind(
            "In-Game",
            "Unlock Needle Arts",
            false,
            "Unlock Needle Strike and all Needle Arts."
        );

        UnlockNeedleArts.SettingChanged += (_, _) => {
            if (UnlockNeedleArts.Value) {
                if (PlayerData.instance is { } data) {
                    data.hasChargeSlash = true;
                    ToolItemManagerUtil.AutoEquip("Hunter", GetNeedleArtByName("HunterArt").ToolItem);
                    
                    foreach (var needleArt in NeedleArts) {
                        needleArt.ToolItem.Unlock();
                    }
                }
                
                UnlockNeedleArts.Value = false;
            }
        };
    }

    private static void InitializeNeedleArtTools() {
        AddNeedleArt(new CrestArt("HunterArt", "FINISHED", "Antic", "Hunter_Anim", 0, true));
        AddNeedleArt(new CrestArt("ReaperArt", "REAPER", "Antic Rpr", "Reaper_Anim", 2));
        AddNeedleArt(new CrestArt("WandererArt", "WANDERER", "Wanderer Antic", "Wanderer_Anim", 4));
        AddNeedleArt(new CrestArt("BeastArt", "WARRIOR", "Warrior Antic", "Warrior_Anim", 3));
        AddNeedleArt(new CrestArt("WitchArt", "WITCH", "Antic", "Whip_Anim", 6, true));
        AddNeedleArt(new CrestArt("ArchitectArt", "TOOLMASTER", "Antic Drill", "Toolmaster_Anim", 5));
        AddNeedleArt(new CrestArt("ShamanArt", "SHAMAN", "Antic", "Shaman_Anim", 7, true));
    }
    
    public static void AddNeedleArt(NeedleArt needleArt) {
        NeedleArts.Add(needleArt); 
        
        var texture = Util.LoadTextureFromAssembly($"NeedleArts.Resources.{needleArt.Name}Icon.png");
        var sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, texture.width, texture.height), 
            new Vector2(0.5f, 0.5f), 
            260f
        );
        
        var tool = NeedleforgePlugin.AddTool(
            needleArt.Name,
            NeedleArtsToolType.Type,
            new LocalisedString { Key = $"{needleArt.Name}_Tool", Sheet = $"Mods.{Id}" },
            new LocalisedString { Key = $"{needleArt.Name}_ToolDesc", Sheet = $"Mods.{Id}" },
            sprite
        );

        tool.UnlockedAtStart = false;
    }

    public static NeedleArt? GetNeedleArtByName(string name) {
        return NeedleArts.FirstOrDefault(art => art.Name == name);
    }

    public static NeedleArt? GetEquippedNeedleArt() {
        return NeedleArts.FirstOrDefault(art => art.ToolItem.IsEquipped);
    }

    // TODO: Update this to later work when multiple needle arts equipped
    public static NeedleArt GetCurrentNeedleArt() {
        return GetEquippedNeedleArt();
    }
}