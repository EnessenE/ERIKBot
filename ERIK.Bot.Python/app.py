from typing import Dict, List
import discord
from discord.channel import TextChannel
from discord.role import Role
from discord.user import User
from discord_slash.context import ComponentContext
from threading import Thread
from StatusChanger import StatusChanger
from commands import Commands
from cloud import *
import asyncio

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

commands = Commands()
statuses = StatusChanger()

@client.event
async def on_ready():
    print('We have logged in as {0.user}'.format(client))
    
    thread = Thread(target = startStatusThread)
    thread.start()

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
    print(f"JOINED A NEW GUILD: {server.name}")

@client.event
async def on_component(ctx: ComponentContext):
    # you may want to filter or change behaviour based on custom_id or message 
    if (len(ctx.custom_id) > 30):
        #GUID Interaction, otherwise drop it
        await commands.HandleInteraction(ctx)

monitoredGuildsRecentJoins = []

@client.event 
async def on_member_join(member:discord.Member):
  print("A new user has joined a guild")

  if member.guild.id in monitoredGuild:
      print("A user has joined a monitored guild")
      #monitoredGuildsRecentJoins[member.guild.id] = monitoredGuildsRecentJoins[member.guild.id] + 1
      if member.id in flaggedUsers:
            print("A flagged user has joined a monitored guild")

            role:Role = discord.utils.get(member.guild.roles,name="restricted")
            await member.add_roles(role, reason="Flagged user")
            
            warnMessage = f"Warning, a flagged user has joined the guild **{member.guild.name}**. \nTheir permissions have been restricted. Please manually review permissions of {member.mention} (display name: {member.display_name}, id: {member.id})"
            textChannel:TextChannel = discord.utils.get(member.guild.channels, name="admin_channel")
            if (textChannel!=None):
                await textChannel.send(warnMessage)
            else:
                owner:User = client.get_user(botOwner)
                await owner.send(warnMessage)
            
def startStatusThread():
    asyncio.run(statuses.Start())

client.run(os.getenv("BOT_TOKEN"))
