using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Runtime.InteropServices;
using Discord.Net;
using DynaBA.Models;
using Newtonsoft.Json;

namespace DynaBA;

internal class Program 
{
    public static DiscordSocketClient Client;
    public IServiceProvider Services;

    private bool _run = true;

    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        var token = Yaml.Parse<Bot>("token.yml");

        Services = ConfigureServices();

        Client = Services.GetRequiredService<DiscordSocketClient>();

        Client.Log += ClientOnLog;
        
        Client.Ready += ClientOnReady;
        Client.SlashCommandExecuted += ClientOnSlashCommandExecuted;

        await Client.LoginAsync(TokenType.Bot, token.Token);
        await Client.StartAsync();

        Console.WriteLine("Ready! Started service.");

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ =>
        {
            _run = false;
        });
        
        while (_run)
        {
            await Task.Delay(1000);
        }

        await Client.LogoutAsync();
    }

    private Task ClientOnLog(LogMessage arg)
    {
        Console.WriteLine(arg.ToString());
        return Task.CompletedTask;
    }

    private async Task ClientOnSlashCommandExecuted(SocketSlashCommand arg)
    {
        var data = arg.Data.Options.First();
        await arg.RespondAsync("thonk");
        
        var yamlData = Yaml.Parse<Dictionary<string, Eureka>>("eureka.yml");
        var eurekaCommand = yamlData[data.Name];

        var embed = new EmbedBuilder()
            .WithTitle(eurekaCommand.Title)
            .WithDescription(eurekaCommand.Content)
            .WithImageUrl(eurekaCommand.Image);

        await arg.ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = null;
            msg.Embed = embed.Build();
        });

    }

    private async Task ClientOnReady()
    {
        try
        {
            foreach (var dcg in Client.Guilds)
            {
                var commands = await Client.Rest.GetGuildApplicationCommands(dcg.Id);

                foreach (var command in commands)
                {
                    await command.DeleteAsync();
                }
            }
        }
        catch (HttpException e)
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            await ClientOnLog(new LogMessage(LogSeverity.Error, "BA Helper",$"An HTTP Error occurred! \n{json}"));
        }
        
        await Client.SetActivityAsync(new Game("the Containment Units", ActivityType.Watching));

        var yamlEurekas = Yaml.Parse<Dictionary<string, Eureka>>("eureka.yml");
        var yamlBAs = Yaml.Parse<Dictionary<string, BaldesionArsenal>>("eureka.yml");

        var eurekaCommandBuilder = new SlashCommandBuilder()
            .WithName("eureka")
            .WithDescription("General Eureka related commands");
        
        var baCommandBuilder = new SlashCommandBuilder()
            .WithName("ba")
            .WithDescription("BA related commands");

        foreach (var (commandName, commandArgs) in yamlEurekas)
        {
            eurekaCommandBuilder = eurekaCommandBuilder.AddOption(new SlashCommandOptionBuilder()
                .WithName(commandName)
                .WithDescription(commandArgs.Description)
                .WithType(ApplicationCommandOptionType.SubCommand));
        }
        
        foreach (var (commandName, commandArgs) in yamlBAs)
        {
            baCommandBuilder = baCommandBuilder.AddOption(new SlashCommandOptionBuilder()
                .WithName(commandName)
                .WithDescription(commandArgs.Description)
                .WithType(ApplicationCommandOptionType.SubCommand));
        }
        
        try
        {
            foreach (var dcg in Client.Guilds)
            {
                await Client.Rest.CreateGuildCommand(eurekaCommandBuilder.Build(), dcg.Id);
            }
            
            foreach (var dcg in Client.Guilds)
            {
                await Client.Rest.CreateGuildCommand(baCommandBuilder.Build(), dcg.Id);
            }
        }
        catch (HttpException e)
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            await ClientOnLog(new LogMessage(LogSeverity.Error, "BA Helper",$"An HTTP Error occurred! \n{json}"));
        }
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
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    public static T Parse<T>(string ymlFile)
    {
        var ymlString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ymlFile));
        return Deserializer.Deserialize<T>(ymlString);
    }
}
