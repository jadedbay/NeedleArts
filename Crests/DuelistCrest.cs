using System.Linq;
using System.Reflection;
using HarmonyLib;
using Needleforge;
using Needleforge.Attacks;
using Needleforge.Data;
using Silksong.UnityHelper.Util;
using TeamCherry.Localization;
using UnityEngine;

namespace NeedleArts.Crests;

public static class DuelistCrest {
    public static void InitializeCrest() {
        var crest = NeedleforgePlugin.AddCrest($"DuelistCrest_{NeedleArtsPlugin.Id}",
            new LocalisedString { Key = "DuelistCrest_Name", Sheet = $"Mods.{NeedleArtsPlugin.Id}"},
            new LocalisedString { Key = "DuelistCrest_Desc", Sheet = $"Mods.{NeedleArtsPlugin.Id}"}
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
            Hitbox = [new(0, 0.4f), new(0, -0.6f), new(-4.2f, -0.1f), new(-4.2f, -0.1f)],
            AnimName = "SlashEffect",
            Color = Color.white,
        };
        
        crest.Moveset.AltSlash = new Attack {
            Name = "DuelistSlashAlt",
            Hitbox = [new(0, 0.4f), new(0, -0.6f), new(-4.2f, -0.1f), new(-4.2f, -0.1f)],
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
            
            var asm = Assembly.GetExecutingAssembly();
            var slashEffectSprites = asm.GetManifestResourceNames()
                .Where(name => name.StartsWith("NeedleArts.Resources.Sprites.DuelistCrest."))
                .OrderBy(name => name)
                .Select(name => SpriteUtil.LoadEmbeddedSprite(asm, name, pivot: new Vector2(0.8f, 0.4f)))
                .ToArray();
            
            var slashEffectAnim = Tk2dUtil.CreateTk2dAnimationClip("SlashEffect", fps: 20, slashEffectSprites);
            slashEffectAnim.frames[0].triggerEvent = true;
            slashEffectAnim.frames[^1].triggerEvent = true;
            
            var architectAnimLib = hc.configs.First(c => c.Config.name == "Toolmaster").Config.heroAnimOverrideLib;
            
            animLibrary.clips = [
                slashEffectAnim,
               
                architectAnimLib.GetClipByName("Slash"),
                architectAnimLib.GetClipByName("SlashAlt"),
            ];
            animLibrary.isValid = false;
            animLibrary.ValidateLookup();
            
            crest.Moveset.Slash.AnimLibrary = animLibrary;
            crest.Moveset.AltSlash.AnimLibrary = animLibrary;
            
            heroConfig.heroAnimOverrideLib = animLibrary; 
        };
    }
}