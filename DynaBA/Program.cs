using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Runtime.InteropServices;

namespace DynaBA;

internal class Program 
{
    public DiscordSocketClient _client;
    public IServiceProvider _services;

    private bool _run = true;

    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        var token = Yaml.Parse<Bot>("./token.yml");

        _services = ConfigureServices();

        _client = _services.GetRequiredService<DiscordSocketClient>();

        _client.Ready += async () =>
        {
            await _services.GetRequiredService<SlashCommandHandler>().InitializeAsync();
        };

        await _client.LoginAsync(TokenType.Bot, token.Token);
        await _client.StartAsync();

        Console.WriteLine("Ready! Started service.");

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ =>
        {
            _run = false;
        });
        
        while (_run)
        {
            await Task.Delay(1000);
        }

        await _client.LogoutAsync();
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

public static class Yaml
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    public static T Parse<T>(string ymlFile)
    {
        var ymlString = File.ReadAllText(ymlFile);
        return Deserializer.Deserialize<T>(ymlString);
    }
}

public struct Bot
{
    public string Token { get; set; }
}

public struct Macro
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string FieldName { get; set; }
    public string Content { get; set; }
    public string? Image { get; set; }
}