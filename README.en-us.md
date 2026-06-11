# DotNetCampus.HttpClientOverOpenSsl

[中文](./README.md)

Route `HttpClient` HTTPS requests through the OpenSSL native DLL (libssl-3) for TLS handshake and data transfer.

## Overview

`DotNetCampus.HttpClientOverOpenSsl` provides an `HttpMessageHandler` based on `SocketsHttpHandler` + OpenSSL, delegating the TLS layer of HTTPS requests to the OpenSSL native library instead of relying on .NET's built-in `SslStream`.

The core classes are `OpenSslSocketsHttpHandler` (inherits `HttpMessageHandler`) and the convenience wrapper `OpenSslHttpClient` (inherits `HttpClient`).

## Getting Started

### 1. Install the NuGet Package

```shell
dotnet add package DotNetCampus.HttpClientOverOpenSsl
```

### 2. Prepare the OpenSSL Native DLL

This library depends on OpenSSL 3.x native library files. Choose one of the following approaches:

#### Option A: Install the openssl-native NuGet Package (Recommended)

```shell
dotnet add package openssl-native
```

This package automatically copies the correct DLL for your runtime platform (x86 / x64 / arm64) to the output directory.

#### Option B: Deploy DLLs Manually

Download the following files from the [official OpenSSL site](https://www.openssl.org/) or [slproweb](https://slproweb.com/products/Win32OpenSSL.html) for pre-built Windows binaries, or a custom build, and place them in your application's output directory:

| Platform | libssl File | libcrypto File |
|----------|-------------|----------------|
| **Windows x64** | `libssl-3-x64.dll` | `libcrypto-3-x64.dll` |
| **Windows x86** | `libssl-3.dll` | `libcrypto-3.dll` |
| **Windows arm64** | `libssl-3-arm64.dll` | `libcrypto-3-arm64.dll` |

> The library searches for the above DLLs in this order: `AppContext.BaseDirectory` → `runtimes/{rid}/native/` → custom `FallbackLibraryPath`. To specify a custom path, set `OpenSSLNative.FallbackLibraryPath`.

### 3. Use in HttpClient

#### Option A: Use `OpenSslHttpClient` (Recommended)

`OpenSslHttpClient` inherits from `HttpClient` and comes pre-configured with `OpenSslSocketsHttpHandler`. It works out of the box:

```csharp
using var client = new OpenSslHttpClient();
var html = await client.GetStringAsync("https://www.baidu.com");
```

All standard `HttpClient` methods are available:

```csharp
using var client = new OpenSslHttpClient();

// GET request
var response = await client.GetAsync("https://example.com/api/data");

// POST request
var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
response = await client.PostAsync("https://example.com/api/submit", content);

// Custom request
using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
request.Headers.Add("X-Custom-Header", "value");
response = await client.SendAsync(request);
```

#### Option B: Use `OpenSslSocketsHttpHandler`

For advanced scenarios that require fine-grained control over `SocketsHttpHandler` settings (timeouts, proxy, cookies, etc.), use `OpenSslSocketsHttpHandler` directly:

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

> `OpenSslSocketsHttpHandler` only handles the TLS layer for HTTPS requests. Plain HTTP requests are unaffected and use the default TCP connection.

## License

MIT