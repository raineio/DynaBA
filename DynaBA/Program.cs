using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YamlDotNet.Serialization;

namespace DynaBA;

internal class Program 
{

    public DiscordSocketClient _client;
    public IServiceProvider _services;
    public Yaml _yaml;

    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        var token = _yaml.Load("");

        _services = ConfigureServices();

        _client = _services.GetRequiredService<DiscordSocketClient>();

        _client.Ready += async () =>
        {
            await _services.GetRequiredService<SlashCommandHandler>().InitializeAsync();
        };

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    private IServiceProvider ConfigureServices()
    {
        var disConfig = new DiscordSocketConfig { MessageCacheSize = 100 };

        return new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(disConfig))
            .AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<SlashCommandHandler>()
            .BuildServiceProvider();
    }
}

public class Yaml
{
    public string Load(string path)
    {
        var token = "";
        return token;
    }

    private void Parse()
    {

    }

    public void Dispose()
    {

    }
}