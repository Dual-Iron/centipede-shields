using BepInEx;

namespace RainWorldPlugin;

[BepInPlugin("org.author.rainworldplugin", nameof(RainWorldPlugin), "0.1.0")]
public sealed class Plugin : BaseUnityPlugin
{
    // Entry point for the plugin.
    public void OnEnable()
    {
        PluginState state = new(Logger);

        _ = new Hooks(state);
    }
}
