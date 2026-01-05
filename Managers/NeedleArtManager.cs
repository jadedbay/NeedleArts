using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeedleArts.ArtTools;
using NeedleArts.Utils;
using Needleforge;
using Silksong.UnityHelper.Util;
using TeamCherry.Localization;
using UnityEngine;

namespace NeedleArts.Managers;

public class NeedleArtManager {
    public static NeedleArtManager Instance { get; internal set; }
    private readonly List<NeedleArt> NeedleArts = [];

    private NeedleArt? _activeNeedleArt;

    public void AddNeedleArt(NeedleArt needleArt) {
        NeedleArts.Add(needleArt); 
        
        var sprite = SpriteUtil.LoadEmbeddedSprite(Assembly.GetExecutingAssembly(), 
            $"NeedleArts.Resources.Sprites.ToolIcons.{needleArt.Name}Icon.png",
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

        var slotInfo = ToolItemManager.GetCrestByName(PlayerData.instance.CurrentCrestID).Slots;
        var slotData = PlayerData.instance.ToolEquips.GetData(PlayerData.instance.CurrentCrestID).Slots;
        var vestiSlot = PlayerData.instance.ExtraToolEquips.GetData("NeedleArtsSlot");

        _activeNeedleArt = GetNeedleArtByName(vestiSlot.EquippedTool);
        
        var slots = slotInfo.Zip(slotData, (info, data) => (info, data))
            .Where(slot => slot.info.Type == NeedleArtsPlugin.ToolType())
            .ToList();
        
        // check up/down
        _activeNeedleArt = slots
            .Where(s =>
                s.info.AttackBinding == AttackToolBinding.Up && inputHandler.inputActions.Up.IsPressed
                || s.info.AttackBinding == AttackToolBinding.Down && inputHandler.inputActions.Down.IsPressed
            ).Select(s => GetNeedleArtByName(s.data.EquippedTool))
            .FirstOrDefault(art => art is not null) 
           
        // neutral
           ?? GetNeedleArtByName(vestiSlot.EquippedTool)
           
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