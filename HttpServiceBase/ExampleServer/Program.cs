
using BeeEeeLibs.HttpServerBase;
using System.Net;

List<RegisterDelegate> endPoints = new List<RegisterDelegate>();

BuildPostStringBased(endPoints);
BuildPostObjectBased(endPoints);
BuildGetStringBased(endPoints);

AspNetVerb.Start(endPoints.ToArray(), IPAddress.Loopback, 4000);

static void BuildPostStringBased(List<RegisterDelegate> endPoints)
{
    endPoints.Post("/one", async ctx =>
    {
        string body = await ctx.ReadBodyAsString();
        ctx.WriteHeader("text/plain", HttpStatusCode.OK);
        await ctx.WriteBodyString($"Response to: {body}");
    });
}


static void BuildPostObjectBased(List<RegisterDelegate> endPoints)
{
    endPoints.Post("/two", async ctx =>
    {
        TwoRequest? request = await ctx.ReadBody<TwoRequest>();
        TwoResponse response = new TwoResponse()
        {
            StatusCode = 200,
            Request = request
        };
        ctx.WriteHeader("application/json", HttpStatusCode.OK);
        await ctx.WriteBodyObject(response);
    });
}


static void BuildGetStringBased(List<RegisterDelegate> endPoints)
{
    endPoints.Get("/three", async ctx =>
    {
        ctx.WriteHeader("text/plain", HttpStatusCode.OK);
        await ctx.WriteBodyString("You hit endpoint three!");
    });
}

class TwoRequest
{
    public string? Name { get; set; }
    public string? Body { get; set; }
}

class TwoResponse
{
    public int StatusCode { get; set; }
    public TwoRequest? Request { get; set; }
}