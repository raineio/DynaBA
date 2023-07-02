using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynaBA;


internal class SlashCommandHandler
{
    private DiscordSocketClient _client;
    private InteractionService _interaction;
    private IServiceProvider _services;

    public SlashCommandHandler(IServiceProvider services)
    {
        _client = services.GetRequiredService<DiscordSocketClient>();
        _interaction = services.GetRequiredService<InteractionService>();
        _services = services;

        _client.SlashCommandExecuted += SlashCommand;
    }

    private async Task SlashCommand(SocketSlashCommand args)
    {
        var result = await _interaction.ExecuteCommandAsync(
            new SocketInteractionContext<SocketSlashCommand>(_client, args),
            _services);

        if (result.Error != null)
        {
            Console.WriteLine(result.ErrorReason);
        }
    }

    public async Task InitializeAsync()
    {
        var modules = await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        foreach (var servers in _client.Guilds)
        {
            if (modules.ToArray() == null)
            {
                Console.WriteLine($"you dun fucked up, {modules} was null");
            }
            await _interaction.AddModulesToGuildAsync(servers, true, modules.ToArray());
        }
    }
}