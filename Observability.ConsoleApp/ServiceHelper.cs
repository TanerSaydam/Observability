namespace Observability.ConsoleApp;
internal sealed class ServiceHelper
{
    public async Task Work1()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity();
        Console.WriteLine("Work1 tamamlandı");

        var serviceOne = new ServiceOne();

        //throw new Exception("Something went wrong");        

        var result = await serviceOne.MakeRequestToGoogle();
        Console.WriteLine($"Length: {result}");
    }

    public async Task Work2()
    {
        using var actity = ActivitySourceProvider.SourceFile.StartActivity();
        Console.WriteLine("Work2 is complete");
        actity.SetTag("work 2 tag", "work 2 tag value");
        actity.AddEvent(new System.Diagnostics.ActivityEvent("work 2 event"));
    }
}