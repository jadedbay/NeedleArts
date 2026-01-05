using System.Linq;
using Needleforge;
using Needleforge.Attacks;
using Needleforge.Data;
using Silksong.UnityHelper;
using Silksong.UnityHelper.Util;
using TeamCherry.Localization;
using UnityEngine;

namespace NeedleArts.Crests;

public static class DuelistCrest {
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
            Name = "DuelistSlash",
            Hitbox = [new(0, -0.4f), new(0, -0.6f), new(-5, -0.6f), new(-5, -0.4f)],
            AnimName = "SlashEffect",
            Color = Color.white,
        };
        
        crest.Moveset.AltSlash = new Attack {
            Name = "DuelistSlashAlt",
            Hitbox = [new(0, -0.4f), new(0, -0.6f), new(-5, -0.6f), new(-5, -0.4f)],
            AnimName = "SlashEffect",
            Color = Color.white,
        };

        crest.Moveset.OnInitialized += () => {
            if (GameObject.Find("DuelistAnimLib") is { } libObj)
                return;

            var hc = HeroController.instance;
            libObj = new GameObject("DuelistAnimLib");
            Object.DontDestroyOnLoad(libObj);
            
            var animLibrary = libObj.AddComponent<tk2dSpriteAnimation>();

            
            /**
            Sprite[] slashClip = [];
            SpriteUtil.LoadEmbeddedSprite("")
            **/
            
            var hunterDownSlash = hc.animCtrl.animator.Library.GetClipByName("DownSlashEffect");
            hunterDownSlash.name = "SlashEffect";
            
            animLibrary.clips = [
                hunterDownSlash
            ];

            heroConfig.heroAnimOverrideLib = animLibrary; 

            crest.Moveset.Slash.AnimLibrary = heroConfig.heroAnimOverrideLib;
            crest.Moveset.AltSlash.AnimLibrary = heroConfig.heroAnimOverrideLib;

            foreach (var clip in hc.animCtrl.animator.Library.clips) {
                NeedleArtsPlugin.Log.LogInfo(clip.name);
            }
        };
    }
}