using System.Text;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;

namespace TollService.TestHelper;

public static class MockedRequestExtensions
{
    public static MockedRequest RespondWithJson<T>(this MockedRequest source, T content)
    {
        source.Respond(() =>
        {
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            return Task.FromResult(new HttpResponseMessage
            {
                Content = httpContent,
                StatusCode = System.Net.HttpStatusCode.OK
            });
        });

        return source;
    }
}
