using System.Diagnostics;

namespace Observability.ConsoleApp;
internal sealed class ServiceOne
{
    public static HttpClient httpClient = new HttpClient();
    public async Task<int> MakeRequestToGoogle()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity(kind: System.Diagnostics.ActivityKind.Producer, name: "Google Request")!;
        try
        {

            var tags = new ActivityTagsCollection();
            tags.Add("userId", 30);

            activity.AddEvent(new("google reqest is start", tags: tags));
            activity.AddTag("request.schema", "https");
            activity.AddTag("request.method", "GET");

            var result = await httpClient.GetAsync("https://www.google.com");
            var responseContent = await result.Content.ReadAsStringAsync();


            tags.Add("google body length", responseContent.Length);
            activity.AddEvent(new("google reqest is finish", tags: tags));

            var serviceTwo = new ServiceTwo();
            var fileLength = await serviceTwo.WriteToFile("Hello world!");

            return responseContent.Length;

        }
        catch (Exception ex)
        {
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            return -1;
        }
    }
}

internal sealed class ServiceTwo
{
    public async Task<int> WriteToFile(string text)
    {
        Activity.Current?.SetTag("Current activity", "1");
        using (var activity = ActivitySourceProvider.Source.StartActivity(name: "WriteToFile1", kind: System.Diagnostics.ActivityKind.Server))
        {
            await File.WriteAllTextAsync("myfile.txt", text);
        }

        using (var activity = ActivitySourceProvider.SourceFile.StartActivity(name: "writetofile2"))
        {
            await File.WriteAllTextAsync("myfile.txt", text);
        }

        return (await File.ReadAllTextAsync("myfile.txt")).Length;
    }
}