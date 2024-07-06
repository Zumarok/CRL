
namespace Crux.CRL.NotificationSystem
{
    /// <summary>
    /// Contains categories of Event Keys.
    /// </summary>

    public static class NotiEvt
    {
        /// <summary>
        /// Contains general combat events.
        /// </summary>
        public static class Combat
        {
            public static readonly string OnGoblinDeath = "^OnGoblinDeath^";
        }

        /// <summary>
        /// Contains UI specific events
        /// </summary>
        public static class UI
        {
            public static readonly string ViewportResolutionChanged = "^ResolutionChange^";
            public static readonly string OnSoloWindowShow = "^OnSoloWindowShow^";
            public static readonly string OnSoloWindowHide = "^OnSoloWindowHide^";
            public static readonly string CloseAllSoloWindows = "^CloseAllSoloWindows^";
        }
    }

}

