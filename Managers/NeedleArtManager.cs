using System.Collections.Generic;
using System.Linq;
using NeedleArts.ArtTools;
using NeedleArts.Utils;
using Needleforge;
using TeamCherry.Localization;
using Unity.Baselib.LowLevel;
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
            NeedleArtsPlugin.ToolType(),
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
        
        var data = PlayerData.instance;
        List<(ToolCrestsData.SlotData data, AttackToolBinding binding)> slots = [
            (data.ExtraToolEquips.GetData("NeedleArtsSlot_Up"), AttackToolBinding.Up),
            (data.ExtraToolEquips.GetData("NeedleArtsSlot_Neutral"), AttackToolBinding.Neutral),
            (data.ExtraToolEquips.GetData("NeedleArtsSlot_Down"), AttackToolBinding.Down),
        ];
        
        // check up/down
        _activeNeedleArt = slots
            .Where(s =>
                s.binding == AttackToolBinding.Up && inputHandler.inputActions.Up.IsPressed
                || s.binding == AttackToolBinding.Down && inputHandler.inputActions.Down.IsPressed
            ).Select(s => GetNeedleArtByName(s.data.EquippedTool))
            .FirstOrDefault(art => art is not null) 
           
        // neutral
           ?? GetNeedleArtByName(slots.Single(s => s.binding == AttackToolBinding.Neutral).data.EquippedTool)
           
        // fallback to any equipped
           ?? slots
               .Select(slot => GetNeedleArtByName(slot.data.EquippedTool))
               .FirstOrDefault(art => art is not null);
    }

    public NeedleArt? GetActiveNeedleArt() {
        return _activeNeedleArt;
    }

    public void ResetActiveNeedleArt() {
        _activeNeedleArt = null;
    }
    
    public static void AutoEquipArt(ToolItem needleArt) {
        var manager = Resources.FindObjectsOfTypeAll<InventoryItemToolManager>()
            .FirstOrDefault(m => m.gameObject.scene.IsValid());

        foreach (var floatingSlot in manager.extraSlots.GetSlots()) {
            if (floatingSlot.Type != NeedleArtsPlugin.ToolType()) continue;
            floatingSlot.SetEquipped(needleArt, isManual: true, refreshTools: true);
        }
    }
}