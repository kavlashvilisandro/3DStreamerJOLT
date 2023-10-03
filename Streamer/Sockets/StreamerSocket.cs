using System.Collections.Concurrent;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V85.Debugger;

namespace Streamer.Sockets;

public class StreamerSocket : IDisposable
{
    private readonly RequestDelegate _next;
    private IWebDriver _webDriver;
    private readonly Channel<byte[]> _bytesChannel;
    private readonly ChannelReader<byte[]> _bytesChannelReader;
    private readonly ChannelWriter<byte[]> _bytesChannelWriter;
    private readonly IWebHostEnvironment _env;
    
    private IServiceProvider _scopedServiceProvider;
    private const string _initialFunction = @"
                    window.dataUrlToBytes = function(dataUrl) 
                    {
                        const base64 = dataUrl.split(',')[1];
                        const binaryString = window.atob(base64);
                        const len = binaryString.length;
                        const bytes = new Uint8Array(len);

                        for (let i = 0; i < len; i++) {
                            bytes[i] = binaryString.charCodeAt(i);
                        }

                        return bytes;
                    }";
    private const string _bytesCatcher = @"
                            function executeScript() 
                            {
                                const frameDataUrl = document.querySelector('canvas').toDataURL('image/png');
                                const frameBytes = dataUrlToBytes(frameDataUrl);
                                console.log(frameBytes);
                                return frameBytes;
                            }
                            return executeScript();";
    
    public StreamerSocket(RequestDelegate next, IServiceProvider serviceProvider, IWebHostEnvironment env)
    {
        _scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
        _next = next;
        _bytesChannel = Channel.CreateUnbounded<byte[]>();
        _bytesChannelReader = _bytesChannel.Reader;
        _bytesChannelWriter = _bytesChannel.Writer;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (Regex.IsMatch(context.Request.Path,"^/stream/[^/]+$"))
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                _webDriver = _scopedServiceProvider.GetService<IWebDriver>();
                Console.WriteLine("Connection established...");
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                ReadBytesFromSelenium(context, webSocket);
                
                await StreamBytes(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
        else
        {
            await _next(context);
        }
    }
    
    public async Task ReadBytesFromSelenium(HttpContext context, WebSocket socket)
    {
        try
        {
            string resourceName = context.Request.Path.ToString().Split('/')[2];
            if (!File.Exists(Path.Combine(_env.WebRootPath, resourceName)))
            {
                _bytesChannelWriter.Complete();
                context.Response.StatusCode = 400;
                return;
            }
        
            string filePath = Path.Combine(_env.WebRootPath, resourceName);
        
            CancellationToken csToken = context.RequestAborted;
        
            _webDriver.Navigate().GoToUrl($"file://{filePath}");
            IJavaScriptExecutor js = (IJavaScriptExecutor)_webDriver;
            js.ExecuteScript(_initialFunction);
        
            while (socket.State == WebSocketState.Open)
            {
                //byte[] buffer = ConvertToByteArray(js.ExecuteScript(_bytesCatcher));
                byte[] buffer = ConvertToByteArray(js.ExecuteScript("return window.bytes"));
            
                await _bytesChannelWriter.WriteAsync(buffer,csToken);
                await Task.Delay(2);
            }
        
            this.Dispose();
            _bytesChannelWriter.Complete();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            _webDriver.Close();
            _webDriver.Dispose();
        }
    }

    public async Task StreamBytes(HttpContext context, WebSocket socket)
    {
        while (await _bytesChannelReader.WaitToReadAsync())
        {
            if (_bytesChannelReader.TryRead(out byte[] messageBytes))
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(messageBytes, 0, messageBytes.Length),
                    WebSocketMessageType.Binary,
                    endOfMessage: true,
                    context.RequestAborted);
            }
        }
    }

    public void Dispose()
    {
        _webDriver.Close();
        _webDriver.Dispose();
    }
    
   private  static byte[] ConvertToByteArray(object result)
    {
        if (result is System.Collections.Generic.IEnumerable<object> enumerable)
        {
            byte[] byteArray = new byte[enumerable.Count()];
            int index = 0;
            foreach (object element in enumerable)
            {
                if (element is long longValue)
                {
                    byteArray[index++] = (byte)longValue;
                }
                else if (element is int intValue)
                {
                    byteArray[index++] = (byte)intValue;
                }
            }
            return byteArray;
        }
        return null;
    }
}