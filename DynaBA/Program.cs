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

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ => { _run = false; });

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
        switch (arg.CommandName)
        {
            case "eureka":
                var data = arg.Data.Options.First();
                await arg.RespondAsync("thonk");

                var yamlData = Yaml.Parse<Dictionary<string, Eureka>>("eureka.yml");

                if (yamlData.TryGetValue(data.Name, out var eurekaCommand))
                {
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
                else
                {
                    var embed = new EmbedBuilder();

                    switch (data.Name)
                    {
                        case "bis":
                            var gearType = (GearType)data.Options.First().Value;
                            var description = """
                                              There are a few differing opinions on what Best in Slot is, but here is a general primer on what the Eurekan Best in Slot is.
                                                 
                                              **__First off, why do we want Elemental +2 gear?__**
                                              
                                              All jobs want Elemental +2 gear, to varying degrees, because of the elemental bonus you get from it.
                                              
                                              {1}
                                              
                                              **__How do I go about getting what I need to get my Best in Slot?__**
                                                  
                                              Getting Eureka Fragments to upgrade your Elemental +1 gear can only be gotten from the Baldesion Arsenal, of which you get 28 per full run.
                                                  
                                              Gearset for your job: [etro.gg]({0})
                                              
                                              Remember to look at the notes further down the page!
                                              """;
                            var title = "Eurekan Best in Slot for {0}";

#pragma warning disable CS8524 
                            var (t, link, special) = gearType switch
#pragma warning restore CS8524
                            {
                                GearType.Fending => ("Tanks",
                                    "https://etro.gg/gearset/ccdfa90d-a16c-4b9e-a5b7-c6b0d3df6fea", ""),
                                GearType.Healing => ("Healers",
                                    "https://etro.gg/gearset/666db791-128a-43de-ae8a-5b0a89c0fefd",
                                    "Healers can not equip a head piece, considering that the Vermilion Cloak, covers their head as well."),
                                GearType.Striking => ("Monk and Samurai",
                                    "https://etro.gg/gearset/fcf3ce10-8891-4fe1-9db8-2c2c80be2fd5", ""),
                                GearType.Scouting => ("Ninja",
                                    "https://etro.gg/gearset/cb75f756-9825-4688-a977-9a6ee796988", ""),
                                GearType.Maiming => ("Dragoon and Reaper",
                                    "https://etro.gg/gearset/88c62a36-8489-4b30-80f1-c8b932bdf15d", ""),
                                GearType.Aiming => ("Ranged",
                                    "https://etro.gg/gearset/c5ba63fb-a671-4716-b35e-c834c863f8b0", ""),
                                GearType.Casting => ("Casters",
                                    "https://etro.gg/gearset/b573db1c-97fa-4c34-9e49-48a6bd59b1ae",
                                    "Casters can not equip a head piece, considering that the Vermilion Cloak, covers their head as well."),
                            };

                            embed.WithTitle(string.Format(title, t)).WithDescription(string.Format(description, link, special));
                            break;
                    }

                    await arg.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Content = null;
                        msg.Embed = embed.Build();
                    });
                }

                break;
        }
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
            await ClientOnLog(new LogMessage(LogSeverity.Error, "BA Helper", $"An HTTP Error occurred! \n{json}"));
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

        var gearSlashCommandOptionBuilder = new SlashCommandOptionBuilder()
            .WithName("class")
            .WithDescription("Gives a choice with different gear types")
            .WithType(ApplicationCommandOptionType.Integer)
            .WithRequired(true);

        foreach (var gearSet in Enum.GetValues<GearType>())
        {
            gearSlashCommandOptionBuilder = gearSlashCommandOptionBuilder.AddChoice(gearSet.ToString(), (int)gearSet);
        }

        eurekaCommandBuilder.AddOption(new SlashCommandOptionBuilder().WithName("bis")
            .WithDescription("Shows the best in slot gear for Eureka and the Baldesion Arsenal")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(gearSlashCommandOptionBuilder)
        );


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
            await ClientOnLog(new LogMessage(LogSeverity.Error, "BA Helper", $"An HTTP Error occurred! \n{json}"));
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
    private static readonly IDeserializer Deserializer =
        new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    public static T Parse<T>(string ymlFile)
    {
        var ymlString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ymlFile));
        return Deserializer.Deserialize<T>(ymlString);
    }
}