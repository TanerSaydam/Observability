using System.Diagnostics;

namespace Observability.Orders.WebAPI;

public static class ActivitySourceProvider
{
    public static ActivitySource Source = new("Orders.WebAPI", "1.0.0");
}
