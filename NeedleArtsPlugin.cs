using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NeedleArts.ArtTools;
using NeedleArts.Data;
using NeedleArts.Managers;
using Needleforge;
using Needleforge.Data;
using Silksong.DataManager;
using UnityEngine;
using PlayerDataExtension = NeedleArts.Data.PlayerDataExtension;

namespace NeedleArts;

[BepInAutoPlugin(id: "io.github.jadedbay.needlearts")]
[BepInDependency("org.silksong-modding.i18n")]
[BepInDependency("org.silksong-modding.fsmutil")]
[BepInDependency("org.silksong-modding.datamanager")]
public partial class NeedleArtsPlugin : BaseUnityPlugin, IProfileDataMod<ConfigData> {
    internal static ManualLogSource Log;
    private Harmony harmony = new(Id);
    
    public ConfigData? ProfileData { get; set; }
    
    public NeedleArtManager NeedleArtManager { get; private set; }

    public static readonly ColorData NeedleArtsToolType = NeedleforgePlugin.AddToolColor(
        "NeedleArts",
        new Color(0.966f, 0.6f, 0.29f),
        true
    );
   
    // -----Config-----
    public static ConfigEntry<bool> SimpleUnlock;
    
    public static ConfigEntry<bool> UnlockNeedleArts;
    // ----------------

    public static PlayerDataExtension PlayerDataExt;
    
    private void Awake() {
        NeedleArtManager = new NeedleArtManager();
        NeedleArtManager.Instance = NeedleArtManager;
        
        Log = Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
        
        InitializeConfig();
        
        InitializeNeedleArtTools();
    }

    private void OnDestroy() {
        NeedleArtManager.Instance = null;
    }

    private void Start() {
        ProfileData ??= new();

        SimpleUnlock.Value = ProfileData.SimpleUnlock;

        PlayerDataExt = new PlayerDataExtension(
            ProfileData
        );
    }
    
    private static void InitializeNeedleArtTools() {
        var manager = NeedleArtManager.Instance;
        
        manager.AddNeedleArt(new CrestArt("HunterArt", "FINISHED", "Antic", "Hunter_Anim", 0, null));
        manager.AddNeedleArt(new CrestArt("ReaperArt", "REAPER", "Antic Rpr", "Reaper_Anim", 2, "completedMemory_reaper"));
        manager.AddNeedleArt(new CrestArt("WandererArt", "WANDERER", "Wanderer Antic", "Wanderer_Anim", 4, "completedMemory_wanderer"));
        manager.AddNeedleArt(new CrestArt("BeastArt", "WARRIOR", "Warrior Antic", "Warrior_Anim", 3, "completedMemory_beast"));
        manager.AddNeedleArt(new CrestArt("WitchArt", "WITCH", "Antic", "Whip_Anim", 6, "completedMemory_witch"));
        manager.AddNeedleArt(new CrestArt("ArchitectArt", "TOOLMASTER", "Antic Drill", "Toolmaster_Anim", 5, "completedMemory_toolmaster"));
        manager.AddNeedleArt(new CrestArt("ShamanArt", "SHAMAN", "Antic", "Shaman_Anim", 7, "completedMemory_shaman"));
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
                    ToolItemManagerUtil.AutoEquip("Hunter", NeedleArtManager.GetNeedleArtByName("HunterArt").ToolItem);
                    
                    foreach (var needleArt in NeedleArtManager.GetAllNeedleArts()) {
                        needleArt.ToolItem.Unlock();
                    }
                }
                
                UnlockNeedleArts.Value = false;
            }
        };
    }
}