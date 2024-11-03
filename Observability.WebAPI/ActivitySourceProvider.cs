using System.Diagnostics;

namespace Observability.WebAPI;

public class ActivitySourceProvider
{
    public static ActivitySource Source = new("Observability.WebAPI");
}
