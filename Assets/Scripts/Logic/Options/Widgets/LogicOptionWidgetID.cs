#nullable enable

using PVZEngine;

namespace MVZ2Logic.Options
{
    public static class LogicOptionWidgetNames
    {
        public const string animationFrequency = "animation_frequency";
        public const string particleAmount = "particle_amount";
        public const string screenShake = "screen_shake";
        public const string language = "language";
        public const string blueprintWarnings = "blueprint_warnings";
        public const string bloodAndGore = "blood_and_gore";
        public const string showHotkeys = "show_hotkeys";
        public const string screenLayout = "screen_layout";
        public const string keybinding = "keybinding";
        public const string heightIndicator = "height_indicator";
        public const string hdrLighting = "hdr_lighting";
        public const string hpBarsEnabled = "hp_bars_enabled";
        public const string hpBarsAutoHide = "hp_bars_auto_hide";
        public const string hpBarsAmountMode = "hp_bars_amount_mode";
        public const string hpBarsHoverDisplayRange = "hp_bars_hover_display_range";
        public const string showSponsorNames = "show_sponsor_names";
        public const string autoUpdateSponsors = "auto_update_sponsors";
        public const string lastSponsorSuccessTime = "last_sponsor_success_time";
        public const string lastSponsorAttemptTime = "last_sponsor_attempt_time";
        public const string updateSponsors = "update_sponsors"; 
        public const string commandBlockMode = "command_block_mode";
        public const string skipTalks = "skip_talks";
        public const string vibration = "vibration";
        public const string fullscreen = "fullscreen";
        public const string vSync = "v_sync";
        public const string targetFramerate = "target_framerate";
        public const string resolution = "resolution";
        public const string showFPS = "show_fps";
        public const string credits = "credits";
        public const string exportLogFiles = "export_log_files";
    }

    public static class LogicOptionWidgetID
    {
        public static readonly NamespaceID language = Get(LogicOptionWidgetNames.language);
        public static readonly NamespaceID blueprintWarnings = Get(LogicOptionWidgetNames.blueprintWarnings);
        public static readonly NamespaceID commandBlockMode = Get(LogicOptionWidgetNames.commandBlockMode);
        public static readonly NamespaceID skipTalks = Get(LogicOptionWidgetNames.skipTalks);
        public static readonly NamespaceID vibration = Get(LogicOptionWidgetNames.vibration);
        public static readonly NamespaceID fullscreen = Get(LogicOptionWidgetNames.fullscreen);
        public static readonly NamespaceID resolution = Get(LogicOptionWidgetNames.resolution);
        public static NamespaceID Get(string name)
        {
            return new NamespaceID(Global.BuiltinNamespace, name);
        }
    }
}