namespace RainWorldPlugin;

public sealed class Hooks
{
    private readonly PluginState state;

    public Hooks(PluginState state)
    {
        this.state = state;

        On.RainWorld.Start += RainWorld_Start;
    }

    private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
    {
        orig(self);

        state.Logger.LogInfo("Hello World!");
    }
}
