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
[BepInDependency("org.silksong-modding.fsmutil")]
public partial class NeedleArtsPlugin : BaseUnityPlugin {
    private Harmony harmony = new(Id);
    internal static ManualLogSource Log;
    
    public static List<NeedleArt> NeedleArts = [];
    
    public static readonly ColorData NeedleArtsToolType = NeedleforgePlugin.AddToolColor(
        "NeedleArts",
        new Color(0.966f, 0.6f, 0.29f),
        true
    );
   
    // -----Config-----
    public static ConfigEntry<bool> SimpleUnlock;
    
    public static ConfigEntry<bool> UnlockNeedleArts;
    // ----------------
    
    private void Awake() {
        Log = Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
        
        InitializeConfig();
        
        InitializeNeedleArtTools();
        InitializeCrest();
    }
    
    private static void InitializeNeedleArtTools() {
        AddNeedleArt(new CrestArt("HunterArt", "FINISHED", "Antic", "Hunter_Anim", 0, null));
        AddNeedleArt(new CrestArt("ReaperArt", "REAPER", "Antic Rpr", "Reaper_Anim", 2, "completedMemory_reaper"));
        AddNeedleArt(new CrestArt("WandererArt", "WANDERER", "Wanderer Antic", "Wanderer_Anim", 4, "completedMemory_wanderer"));
        AddNeedleArt(new CrestArt("BeastArt", "WARRIOR", "Warrior Antic", "Warrior_Anim", 3, "completedMemory_beast"));
        AddNeedleArt(new CrestArt("WitchArt", "WITCH", "Antic", "Whip_Anim", 6, "completedMemory_witch"));
        AddNeedleArt(new CrestArt("ArchitectArt", "TOOLMASTER", "Antic Drill", "Toolmaster_Anim", 5, "completedMemory_toolmaster"));
        AddNeedleArt(new CrestArt("ShamanArt", "SHAMAN", "Antic", "Shaman_Anim", 7, "completedMemory_shaman"));
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

    public static NeedleArt? GetSelectedNeedleArt() {
        var inputHandler = HeroController.instance.inputHandler;
        
        var toolItem =
            ToolItemManager.GetBoundAttackTool(AttackToolBinding.Neutral, ToolEquippedReadSource.Active);
        
        if (inputHandler.inputActions.Up.IsPressed) {
            var dirToolItem = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Up, ToolEquippedReadSource.Active);
            if (dirToolItem != null) toolItem = dirToolItem;
        } else if (inputHandler.inputActions.Down.IsPressed) {
            var dirToolItem = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Down, ToolEquippedReadSource.Active);
            if (dirToolItem != null) toolItem = dirToolItem;
        }
        
        return GetNeedleArtByName(toolItem.name);
    }
    
    private void InitializeConfig() {
        SimpleUnlock = Config.Bind(
            "Gameplay",
            "Simple CrestArt Unlock",
            false,
            "True = Unlock when crest unlocked, False = Unlock when all slots of crest unlocked."
        );

        SimpleUnlock.SettingChanged += (_, _) => {
            if (PlayerData.instance == null) return;
            
            if (SimpleUnlock.Value) {
                foreach (var crestArt in NeedleArts.OfType<CrestArt>()) {
                    crestArt.AddSimpleUnlockTest();
                }
            } else {
                foreach (var crestArt in NeedleArts.OfType<CrestArt>()) {
                    crestArt.RemoveSimpleUnlockTest();
                }
            }
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

    private static void InitializeCrest() {
        var crest = NeedleforgePlugin.AddCrest($"PinmasterCrest_{Id}",
            new LocalisedString { Key = "PinmasterCrest_Name", Sheet = $"Mods.{Id}"},
            new LocalisedString { Key = "PinmasterCrest_Desc", Sheet = $"Mods.{Id}"}
        );
        
        crest.AddToolSlot(NeedleArtsToolType.Type, AttackToolBinding.Neutral, new Vector2(0.0f, -0.61f), false);
        crest.AddToolSlot(NeedleArtsToolType.Type, AttackToolBinding.Up, new Vector2(0.0f, 1.16f), false);
        crest.AddToolSlot(NeedleArtsToolType.Type, AttackToolBinding.Down, new Vector2(0.0f, -2.52f), false);
        
        crest.ApplyAutoSlotNavigation();
    }
}