using System.Collections.Generic;
using System.Linq;

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
    
    public static IEnumerable<(string CrestName, string ArtName)> GetAllPairs() {
        return CrestToArt.Select(kvp => (kvp.Key, kvp.Value));
    }

    public static bool IsValidVanillaCrest(string crestName) {
        return GetArtName(crestName) != null ? true : false;
    }
}
