import discord
import pymongo as mongo
import time
import yaml

bot = discord.Bot()

with open("config.yml", "r") as ymlfile:
    config = yaml.load(ymlfile, Loader=yaml.Loader)

@bot.event
async def on_ready():
    print(f'Logged on as user {bot.user} with ID {bot.application_id} and intents {bot.intents}')

@bot.slash_command()
async def portal_map(ctx: discord.ApplicationContext):
    embed = discord.Embed(title = config["portal_map"]["title"], description = "")
    embed.add_field(name="", value=config["portal_map"]["content"])
    embed.set_image(url = config["portal_map"]["image"])

    await ctx.respond(embed = embed)

@bot.slash_command()
async def macros(ctx: discord.ApplicationContext):
    embed = discord.Embed(title=config["portal_assignments"]["title"], description="")
    embed.add_field(name="", value=config["portal_assignments"]["content"])    

    await ctx.respond(embed = embed)

@bot.slash_command()
async def first_timer(ctx: discord.ApplicationCommand):
    embed = discord.Embed(title = config["first_timer"]["title"], description="")
    embed.add_field(name="", value = config["first_timer"]["content"])

    await ctx.respond(embed = embed)

bot.run(config["token"])
