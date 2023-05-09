import discord
import pymongo as mongo
import time
    
token_file = open("token.txt", "r")

bot = discord.Bot()
token = token_file.read()

@bot.event
async def on_ready():
    print(f'Logged on as user {bot.user} with ID {bot.application_id} and intents {bot.intents}')

async def HandleDbInteraction():
    mongo.Database.create_collection()
    print(f'Handled db interaction at {time.time} in unix')

@bot.slash_command()
async def map(ctx: discord.ApplicationContext):
    await ctx.respond(discord.Embed("Hydatos Map", "Party portals will be assigned by party leads!\nhttps://i.imgur.com/r9mxDnT.png", color="0x30"))

@bot.slash_command()
async def macros(ctx: discord.ApplicationContext):
    await ctx.respond(discord.Embed(
        f"Portal Macros", 
        f"/macroicon \"Chain Stratagem\"\n"
        f"/party Portal Assignments!<se.2>\n"
        f"/party  <1>\n"
        f"/party  <2>\n"
        f"/party  <3>\n"
        f"/party  <4>\n"
        f"/party  <5>\n"
        f"/party  <6>\n"
        f"/party  <7>\n"
        f"/party  <8>\n"
        f"<se.6>"))

bot.run(token)
