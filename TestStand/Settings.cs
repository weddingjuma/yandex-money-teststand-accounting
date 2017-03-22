using Jupiter.Services.Settings;

namespace TestStand
{
    public static class Settings
    {
        public static string Token
        {
            get { return SettingsService.Local.Get<string>(); }
            set { SettingsService.Local.Set(value); }
        }

        public static bool IsAuthorized
        {
            get { return SettingsService.Local.Get<bool>(); }
            set { SettingsService.Local.Set(value); }
        }

        public static string CurrentSessionBadgeId
        {
            get { return SettingsService.Local.Get<string>(); }
            set { SettingsService.Local.Set(value); }
        }
    }
}
