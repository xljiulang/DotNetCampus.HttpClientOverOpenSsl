using System.Net;
using System.Text;

namespace DotNetCampus.HttpClientOverOpenSsl
{
    static class ProxyHelper
    {
        private const string CRLF = "\r\n";
        private const string HTTP11_SPACE = "HTTP/1.1 ";

        public static async Task EstablishProxyTunnelAsync(
            Stream stream,
            string host,
            int port,
            Uri proxyUri,
            ICredentials? proxyCredentials,
            CancellationToken cancellationToken)
        {
            var requestBuilder = new StringBuilder()
                .Append($"CONNECT {host}:{port} HTTP/1.1").Append(CRLF)
                .Append($"Host: {host}:{port}").Append(CRLF);

            if (proxyCredentials != null)
            {
                var credentials = proxyCredentials.GetCredential(proxyUri, proxyUri.Scheme);
                if (credentials != null)
                {
                    var userInfo = $"{credentials.UserName}:{credentials.Password}";
                    var parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(userInfo));
                    requestBuilder.Append($"Proxy-Authorization: Basic {parameter}").Append(CRLF);
                }
            }

            requestBuilder.Append(CRLF);

            try
            {
                var requestString = requestBuilder.ToString();
                var requestBytes = Encoding.ASCII.GetBytes(requestString);
                await stream.WriteAsync(requestBytes, cancellationToken);
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }

            using (cancellationToken.Register(s => ((Stream)s!).Dispose(), stream))
            {
                var reader = new StreamReader(stream, leaveOpen: true);
                var firstLine = await reader.ReadLineAsync();
                if (firstLine == null)
                {
                    stream.Dispose();
                    throw new IOException("代理服务器意外关闭了连接");
                }

                // HTTP/1.1 200 Connection Established
                if (firstLine.StartsWith(HTTP11_SPACE) == false)
                {
                    stream.Dispose();
                    throw new IOException("代理服务器响应了无效的代理协议");
                }

                var index = firstLine.AsSpan(HTTP11_SPACE.Length).IndexOf(' ');
                if (index <= 0 || int.TryParse(firstLine.AsSpan(HTTP11_SPACE.Length, index), out var statusCode) == false)
                {
                    stream.Dispose();
                    throw new IOException("代理服务器响应了无效的状态码");
                }

                if (statusCode != 200)
                {
                    stream.Dispose();
                    throw new IOException($"代理服务器响应了{statusCode}的状态码");
                }

                // 消费剩余头部
                var nextLine = await reader.ReadLineAsync();
                while (nextLine != null && nextLine.Length > 0)
                {
                    nextLine = await reader.ReadLineAsync();
                }
            }
        }
    }
}
