using MudBlazor;

namespace SyncArea.Misc
{
    public static class AppliationSettings
    {
        public static DialogOptions DialogOptions { get; set; } = new DialogOptions
        {
            NoHeader = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Medium
        };
    }
}
