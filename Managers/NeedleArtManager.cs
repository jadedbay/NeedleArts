using System.Collections.Generic;
using System.Linq;
using NeedleArts.ArtTools;
using Needleforge;
using TeamCherry.Localization;
using UnityEngine;

namespace NeedleArts.Managers;

public class NeedleArtManager {
    public static NeedleArtManager Instance { get; internal set; }
    private readonly List<NeedleArt> NeedleArts = [];

    private NeedleArt? _activeNeedleArt;

    public void AddNeedleArt(NeedleArt needleArt) {
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
            NeedleArtsPlugin.NeedleArtsToolType.Type,
            new LocalisedString { Key = $"{needleArt.Name}_Tool", Sheet = $"Mods.{NeedleArtsPlugin.Id}" },
            new LocalisedString { Key = $"{needleArt.Name}_ToolDesc", Sheet = $"Mods.{NeedleArtsPlugin.Id}" },
            sprite
        );

        tool.UnlockedAtStart = false;
    }

    public List<NeedleArt> GetAllNeedleArts() {
        return NeedleArts;
    }

    public NeedleArt? GetNeedleArtByName(string name) {
        return NeedleArts.FirstOrDefault(art => art.Name == name);
    }

    public void SetActiveNeedleArt() {
        var inputHandler = HeroController.instance.inputHandler;

        var slots = ToolItemManager.GetCrestByName(PlayerData.instance.CurrentCrestID).Slots;
        var slotData = PlayerData.instance.ToolEquips.GetData(PlayerData.instance.CurrentCrestID).Slots;

        NeedleArt? needleArt = null;
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i].Type != NeedleArtsPlugin.NeedleArtsToolType.Type) continue;

            switch (slots[i].AttackBinding) {
                case AttackToolBinding.Up: {
                    if (inputHandler.inputActions.Up.IsPressed)
                        needleArt ??= GetNeedleArtByName(slotData[i].EquippedTool);
                    break;
                }
                case AttackToolBinding.Down: {
                    if (inputHandler.inputActions.Down.IsPressed)
                        needleArt ??= GetNeedleArtByName(slotData[i].EquippedTool);
                    break;
                }
                case AttackToolBinding.Neutral:
                default:
                    needleArt ??= GetNeedleArtByName(slotData[i].EquippedTool);
                    break;
            }

            if (needleArt != null) break;
        }

        _activeNeedleArt = needleArt;
    }

    public NeedleArt? GetActiveNeedleArt() {
        return _activeNeedleArt;
    }

    public void ResetActiveNeedleArt() {
        _activeNeedleArt = null;
    }
}