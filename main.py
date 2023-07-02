import discord
import time
import yaml

bot = discord.Bot()

with open("config.yml", "r") as configfile:
    config = yaml.load(configfile, Loader = yaml.Loader)

with open("macros.yml", "r") as macrofile:
    macros = yaml.load(macrofile, Loader = yaml.Loader)

#Unused for now, will add this in later, once BA runs happen
fun_facts = [
    "Did you know, in Absolute Virtues room, there is a crack in the east side of the room!",
    "Did you know, if the Centaur gets away with a Berserk cast, your run leader will hate you?"
    "Did you know, if you use let the Skatene before Absolute Virtue chirp you, and you emote, you will slow dance!"
]

@bot.event
async def on_ready():
    activity_prompt = "Eureka for suspicious activity"
    activity = discord.ActivityType.watching

    await bot.change_presence(status = discord.Status.idle, activity = activity)
    print(f'Logged on as user {bot.user} with ID {bot.application_id} and intents {bot.intents}')

@bot.slash_command()
async def portal_map(ctx: discord.ApplicationContext):
    embed = discord.Embed(title = macros["portal_map"]["title"], description = macros["portal_map"]["description"])
    embed.add_field(name = "", value = macros["portal_map"]["content"])
    embed.set_image(url = macros["portal_map"]["image"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def portal_macro(ctx: discord.ApplicationContext):
    embed = discord.Embed(title = macros["portal_assignments"]["title"], description = macros["portal_assignments"]["description"])
    embed.add_field(name = "", value = macros["portal_assignments"]["content"])    

    await ctx.respond(embed = embed)

@bot.slash_command()
async def first_timer(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = macros["first_timer"]["title"], description = macros["first_timer"]["description"])
    embed.add_field(name = "", value = macros["first_timer"]["content"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def magicite(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = macros["magia"]["title"], description = macros["magia"]["description"])
    embed.add_field(name = "", value = macros["magia"]["content"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def faeries(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = macros["faerie_map"]["title"], description = macros["faerie_map"]["description"])
    embed.add_field(name = "", value = macros["faerie_map"]["content"])
    embed.set_image(url = macros["faerie_map"]["image"])

    await ctx.respond(embed = embed)

bot.run(config["token"])
