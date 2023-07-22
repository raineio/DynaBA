using Discord.WebSocket;
using Discord;
using DynaBA.Enums;
using DynaBA.Models;
using Discord.Net;
using Newtonsoft.Json;

namespace DynaBA;

internal partial class Program
{
    private async Task ClientOnReady()
    {
        if (!string.IsNullOrWhiteSpace(_bot.RebootMsg))
        {
            var f = _bot.RebootMsg.Split('.').Select(ulong.Parse).ToArray();
            var channel = Client.GetChannel(f[0]) as SocketTextChannel;
            await channel!.DeleteMessageAsync(f[1]);
            _bot.RebootMsg = null;
        }

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

        var eurekaYml = Yaml.Parse<Dictionary<string, Eureka>>("eureka.yml");
        var baYml = Yaml.Parse<Dictionary<string, BaldesionArsenal>>("baldesion.yml");

        var eurekaCommandBuilder = new SlashCommandBuilder()
            .WithName("eureka")
            .WithDescription("General Eureka related commands");

        var baCommandBuilder = new SlashCommandBuilder()
            .WithName("ba")
            .WithDescription("BA related commands");

        var restartCommandBuilder = new SlashCommandBuilder()
            .WithName("restart")
            .WithDescription("Restarts the bot")
            .WithDefaultMemberPermissions(GuildPermission.ManageMessages);

        foreach (var (commandName, commandArgs) in eurekaYml)
        {
            eurekaCommandBuilder = eurekaCommandBuilder.AddOption(new SlashCommandOptionBuilder()
                .WithName(commandName)
                .WithDescription(commandArgs.Description)
                .WithType(ApplicationCommandOptionType.SubCommand));
        }

        foreach (var (commandName, commandArgs) in baYml)
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

        gearSlashCommandOptionBuilder = Enum.GetValues<GearType>().Aggregate(gearSlashCommandOptionBuilder, (current, gearSet) => current.AddChoice(gearSet.ToString(), (int)gearSet));

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
                await Client.Rest.CreateGuildCommand(baCommandBuilder.Build(), dcg.Id);
                await Client.Rest.CreateGuildCommand(restartCommandBuilder.Build(), dcg.Id);
            }
        }
        catch (HttpException e)
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            await ClientOnLog(new LogMessage(LogSeverity.Error, "BA Helper", $"An HTTP Error occurred! \n{json}"));
        }
    }


}