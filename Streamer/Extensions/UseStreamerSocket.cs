using Streamer.Sockets;

namespace Streamer.Engine;

public static class UseStreamerSocket
{
    public static IApplicationBuilder UseSocketStreaming(this IApplicationBuilder app)
    {
        return app.UseMiddleware<StreamerSocket>();
    }
}