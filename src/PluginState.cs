using BepInEx.Logging;

namespace RainWorldPlugin;

public sealed class PluginState
{
    public PluginState(ManualLogSource logger)
    {
        Logger = logger;
    }

    public ManualLogSource Logger { get; }
}
