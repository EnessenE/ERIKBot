from wcmodel import WCModel
from Image.emoji_loader import EmojiResolver
from typing import Dict, List
import discord
from discord_slash.context import ComponentContext
import loader
import logging
import configparser
import os

client = discord.Client(intents=discord.Intents.all())
loader.init(client)

config = configparser.ConfigParser()
env = os.getenv("BOT_ENV")
config.read(f"config.{env}.ini")

print(f"Running in environment {env}")

logging.basicConfig(level=logging.INFO)

logger = logging.getLogger('discord')
logger.setLevel(logging.INFO)
handler = logging.FileHandler(filename='discord.log', encoding='utf-8', mode='w')
handler.setFormatter(logging.Formatter('%(asctime)s:%(levelname)s:%(name)s: %(message)s'))
logger.addHandler(handler)

from StatusChanger import StatusChanger
from commands import Commands
from cloud import *
commands = Commands()
statuses = StatusChanger()

MAX_MESSAGES = 10_000
DEFAULT_DAYS = 50

@client.event
async def on_ready():
    print('We have logged in as {0.user}'.format(client))
    await statuses.Start()
    await emojiInit()

@client.event
async def on_message(message):
    if message.author == client.user:
        return

    messageattachments = message.attachments
    if len(messageattachments) > 0:
        for attachment in messageattachments:
            if attachment.filename.endswith('.exe'):
                await message.add_reaction('❗')
                await message.reply("Careful, this is a .exe file!")
            else:
                break

@client.event
async def on_guild_join(server: discord.Guild):
    await emojiInitServer(server)

@client.event
async def on_component(ctx: ComponentContext):
    # you may want to filter or change behaviour based on custom_id or message
    await commands.HandleInteraction(ctx)

client.run(config['bot']['token'])

@emojis.error
async def emojis_error(ctx, error):
	if isinstance(error, NoPrivateMessage):
		await ctx.channel.send("This command can only be used in a server channel")
	else:
		traceback.print_exc()