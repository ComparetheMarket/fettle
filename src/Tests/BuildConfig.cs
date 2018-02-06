namespace Fettle.Tests
{
    internal static class BuildConfig
    {
        public const string AsString =
#if DEBUG
            "Debug";

#else
            "Release";
#endif

    }
}
