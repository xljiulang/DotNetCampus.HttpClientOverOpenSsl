# DotNetCampus.HttpClientOverOpenSsl

[English](./README.en-us.md)

让 `HttpClient` 的 HTTPS 请求通过 OpenSSL 原生 DLL（libssl-3）完成 TLS 握手和数据传输。

## 简介

`DotNetCampus.HttpClientOverOpenSsl` 提供了一个基于 `SocketsHttpHandler` + OpenSSL 的 `HttpMessageHandler`，将 HTTPS 请求的 TLS 层交给 OpenSSL 原生库处理，而非依赖 .NET 内置的 `SslStream`。

核心类是 `OpenSslSocketsHttpHandler`（继承自 `HttpMessageHandler`）和便捷封装 `OpenSslHttpClient`（继承自 `HttpClient`）。

## 使用方式

### 1. 安装 NuGet 包

```shell
dotnet add package DotNetCampus.HttpClientOverOpenSsl
```

### 2. 准备 OpenSSL 原生 DLL

本库依赖 OpenSSL 3.x 的原生库文件。你可以选择以下两种方式之一：

#### 方式一：安装 openssl-native NuGet 包（推荐）

```shell
dotnet add package openssl-native
```

此包会自动根据运行时平台（x86 / x64 / arm64）将对应的 DLL 复制到输出目录。

#### 方式二：手动部署 DLL

从 [OpenSSL 官方](https://www.openssl.org/) 或 [slproweb](https://slproweb.com/products/Win32OpenSSL.html) 下载预编译的 Windows 版本，或自编译获取以下文件，放置到应用程序的输出目录：

| 平台 | libssl 文件 | libcrypto 文件 |
|------|------------|---------------|
| **Windows x64** | `libssl-3-x64.dll` | `libcrypto-3-x64.dll` |
| **Windows x86** | `libssl-3.dll` | `libcrypto-3.dll` |
| **Windows arm64** | `libssl-3-arm64.dll` | `libcrypto-3-arm64.dll` |

> 库在加载时会按 `AppContext.BaseDirectory` → `runtimes/{rid}/native/` → 自定义 `FallbackLibraryPath` 的顺序查找上述 DLL。如需指定自定义路径，可设置 `OpenSSLNative.FallbackLibraryPath`。

### 3. 在 HttpClient 中使用

#### 方式一：使用 `OpenSslHttpClient`（推荐）

`OpenSslHttpClient` 继承自 `HttpClient`，内部已预配置 `OpenSslSocketsHttpHandler`，开箱即用：

```csharp
using var client = new OpenSslHttpClient();
var html = await client.GetStringAsync("https://www.baidu.com");
```

用法与标准 `HttpClient` 完全一致，所有 `HttpClient` 的方法均可直接调用：

```csharp
using var client = new OpenSslHttpClient();

// GET 请求
var response = await client.GetAsync("https://example.com/api/data");

// POST 请求
var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
response = await client.PostAsync("https://example.com/api/submit", content);

// 自定义请求
using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
request.Headers.Add("X-Custom-Header", "value");
response = await client.SendAsync(request);
```

#### 方式二：使用 `OpenSslSocketsHttpHandler`

如果需要更精细的控制（例如自定义 `SocketsHttpHandler` 的超时、代理、Cookie 等设置），可以直接使用 `OpenSslSocketsHttpHandler`：

```csharp
var innerHandler = new SocketsHttpHandler
{
    UseCookies = false,
    MaxConnectionsPerServer = 32,
    PooledConnectionLifetime = TimeSpan.FromMinutes(2)
};

using var handler = new OpenSslSocketsHttpHandler(innerHandler);
using var client = new HttpClient(handler);
var response = await client.GetAsync("https://example.com");
```

> `OpenSslSocketsHttpHandler` 仅处理 HTTPS 请求的 TLS 层。HTTP（明文）请求不受影响，走默认 TCP 连接。

## 许可证

MIT