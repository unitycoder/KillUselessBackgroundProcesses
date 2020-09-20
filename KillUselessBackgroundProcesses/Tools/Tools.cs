using System.Diagnostics;

namespace KillUselessBackgroundProcesses.Tools
{
    public static class Tools
    {
        static readonly string searchEngineURL = "https://google.com/search?q=";

        public static void SearchOnline(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return;
            Process.Start(searchEngineURL + searchString);
        }
    }
}
