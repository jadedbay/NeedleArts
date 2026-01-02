using Needleforge;
using Needleforge.Data;
using TeamCherry.Localization;
using UnityEngine;

namespace NeedleArts.Crests;

public class DuelistCrest {
    public static void InitializeCrest() {
        var crest = NeedleforgePlugin.AddCrest($"DuelistCrest_{NeedleArtsPlugin.Id}",
            new LocalisedString { Key = "DuelistCrest_Name", Sheet = $"Mods.{NeedleforgePlugin.Id}"},
            new LocalisedString { Key = "DuelistCrest_Desc", Sheet = $"Mods.{NeedleforgePlugin.Id}"}
        );

        crest.HudFrame.Preset = VanillaCrest.REAPER;
        
        crest.AddToolSlot(ToolItemType.Skill, AttackToolBinding.Neutral, new Vector2(0.0f, -0.61f), false);
        crest.AddToolSlot(NeedleArtsPlugin.ToolType(), AttackToolBinding.Up, new Vector2(0.0f, 1.16f), false);
        crest.AddToolSlot(NeedleArtsPlugin.ToolType(), AttackToolBinding.Down, new Vector2(0.0f, -2.52f), false);
        crest.ApplyAutoSlotNavigation();
       
        var heroConfig = ScriptableObject.CreateInstance<HeroConfigNeedleforge>();
        crest.Moveset.HeroConfig = heroConfig;

        heroConfig.canBind = true;
        heroConfig.SetCanUseAbilities(true);
        heroConfig.SetAttackFields(
            time: 0.35f, recovery: 0.15f, cooldown: 0.41f,
            quickSpeedMult: 1.5f, quickCooldown: 0.205f
        );
    }
}