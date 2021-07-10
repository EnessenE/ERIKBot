import discord
from discord_slash.context import ComponentContext
import loader
import logging
import configparser

client = discord.Client(intents=discord.Intents.all())
loader.init(client)

config = configparser.ConfigParser()
config.read('config.ini')

logging.basicConfig(level=logging.INFO)

logger = logging.getLogger('discord')
logger.setLevel(logging.INFO)
handler = logging.FileHandler(filename='discord.log', encoding='utf-8', mode='w')
handler.setFormatter(logging.Formatter('%(asctime)s:%(levelname)s:%(name)s: %(message)s'))
logger.addHandler(handler)

from StatusChanger import StatusChanger
from commands import Commands

commands = Commands()
statuses = StatusChanger()

@client.event
async def on_ready():
    print('We have logged in as {0.user}'.format(client))
    await statuses.Start()

@client.event
async def on_message(message):
    if message.author == client.user:
        return

    if message.content.startswith('$hello'):
        await message.channel.send('Hello!')

@client.event
async def on_component(ctx: ComponentContext):
    # you may want to filter or change behaviour based on custom_id or message
    await commands.HandleInteraction(ctx)

client.run(config['bot']['token'])
