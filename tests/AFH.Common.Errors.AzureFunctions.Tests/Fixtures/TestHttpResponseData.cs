using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Tests.Fixtures;

internal sealed class TestHttpCookies : HttpCookies
{
    private readonly List<IHttpCookie> _cookies = [];

    public override void Append(string name, string value)
    {
        _cookies.Add(new HttpCookie(name, value));
    }

    public override void Append(IHttpCookie cookie)
    {
        _cookies.Add(cookie);
    }

    public override IHttpCookie CreateNew()
    {
        return new HttpCookie(string.Empty, string.Empty);
    }
}

internal sealed class TestHttpResponseData(FunctionContext functionContext) : HttpResponseData(functionContext)
{
    public override HttpStatusCode StatusCode { get; set; }

    public override HttpHeadersCollection Headers { get; set; } = [];

    public override Stream Body { get; set; } = new MemoryStream();

    public override HttpCookies Cookies { get; } = new TestHttpCookies();
}

internal sealed class NonSeekableTestHttpResponseData(FunctionContext functionContext) : HttpResponseData(functionContext)
{
    private readonly NonSeekableCaptureStream _body = new();

    public override HttpStatusCode StatusCode { get; set; }

    public override HttpHeadersCollection Headers { get; set; } = [];

    public override Stream Body { get => _body; set => throw new NotSupportedException(); }

    public override HttpCookies Cookies { get; } = new TestHttpCookies();

    public string BodyText => _body.Text;
}

internal sealed class NonSeekableCaptureStream : Stream
{
    private readonly MemoryStream _inner = new();

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public string Text => System.Text.Encoding.UTF8.GetString(_inner.ToArray());

    public override void Flush() => _inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
}
