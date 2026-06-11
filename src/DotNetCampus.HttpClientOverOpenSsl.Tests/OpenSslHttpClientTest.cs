namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
public sealed class OpenSslHttpClientTest
{
    [TestMethod]
    public async Task OpenSslHttpClient_GetBaidu_ReturnsFirst200Characters()
    {
        using var httpClient = new OpenSslHttpClient();
        var response = await httpClient.GetStringAsync("https://www.baidu.com");

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Length >= 200, $"Expected at least 200 characters, but got {response.Length}.");

        var first200 = response[..200];
        Assert.AreEqual(200, first200.Length);
    }

    [TestMethod]
    public async Task GetAsync_WithHttpsUrl_ReturnsSuccessStatusCode()
    {
        using var httpClient = new OpenSslHttpClient();
        using var response = await httpClient.GetAsync("https://www.baidu.com");

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success status code, but got {(int)response.StatusCode}.");
        Assert.IsNotNull(response.Content);
    }

    [TestMethod]
    public async Task SendAsync_WithHttpsUrl_ReturnsSuccessStatusCode()
    {
        using var httpClient = new OpenSslHttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.baidu.com");
        using var response = await httpClient.SendAsync(request);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success status code, but got {(int)response.StatusCode}.");
        Assert.IsNotNull(response.Content);
    }

    [TestMethod]
    public async Task OpenSslSocketsHttpHandler_WithHttpClient_GetAsync_ReturnsSuccessStatusCode()
    {
        using var handler = new OpenSslSocketsHttpHandler();
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync("https://www.baidu.com");

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success status code, but got {(int)response.StatusCode}.");
        Assert.IsNotNull(response.Content);
    }

    [TestMethod]
    public async Task OpenSslSocketsHttpHandler_WithHttpClient_SendAsync_ReturnsSuccessStatusCode()
    {
        using var handler = new OpenSslSocketsHttpHandler();
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.baidu.com");
        using var response = await client.SendAsync(request);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success status code, but got {(int)response.StatusCode}.");
        Assert.IsNotNull(response.Content);
    }
}
