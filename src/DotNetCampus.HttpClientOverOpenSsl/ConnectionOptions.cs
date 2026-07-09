namespace DotNetCampus.HttpClientOverOpenSsl
{
    sealed class ConnectionOptions
    {
        private static readonly HttpRequestOptionsKey<ConnectionOptions> key = new(nameof(ConnectionOptions));

        /// <summary>
        /// 是否为安全传输
        /// </summary>
        public bool IsSecurity { get; }

        /// <summary>
        /// 原始请求Uri
        /// </summary>
        public Uri OriginalUri { get; }

        /// <summary>
        /// 连接选项
        /// </summary>
        /// <param name="isSecurity"></param> 
        /// <param name="originalUri"></param>
        public ConnectionOptions(bool isSecurity, Uri originalUri)
        {
            this.IsSecurity = isSecurity;
            this.OriginalUri = originalUri;
        }


        /// <summary>
        /// 获取自定义连接选项
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ConnectionOptions Get(HttpRequestMessage request)
        {
            return request.Options.TryGetValue(key, out var options)
                ? options
                : throw new InvalidOperationException("必须先 Set()");
        }

        /// <summary>
        /// 设置使用自定义连接
        /// </summary>
        /// <param name="request"></param>   
        public static void Set(HttpRequestMessage request)
        {
            if (request.Options.TryGetValue(key, out _))
            {
                return;
            }

            var originalUri = request.RequestUri ?? throw new HttpRequestException("必须指定请求的URI");
            var isSecurity = originalUri.Scheme == Uri.UriSchemeHttps
                || originalUri.Scheme == Uri.UriSchemeWss
                || originalUri.Scheme == Uri.UriSchemeFtps;

            if (isSecurity == true)
            {
                // 修改Scheme之前，记录原始的Host
                if (request.Headers.Host == null)
                {
                    request.Headers.Host = originalUri.Authority;
                }

                // 修改协议非安全Scheme防止自动ssl连接
                if (originalUri.Scheme == Uri.UriSchemeHttps)
                {
                    request.RequestUri = new UriBuilder(originalUri) { Scheme = Uri.UriSchemeHttp }.Uri;
                }
                else if (originalUri.Scheme == Uri.UriSchemeWss)
                {
                    request.RequestUri = new UriBuilder(originalUri) { Scheme = Uri.UriSchemeWs }.Uri;
                }
                else if (originalUri.Scheme == Uri.UriSchemeFtps)
                {
                    request.RequestUri = new UriBuilder(originalUri) { Scheme = Uri.UriSchemeFtp }.Uri;
                }
            }

            var options = new ConnectionOptions(isSecurity, originalUri);
            request.Options.Set(key, options);
        }

        /// <summary>
        /// 移除使用自定义连接
        /// </summary>
        /// <param name="request"></param>
        public static void Remove(HttpRequestMessage request)
        {
            if (request.Options.Remove(key.Key, out var value) &&
                value is ConnectionOptions options)
            {
                request.RequestUri = options.OriginalUri;
            }
        }
    }
}
