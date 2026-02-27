
    namespace Presentation
    {
        [System.Flags]
        public enum TopBarFeature
        {
            None      = 0,
            Coin      = 1 << 0,
            Heart     = 1 << 1,
            Invite    = 1 << 2,
            Sns       = 1 << 3,
            Settings  = 1 << 4,
        }
    }
