using System.Diagnostics;

namespace Observability.ConsoleApp;
internal static class ActivitySourceProvider
{
    public static ActivitySource Source = new(OpenTelemtryConstants.ActivitySourceName);
    public static ActivitySource SourceFile = new(OpenTelemtryConstants.ActivitySourceFileName);
}
