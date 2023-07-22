using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Runtime.InteropServices;
using System.Text;
using Discord.Net;
using DynaBA.Enums;
using DynaBA.Models;
using Newtonsoft.Json;

namespace DynaBA;

internal partial class Program
{
    public static DiscordSocketClient Client;
    public IServiceProvider Services;
    private Bot _bot;
    private bool _run = true;

    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        _bot = Yaml.Parse<Bot>("token.yml");

        Services = ConfigureServices();

        Client = Services.GetRequiredService<DiscordSocketClient>();

        Client.Log += ClientOnLog;

        Client.Ready += ClientOnReady;
        Client.SlashCommandExecuted += ClientOnSlashCommandExecuted;

        await Client.LoginAsync(TokenType.Bot, _bot.Token);
        await Client.StartAsync();

        Console.WriteLine("Ready! Started service.");

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ => { _run = false; });

        while (_run)
        {
            await Task.Delay(1000);
        }

        await File.WriteAllTextAsync("token.yml", Yaml.Stringify(_bot));

        await Client.LogoutAsync();
    }

    private Task ClientOnLog(LogMessage arg)
    {
        Console.WriteLine(arg.ToString());
        return Task.CompletedTask;
    }

    private IServiceProvider ConfigureServices()
    {
        var disConfig = new DiscordSocketConfig { MessageCacheSize = 100 };

        return new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(disConfig))
            .AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()))
            .BuildServiceProvider();
    }
}

public static class Yaml
{
    private static readonly IDeserializer _deserializer =
        new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    private static readonly ISerializer _serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    public static T Parse<T>(string ymlFile)
    {
        var ymlString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ymlFile));
        return _deserializer.Deserialize<T>(ymlString);
    }

    public static string Stringify(object obj)
    {
        return _serializer.Serialize(obj);
    }
}