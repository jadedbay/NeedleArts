using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Needleforge;
using Needleforge.Data;
using UnityEngine;

namespace NeedleArts;

[BepInAutoPlugin(id: "io.github.jadedbay.needlearts")]
public partial class NeedleArtsPlugin : BaseUnityPlugin {
    private Harmony harmony { get; } = new(Id);
    internal static ManualLogSource Log;
    
    public static readonly ColorData NeedleArtsToolType = NeedleforgePlugin.AddToolColor(
        "NeedleArts",
        new Color(123.0f / 255.0f, 193.0f / 255.0f, 126.0f / 255.0f),
        true
    ); 
    
    private void Awake() {
        Log = Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();

        NeedleforgePlugin.AddTool($"{Id}_Hunter", NeedleArtsToolType.Type);
    }
}