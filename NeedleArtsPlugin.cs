using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NeedleArts.ArtTools;
using NeedleArts.Data;
using Needleforge;
using Needleforge.Data;
using Silksong.DataManager;
using TeamCherry.Localization;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using PlayerDataExtension = NeedleArts.Data.PlayerDataExtension;

namespace NeedleArts;

[BepInAutoPlugin(id: "io.github.jadedbay.needlearts")]
[BepInDependency("org.silksong-modding.i18n")]
[BepInDependency("org.silksong-modding.fsmutil")]
[BepInDependency("org.silksong-modding.datamanager")]
public partial class NeedleArtsPlugin : BaseUnityPlugin, IProfileDataMod<ConfigData> {
    internal static NeedleArtsPlugin Instance { get; private set; }
    internal static ManualLogSource Log;
    private Harmony harmony = new(Id);
    
    public ConfigData? ProfileData { get; set; }
    
    public static List<NeedleArt> NeedleArts = [];
    
    public static readonly ColorData NeedleArtsToolType = NeedleforgePlugin.AddToolColor(
        "NeedleArts",
        new Color(0.966f, 0.6f, 0.29f)
    );
   
    // -----Config-----
    public static ConfigEntry<bool> SimpleUnlock;
    
    public static ConfigEntry<bool> UnlockNeedleArts;
    // ----------------

    public static PlayerDataExtension PlayerDataExt;
    
    private void Awake() {
        Instance = this;
        Log = Logger;
        
        Log.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
        
        InitializeConfig();
        
        InitializeNeedleArtTools();
    }

    private void Start() {
        ProfileData ??= new();

        SimpleUnlock.Value = ProfileData.SimpleUnlock;

        PlayerDataExt = new PlayerDataExtension(
            ProfileData
        );
    }
    
    private static void InitializeNeedleArtTools() {
        AddNeedleArt(new CrestArt("HunterArt", "FINISHED", "Antic", "Hunter_Anim", 0, null, true));
        AddNeedleArt(new CrestArt("ReaperArt", "REAPER", "Antic Rpr", "Reaper_Anim", 2, "completedMemory_reaper"));
        AddNeedleArt(new CrestArt("WandererArt", "WANDERER", "Wanderer Antic", "Wanderer_Anim", 4, "completedMemory_wanderer"));
        AddNeedleArt(new CrestArt("BeastArt", "WARRIOR", "Warrior Antic", "Warrior_Anim", 3, "completedMemory_beast"));
        AddNeedleArt(new CrestArt("WitchArt", "WITCH", "Antic", "Whip_Anim", 6, "completedMemory_witch", true));
        AddNeedleArt(new CrestArt("ArchitectArt", "TOOLMASTER", "Antic Drill", "Toolmaster_Anim", 5, "completedMemory_toolmaster"));
        AddNeedleArt(new CrestArt("ShamanArt", "SHAMAN", "Antic", "Shaman_Anim", 7, "completedMemory_shaman", true));
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
    
    private void InitializeConfig() {
        SimpleUnlock = Config.Bind(
            "Gameplay",
            "Simple CrestArt Unlock",
            false,
            "True = Unlock when crest unlocked, False = Unlock when all slots of crest unlocked."
        );

        SimpleUnlock.SettingChanged += (_, _) => {
            ProfileData.SimpleUnlock = SimpleUnlock.Value;
        };
        
        UnlockNeedleArts = Config.Bind(
            "Cheats/Testing",
            "Unlock Needle Arts",
            false,
            "Instantly unlock Needle Strike and all Needle Arts."
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
}