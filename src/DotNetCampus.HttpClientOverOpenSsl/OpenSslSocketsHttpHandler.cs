using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DotNetCampus.HttpClientOverOpenSsl
{
    /// <summary>
    /// 提供一个 <see cref="HttpMessageHandler" />，通过 OpenSSL（基于 <see cref="SocketsHttpHandler" />）执行 TLS/SSL
    /// 操作，允许通过 <see cref="OpenSslClientAuthenticationOptions" /> 自定义 TLS 身份验证选项。
    /// </summary>
    public sealed class OpenSslSocketsHttpHandler : DelegatingHandler
    {
        private bool _useProxy = true;
        private IWebProxy? _proxy = WebRequest.DefaultWebProxy;
        private readonly SocketsHttpHandler _handler = new();

        private static readonly HashSet<string> _socksSchemes = new(StringComparer.OrdinalIgnoreCase) { "socks5", "socks4a", "socks4" };
        private static readonly bool UseOpenSslAsyncStream = OperatingSystem.IsWindows();

        /// <summary>
        /// 获取或设置连接在连接池中空闲多久后可被视为可重用。
        /// </summary>
        /// <value>
        /// 连接在连接池中的最长空闲时间。此属性的默认值在 .NET 6 及更高版本中为 1 分钟，
        /// 在 .NET Core 和 .NET 5 中为 2 分钟。
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 指定的值小于 <see cref="TimeSpan.Zero" />（<see cref="System.Threading.Timeout.InfiniteTimeSpan" /> 除外）。
        /// </exception>
        public TimeSpan PooledConnectionIdleTimeout
        {
            get => _handler.PooledConnectionIdleTimeout;
            set => _handler.PooledConnectionIdleTimeout = value;
        }
        /// <summary>
        /// 获取或设置连接在连接池中可被视为可重用的最长时间。
        /// </summary>
        /// <value>
        /// 连接在连接池中的最长时间。此属性的默认值为 <see cref="System.Threading.Timeout.InfiniteTimeSpan" />。
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 指定的值小于 <see cref="TimeSpan.Zero" /> 或等于 <see cref="System.Threading.Timeout.InfiniteTimeSpan" />。
        /// </exception>
        public TimeSpan PooledConnectionLifetime
        {
            get => _handler.PooledConnectionLifetime;
            set => _handler.PooledConnectionLifetime = value;
        }
        /// <summary>
        /// 获取或设置一个值，指示处理程序是否随请求发送 Authorization 标头。
        /// </summary>
        /// <value>
        /// 如果处理程序随请求发送 Authorization 标头，则为 <see langword="true" />；否则为 <see langword="false" />。
        /// </value>
        public bool PreAuthenticate
        {
            get => _handler.PreAuthenticate;
            set => _handler.PreAuthenticate = value;
        }
        /// <summary>
        /// 获取一个可写字典（即映射），用于存储 <see cref="HttpClient" /> 请求的自定义属性。
        /// 该字典初始化为空；你可以为自定义处理程序和特殊处理插入和查询键值对。
        /// </summary>
        public IDictionary<string, object?> Properties => _handler.Properties;
        /// <summary>
        /// 获取或设置当 <see cref="SocketsHttpHandler.UseProxy" /> 属性为 <see langword="true" /> 时的自定义代理。
        /// </summary>
        /// <value>
        /// 自定义代理。
        /// </value>
        public IWebProxy? Proxy
        {
            get => UseOpenSslAsyncStream ? _proxy : _handler.Proxy;
            set
            {
                if (UseOpenSslAsyncStream)
                {
                    _proxy = value;
                }
                else
                {
                    _handler.Proxy = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置一个回调，用于选择对请求标头值进行编码的 <see cref="System.Text.Encoding" />。
        /// </summary>
        /// <value>
        /// 标头编码选择器回调，用于选择对指定请求标头名称的值进行编码的 <see cref="System.Text.Encoding" />，
        /// 或为 <see langword="null" /> 以表示默认行为。
        /// </value>
        public HeaderEncodingSelector<HttpRequestMessage>? RequestHeaderEncodingSelector
        {
            get => _handler.RequestHeaderEncodingSelector;
            set => _handler.RequestHeaderEncodingSelector = value;
        }
        /// <summary>
        /// 获取或设置等待从响应中排出数据的超时时间。
        /// </summary>
        /// <value>
        /// 等待从响应中排出数据的超时时间。
        /// </value>
        public TimeSpan ResponseDrainTimeout
        {
            get => _handler.ResponseDrainTimeout;
            set => _handler.ResponseDrainTimeout = value;
        }
        /// <summary>
        /// 获取或设置一个回调，用于选择对响应标头值进行解码的 <see cref="System.Text.Encoding" />。
        /// </summary>
        /// <value>
        /// 标头编码选择器回调，用于选择对指定响应标头名称的值进行解码的 <see cref="System.Text.Encoding" />，
        /// 或为 <see langword="null" /> 以表示默认行为。
        /// </value>
        public HeaderEncodingSelector<HttpRequestMessage>? ResponseHeaderEncodingSelector
        {
            get => _handler.ResponseHeaderEncodingSelector;
            set => _handler.ResponseHeaderEncodingSelector = value;
        }
        /// <summary>
        /// 获取或设置用于客户端 TLS 身份验证的选项集。
        /// </summary>
        /// <value>
        /// 用于客户端 TLS 身份验证的选项集。
        /// </value>
        public OpenSslClientAuthenticationOptions SslOptions { get; set; } = new();
        /// <summary>
        /// 获取或设置一个值，指示处理程序是否应使用 Cookie。
        /// </summary>
        /// <value>
        /// 指示处理程序是否应使用 Cookie 的值。
        /// </value>
        public bool UseCookies
        {
            get => _handler.UseCookies;
            set => _handler.UseCookies = value;
        }
        /// <summary>
        /// 获取或设置一个值，指示处理程序是否应使用代理。
        /// </summary>
        /// <value>
        /// 指示处理程序是否应使用代理的值。
        /// </value>
        public bool UseProxy
        {
            get => UseOpenSslAsyncStream ? _useProxy : _handler.UseProxy;
            set
            {
                if (UseOpenSslAsyncStream)
                {
                    _useProxy = value;
                }
                else
                {
                    _handler.UseProxy = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置一个值，指示当所有现有连接上的最大并发流数达到上限时，
        /// 是否允许建立额外的 HTTP/2 连接到同一服务器。
        /// </summary>
        /// <value>
        /// 如果允许创建额外的 HTTP/2 连接，则为 <see langword="true" />；否则为 <see langword="false" />。
        /// </value>
        public bool EnableMultipleHttp2Connections
        {
            get => _handler.EnableMultipleHttp2Connections;
            set => _handler.EnableMultipleHttp2Connections = value;
        }

        /// <summary>
        /// 获取或设置响应标头的最大长度，以千字节（1024 字节）为单位。
        /// </summary>
        /// <value>
        /// 服务器响应中标头部分的最大大小，以千字节为单位。
        /// </value>
        public int MaxResponseHeadersLength
        {
            get => _handler.MaxResponseHeadersLength;
            set => _handler.MaxResponseHeadersLength = value;
        }
        /// <summary>
        /// 获取或设置可以从响应中排出的最大数据量，以字节为单位。
        /// </summary>
        /// <value>
        /// 可以从响应中排出的最大数据量，以字节为单位。
        /// </value>
        public int MaxResponseDrainSize
        {
            get => _handler.MaxResponseDrainSize;
            set => _handler.MaxResponseDrainSize = value;
        }
        /// <summary>
        /// 获取或设置允许连接到单个服务器的最大同时 TCP 连接数。
        /// </summary>
        /// <value>
        /// 允许连接到单个服务器的最大同时 TCP 连接数。
        /// </value>
        public int MaxConnectionsPerServer
        {
            get => _handler.MaxConnectionsPerServer;
            set => _handler.MaxConnectionsPerServer = value;
        }
        /// <summary>
        /// 获取或设置允许的最大 HTTP 重定向次数。
        /// </summary>
        /// <value>
        /// 允许的最大 HTTP 重定向次数。
        /// </value>
        public int MaxAutomaticRedirections
        {
            get => _handler.MaxAutomaticRedirections;
            set => _handler.MaxAutomaticRedirections = value;
        }

        /// <summary>
        /// 定义由此 <see cref="SocketsHttpHandler" /> 打开的所有连接的初始 HTTP/2 流接收窗口大小。
        /// </summary>
        public int InitialHttp2StreamWindowSize
        {
            get => _handler.InitialHttp2StreamWindowSize;
            set => _handler.InitialHttp2StreamWindowSize = value;
        }
        /// <summary>
        /// 获取或设置一个值，指示处理程序是否应跟随重定向响应。
        /// </summary>
        /// <value>
        /// 如果处理程序应跟随重定向响应，则为 <see langword="true" />；否则为 <see langword="false" />。
        /// 默认值为 <see langword="true" />。
        /// </value>
        public bool AllowAutoRedirect
        {
            get => _handler.AllowAutoRedirect;
            set => _handler.AllowAutoRedirect = value;
        }
        /// <summary>
        /// 获取或设置处理程序用于自动解压缩 HTTP 内容响应的解压缩方法类型。
        /// </summary>
        /// <value>
        /// 处理程序用于自动解压缩 HTTP 内容响应的解压缩方法类型。
        /// </value>
        public DecompressionMethods AutomaticDecompression
        {
            get => _handler.AutomaticDecompression;
            set => _handler.AutomaticDecompression = value;
        }
        /// <summary>
        /// 获取或设置一个自定义回调，用于提供对明文 HTTP 协议流的访问。
        /// </summary>
        /// <value>
        /// 提供对明文 HTTP 协议流访问的回调。
        /// </value>
        public Func<SocketsHttpPlaintextStreamFilterContext, CancellationToken, ValueTask<Stream>>? PlaintextStreamFilter
        {
            get => _handler.PlaintextStreamFilter;
            set => _handler.PlaintextStreamFilter = value;
        }
        /// <summary>
        /// 获取或设置托管的 Cookie 容器对象。
        /// </summary>
        /// <value>
        /// 托管的 Cookie 容器对象。
        /// </value>
        public CookieContainer CookieContainer
        {
            get => _handler.CookieContainer;
            set => _handler.CookieContainer = value;
        }
        /// <summary>
        /// 获取或设置连接建立超时前等待的时间。
        /// </summary>
        /// <value>
        /// 连接建立超时前等待的时间。默认值为 <see cref="System.Threading.Timeout.InfiniteTimeSpan" />。
        /// </value>
        public TimeSpan ConnectTimeout
        {
            get => _handler.ConnectTimeout;
            set => _handler.ConnectTimeout = value;
        }
        /// <summary>
        /// 当使用默认（系统）代理时，获取或设置用于向默认代理服务器提交身份验证的凭据。
        /// </summary>
        /// <value>
        /// 用于向身份验证代理验证用户的凭据。
        /// </value>
        public ICredentials? DefaultProxyCredentials
        {
            get => _handler.DefaultProxyCredentials;
            set => _handler.DefaultProxyCredentials = value;
        }
        /// <summary>
        /// 获取或设置服务器 HTTP 100 Continue 响应的超时值。
        /// </summary>
        /// <value>
        /// 等待 HTTP 100 Continue 的时间间隔。默认值为 1 秒。
        /// </value>
        public TimeSpan Expect100ContinueTimeout
        {
            get => _handler.Expect100ContinueTimeout;
            set => _handler.Expect100ContinueTimeout = value;
        }
        /// <summary>
        /// 获取或设置保持活动的 ping 延迟。
        /// </summary>
        /// <value>
        /// 保持活动的 ping 延迟。默认值为 <see cref="Timeout.InfiniteTimeSpan" />。
        /// </value>
        public TimeSpan KeepAlivePingDelay
        {
            get => _handler.KeepAlivePingDelay;
            set => _handler.KeepAlivePingDelay = value;
        }
        /// <summary>
        /// 获取或设置保持活动的 ping 超时。
        /// </summary>
        /// <value>
        /// 保持活动的 ping 超时。默认值为 20 秒。
        /// </value>
        public TimeSpan KeepAlivePingTimeout
        {
            get => _handler.KeepAlivePingTimeout;
            set => _handler.KeepAlivePingTimeout = value;
        }
        /// <summary>
        /// 获取或设置保持活动的 ping 行为。
        /// </summary>
        /// <value>
        /// 保持活动的 ping 行为。
        /// </value>
        public HttpKeepAlivePingPolicy KeepAlivePingPolicy
        {
            get => _handler.KeepAlivePingPolicy;
            set => _handler.KeepAlivePingPolicy = value;
        }
        /// <summary>
        /// 获取或设置身份验证信息，该信息由此处理程序使用。
        /// </summary>
        /// <value>
        /// 与处理程序关联的身份验证凭据。默认值为 <see langword="null" />。
        /// </value>
        public ICredentials? Credentials
        {
            get => _handler.Credentials;
            set => _handler.Credentials = value;
        }

        /// <summary>
        /// 获取或设置用于传播分布式跟踪和上下文的传播器。使用 <see langword="null" /> 禁用传播。
        /// </summary>
        /// <value>
        /// 默认值为 <see cref="DistributedContextPropagator.Current" />。
        /// </value>
        public DistributedContextPropagator? ActivityHeadersPropagator
        {
            get => _handler.ActivityHeadersPropagator;
            set => _handler.ActivityHeadersPropagator = value;
        }

        /// <summary>
        /// 初始化 <see cref="OpenSslSocketsHttpHandler" /> 类的新实例。
        /// </summary>
        public OpenSslSocketsHttpHandler()
        {
            if (UseOpenSslAsyncStream)
            {
                _handler.UseProxy = false;
                _handler.ConnectCallback = ConnectAsync;
            }

            InnerHandler = _handler;
        }

        /// <inheritdoc/>
        protected sealed override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ConnectionOptions.Set(request);
            var response = base.Send(request, cancellationToken);
            ConnectionOptions.Remove(request);
            return response;
        }

        /// <inheritdoc/>
        protected sealed override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ConnectionOptions.Set(request);
            var response = await base.SendAsync(request, cancellationToken);
            ConnectionOptions.Remove(request);
            return response;
        }

        /// <summary>
        /// 建立连接。
        /// </summary>
        /// <param name="context">套接字 HTTP 连接上下文。</param>
        /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
        /// <returns>连接成功后返回的网络流。</returns>
        /// <exception cref="InvalidOperationException">请求 URI 为 null。</exception>
        /// <exception cref="NotSupportedException">不支持的 HTTP 版本或代理协议。</exception>
        private ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var request = context.InitialRequestMessage;
            if (request.VersionPolicy != HttpVersionPolicy.RequestVersionOrLower && request.Version >= HttpVersion.Version30)
            {
                throw new NotSupportedException($"自定义连接功能不支持HTTP/{request.Version}");
            }

            var useProxy = this.UseProxy;
            if (useProxy == false || this.Proxy == null)
            {
                return this.EstablishTcpHostAsync(context, cancellationToken);
            }

            if (request.RequestUri == null)
            {
                throw new InvalidOperationException("RequestUri不能为NULL");
            }

            var proxyUri = this.Proxy.GetProxy(request.RequestUri);
            if (proxyUri == null)
            {
                return this.EstablishTcpHostAsync(context, cancellationToken);
            }

            if (Uri.UriSchemeHttp == proxyUri.Scheme)
            {
                return this.EstablishProxyTunnelAsync(proxyUri, context, cancellationToken);
            }

            if (_socksSchemes.Contains(proxyUri.Scheme))
            {
                return this.EstablishSocksTunnelAsync(proxyUri, context, cancellationToken);
            }

            throw new NotSupportedException($"自定义连接不支持{proxyUri.Scheme}代理协议的服务器");
        }

        /// <summary>
        /// 通过 TCP 直接连接目标主机并建立 SSL 隧道。
        /// </summary>
        /// <param name="context">套接字 HTTP 连接上下文。</param>
        /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
        /// <returns>连接成功后返回的包含 SSL 加密的网络流。</returns>
        private async ValueTask<Stream> EstablishTcpHostAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var host = context.DnsEndPoint.Host;
            var port = context.DnsEndPoint.Port;
            var connectionOptions = ConnectionOptions.Get(context.InitialRequestMessage);
            var networkStream = await TcpConnectAsync(host, port, cancellationToken);

            return await SslConnectAsync(connectionOptions, networkStream, this.SslOptions, host, cancellationToken);
        }

        /// <summary>
        /// 通过 HTTP 隧道代理建立连接。
        /// </summary>
        /// <param name="proxyUri">代理服务器的 URI。</param>
        /// <param name="context">套接字 HTTP 连接上下文。</param>
        /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
        /// <returns>连接成功后返回的包含 SSL 加密的网络流。</returns>
        private async ValueTask<Stream> EstablishProxyTunnelAsync(Uri proxyUri, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var networkStream = await TcpConnectAsync(proxyUri.Host, proxyUri.Port, cancellationToken);
            var host = context.DnsEndPoint.Host;
            var port = context.DnsEndPoint.Port;
            var connectionOptions = ConnectionOptions.Get(context.InitialRequestMessage);

            await ProxyHelper.EstablishProxyTunnelAsync(
                networkStream,
                host,
                port,
                proxyUri,
                this.Proxy?.Credentials,
                cancellationToken);

            return await SslConnectAsync(
                connectionOptions,
                networkStream,
                this.SslOptions,
                context.DnsEndPoint.Host,
                cancellationToken);
        }

        /// <summary>
        /// 通过 SOCKS 隧道代理建立连接。
        /// </summary>
        /// <param name="proxyUri">代理服务器的 URI。</param>
        /// <param name="context">套接字 HTTP 连接上下文。</param>
        /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
        /// <returns>连接成功后返回的包含 SSL 加密的网络流。</returns>
        private async ValueTask<Stream> EstablishSocksTunnelAsync(Uri proxyUri, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var networkStream = await TcpConnectAsync(proxyUri.Host, proxyUri.Port, cancellationToken);
            var host = context.DnsEndPoint.Host;
            var port = context.DnsEndPoint.Port;
            var connectionOptions = ConnectionOptions.Get(context.InitialRequestMessage);

            await SocksHelper.EstablishSocksTunnelAsync(
                networkStream,
                host,
                port,
                proxyUri,
                this.Proxy?.Credentials,
                async: true,
                cancellationToken);

            return await SslConnectAsync(
                connectionOptions,
                networkStream,
                this.SslOptions,
                context.DnsEndPoint.Host,
                cancellationToken);
        }

        /// <summary>
        /// 建立 TCP 连接。
        /// </summary>
        /// <param name="host">目标主机名。</param>
        /// <param name="port">目标端口号。</param>
        /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
        /// <returns>连接成功后返回的 TCP 网络流。</returns>
        private async static Task<NetworkStream> TcpConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(host, port, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 在现有网络流上建立 SSL 连接。
        /// </summary>
        /// <param name="connectionOptions">连接选项。</param>
        /// <param name="networkStream">现有的网络流。</param>
        /// <param name="sslOptions">SSL 身份验证选项。</param>
        /// <param name="host">目标主机名。</param>
        /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
        /// <returns>连接成功后返回的包含 SSL 加密的流。</returns>
        private async static Task<Stream> SslConnectAsync(ConnectionOptions connectionOptions, NetworkStream networkStream, OpenSslClientAuthenticationOptions sslOptions, string host, CancellationToken cancellationToken)
        {
            if (connectionOptions.IsSecurity == false)
            {
                return networkStream;
            }

            var openSslStream = new OpenSslAsyncStream(networkStream, leaveInnerStreamOpen: false);
            var targetHost = string.IsNullOrEmpty(sslOptions.TargetHost) ? host : sslOptions.TargetHost;
            await openSslStream.AuthenticateAsClientAsync(new OpenSslClientAuthenticationOptions
            {
                TargetHost = targetHost
            }, cancellationToken).ConfigureAwait(false);

            return openSslStream;
        }
    }
}
