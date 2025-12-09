using BepInEx;

namespace NeedleArts;

// TODO - adjust the plugin guid as needed
[BepInAutoPlugin(id: "io.github.jadedbay.needlearts")]
public partial class NeedleArtsPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Put your initialization logic here
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
