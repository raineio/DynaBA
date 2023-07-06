using Discord;
using Discord.Interactions;
using ContextType = Discord.Interactions.ContextType;

namespace DynaBA.Commands;

[RequireContext(ContextType.Guild)]
public class Commands : InteractionModuleBase
{
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    [SlashCommand("command", "Print specified command, if blank, prints the first command")]
    public async Task PrintAllCommands(string command)
    {
        await RespondAsync("Thinking...");
            
        var embed = new EmbedBuilder();
        var macros = Yaml.Parse<Dictionary<string, Macro>>("./macros.yml");
        var macro = macros.FirstOrDefault(x => x.Key == command).Value;

            
        embed.WithTitle(macro.Title)
            .WithDescription(macro.Description)
            .AddField("Info", macro.Content)
            .WithImageUrl(macro.Image ??= null);
            
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = null;
            msg.Embed = embed.Build();
        });
    }
}