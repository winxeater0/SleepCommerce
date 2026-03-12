using System.Diagnostics;

namespace SleepCommerce.Application.Diagnostics;

public static class ActivitySources
{
    public const string Name = "SleepCommerce.Application";
    public static readonly ActivitySource Default = new(Name);
}
