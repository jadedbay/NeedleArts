using System.Collections.Generic;
using System.Linq;
using NeedleArts.ArtTools;
using NeedleArts.Managers;

namespace NeedleArts;

public static class CrestArtUtil {
    private static readonly Dictionary<string, string> CrestToArt = new() {
        { "Hunter", "HunterArt" },
        { "Reaper", "ReaperArt" },
        { "Wanderer", "WandererArt" },
        { "Warrior", "BeastArt" },
        { "Witch", "WitchArt" },
        { "Toolmaster", "ArchitectArt" },
        { "Spell", "ShamanArt" }
    };

    private static readonly Dictionary<string, string> ArtToCrest = 
        CrestToArt.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static string GetArtName(string crestName) {
        crestName = crestName.Contains("Hunter") ? "Hunter" : crestName;
        return CrestToArt.TryGetValue(crestName, out var art) ? art : null;
    }

    public static string GetCrestName(string artName) {
        return ArtToCrest.TryGetValue(artName, out var crest) ? crest : null;
    }

    public static NeedleArt GetCrestArt() {
        return NeedleArtManager.Instance.GetNeedleArtByName(GetArtName(PlayerData.instance.CurrentCrestID));
    }
}
