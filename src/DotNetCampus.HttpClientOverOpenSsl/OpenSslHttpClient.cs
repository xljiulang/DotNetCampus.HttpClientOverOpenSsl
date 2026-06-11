namespace DotNetCampus.HttpClientOverOpenSsl;

/// <summary>
/// 采用 OpenSSL 实现的 Http 网络处理客户端
/// </summary>
public sealed class OpenSslHttpClient() : HttpClient(new OpenSslSocketsHttpHandler())
{
}