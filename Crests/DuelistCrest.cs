using System.Linq;
using Needleforge;
using Needleforge.Attacks;
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
            time: 0.35f, recovery: 0.2f, cooldown: 0.45f,
            quickSpeedMult: 1.5f, quickCooldown: 0.225f
        );

        crest.Moveset.Slash = new Attack {
            Name = "Slash",
            Hitbox = [new(0, 0), new(0, -1), new(-3, -1), new(-3, 0)],
            AnimName = "SlashEffect",
            Color = Color.white,
        };
        
        crest.Moveset.AltSlash = new Attack {
            Name = "SlashAlt",
            Hitbox = [new(0, 0), new(0, -1), new(-3, -1), new(-3, 0)],
            AnimName = "SlashEffectAlt",
            Color = Color.white,
        };

        crest.Moveset.OnInitialized += () => {
            var hc = HeroController.instance;
            
            heroConfig.heroAnimOverrideLib = hc.configs.First(c => c.Config.name == "Toolmaster")
                .Config.heroAnimOverrideLib;

            crest.Moveset.Slash.AnimLibrary = heroConfig.heroAnimOverrideLib;
            crest.Moveset.AltSlash.AnimLibrary = heroConfig.heroAnimOverrideLib;

            foreach (var clip in heroConfig.heroAnimOverrideLib.clips) {
                NeedleArtsPlugin.Log.LogInfo(clip.name);
            }
        };
    }
}