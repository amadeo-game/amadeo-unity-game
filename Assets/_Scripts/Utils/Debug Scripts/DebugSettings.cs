public static class DebugSettings
{
    public static bool DebugModeEnabled = false;

    // You can add a method to toggle this based on input, configuration, etc.
    public static void ToggleDebugMode(bool enabled)
    {
        DebugModeEnabled = enabled;
    }
}