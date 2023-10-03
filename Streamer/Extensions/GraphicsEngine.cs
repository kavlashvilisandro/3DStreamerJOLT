using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Streamer.Engine;

public static class GraphicsEngine
{
    public static IServiceCollection AddGraphicsEngine(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IWebDriver>((IServiceProvider provider) =>
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless=new");//TODO: headless mode
            return new ChromeDriver(config.GetValue<string>("ChromiumDriverPath"), options);
        });
        return services;
    }
}