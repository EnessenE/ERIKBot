from RepeatedTimer import RepeatedTimer
import discord
from discord.ext.commands import bot
from discord_slash import SlashCommand
from discord_slash.utils.manage_commands import create_option
import logging
import configparser
import numpy as np    

config = configparser.ConfigParser()
config.read('config.ini')

logging.basicConfig(level=logging.INFO)

logger = logging.getLogger('discord')
logger.setLevel(logging.INFO)
handler = logging.FileHandler(filename='discord.log', encoding='utf-8', mode='w')
handler.setFormatter(logging.Formatter('%(asctime)s:%(levelname)s:%(name)s: %(message)s'))
logger.addHandler(handler)

client = discord.Client(intents=discord.Intents.all())
slash = SlashCommand(client, sync_commands=True) # Declares slash commands through the client.
guild_ids = [474590032267837471]
ApexCharacters = [
      'Bloodhound',
      'Gibraltar',
      'Lifeline',
      'Pathfinder',
      'Wraith',
      'Bangalore',
      'Caustic',
      'Mirage',
      'Octane',
      'Wattson',
      'Crypto',
      'Revenant',
      'Loba',
      'Rampart',
      'Horizon',
      'Fuse',
      'Valkyrie'
    ]
ApexSentence = ['Have you tried', 'How about', 'Try', 'Maybe', 'Ever tried', '', 'Maybe try', 'Just play', 'Have you tried pressing Q quicker with', 'Slide more as', 'Abuse the charge tower with', 'Lose your rp with']

Statuses= [
    "Hi?",
    "Bye?",
    "Fun fact, I am out of them",
    "Punctuation",
    "In these trying times...",
    "In these tough times...",
    "We from E&I would like to...",
    "?!",
    "!?",
    "Apocalypse Drifting",
    "Traintickets please",
    "Starstruck Waterfall",
    "Alive Circles",
    "Mughead",
    "Overstory",
    "Titled Duck Game",
    "Zeus",
    "Empty Soldier",
    "Buildremove",
    "Boringline Florida",
    "Watcher 3: The Normal Hunt",
    "Apex Predators",
    "Goal 2",
    "Sid Meier's Hex Game",
    "Car League",
    "Tom Clancy's: I am long dead",
    "Cold Brass",
    "SWAT 5: The one with magic",
    "Risk of Storm 2",
    "Checkmate, the chessening"
  ]

async def SetStatus():
    status = np.random.choice(Statuses)  
    await client.change_presence(activity=discord.Game(name=status))
    print(f"Setting status to: {status}")

@client.event
async def on_ready():
    print('We have logged in as {0.user}'.format(client))
    await SetStatus()

@client.event
async def on_message(message):
    if message.author == client.user:
        return

    if message.content.startswith('$hello'):
        await message.channel.send('Hello!')

@slash.slash(name='test',
             description='This is just a test command, nothing more.',
             options=[
               create_option(
                 name='optone',
                 description='This is the first option we have.',
                 option_type=3,
                 required=True
               )
             ]             )
async def test(ctx, optone: str):
  await ctx.send(content=f'I got you, you said {optone}!')

@slash.slash(name='ping',)
async def ping(ctx): # Defines a new 'context' (ctx) command called 'ping.'
    await ctx.send(f'Pong! ({client.latency*1000}ms)')

@slash.slash(name='apex', description="Returns a random apex character for you to play.")
async def ping(ctx): # Defines a new 'context' (ctx) command called 'ping.'
    char = np.random.choice(ApexCharacters)  
    sent = np.random.choice(ApexSentence)
    await ctx.send(f'{str(sent)} **{str(char)}**?')

client.run(config['bot']['token'])
statusTimer = RepeatedTimer(1000, SetStatus, "Status")
