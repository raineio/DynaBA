import discord
import time
import yaml

bot = discord.Bot()

with open("config.yml", "r") as ymlfile:
    config = yaml.load(ymlfile, Loader = yaml.Loader)

#Unused for now, will add this in later, once BA runs happen
fun_facts = [
    "Did you know, in Absolute Virtues room, there is a crack in the east side of the room!",
    "Did you know, if the Centaur gets away with a Berserk cast, your run leader will hate you?"
    "Did you know, if you use let the Skatene before Absolute Virtue chirp you, and you emote, you will slow dance!"
]

@bot.event
async def on_ready():
    print(f'Logged on as user {bot.user} with ID {bot.application_id} and intents {bot.intents}')

@bot.slash_command()
async def portal_map(ctx: discord.ApplicationContext):
    embed = discord.Embed(title = config["portal_map"]["title"], description = config["portal_map"]["description"])
    embed.add_field(name = "", value=config["portal_map"]["content"])
    embed.set_image(url = config["portal_map"]["image"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def portal_macro(ctx: discord.ApplicationContext):
    embed = discord.Embed(title=config["portal_assignments"]["title"], description = config["portal_assignments"]["description"])
    embed.add_field(name = "", value=config["portal_assignments"]["content"])    

    await ctx.respond(embed = embed)

@bot.slash_command()
async def first_timer(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = config["first_timer"]["title"], description = config["first_timer"]["description"])
    embed.add_field(name = "", value = config["first_timer"]["content"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def magicite(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = config["magia"]["title"], description = config["magia"]["description"])
    embed.add_field(name = "", value = config["magia"]["content"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def faeries(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = config["faerie_map"]["title"], description = config["faerie_map"]["description"])
    embed.add_field(name = "", value = config["faerie_map"]["content"])
    embed.set_image(url = config["faerie_map"]["image"])

    await ctx.respond(embed = embed)

bot.run(config["token"])
