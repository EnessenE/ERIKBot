import collections
import os
import sys
from typing import Dict, List
import json

from discord_slash.context import ComponentContext

from preprocessing import resolve_tags, get_emojis
from loader import *
from datetime import datetime, timedelta
from tqdm import tqdm
from tqdm.asyncio import tqdm_asyncio
from discord.ext.commands import Bot, has_permissions, MissingPermissions, guild_only, NoPrivateMessage
import traceback
from loader import *
import discord

async def server_messages(server: discord.Guild, to_edit: discord.Message, authorid) -> list:
	date_after = server.created_at
	messages = []
	# get all readable channels
	channels: List[discord.TextChannel] = list(filter(
		lambda c: c.permissions_for(server.me).read_messages,
		server.text_channels
	))
	for i, channel in tqdm(list(enumerate(channels)), desc=f"Channels {server.name}", file=sys.stdout):
		print(channel.name)
		print(len(messages))
		await to_edit.edit(content=f"> Reading {channel.mention} ({i+1}/{len(channels)}) (total up til now: {len(messages)} messages)")
		# for every message in the channel up to a limit
		async for message in tqdm_asyncio(channel.history(limit=1000000, after=date_after), desc=channel.name, file=sys.stdout, leave=False):
			if not message.author.bot:
				messages.append(message)
	
	authorMessages = dict()
	for message in messages:
		if not message.author.bot:
			if not message.author.id in authorMessages:
				authorMessages[message.author.id] = []
			authorMessages[message.author.id].append(message.content)

	return authorMessages[authorid], authorMessages


async def load(ctx, authorid, days=None):
	finalmessages = []
	await ctx.channel.send(f"Loading messages since the server creation")
	to_edit = await ctx.channel.send(".")
	server = ctx.guild
	
	preload = False
	authorFilepath = f"files/save_{server.id}_{authorid}.json"
	if (os.path.isfile(authorFilepath)):
		preload = True

	if not preload:
		messages, authorMessages = await server_messages(server, to_edit, authorid)
		# we save messages for fast reloading
		for lauthorid, value in authorMessages.items():
			with open(f"files/save_{server.id}_{lauthorid}.json", "w") as fmessages:
				json.dump(value, fmessages)
				
		finalmessages = messages
		await to_edit.edit(content=f"Done, loaded new data from {len(server.channels)} channels!")
	else:
		await to_edit.edit(content=f"Loading preloaded data...")
		with open(authorFilepath) as jsonfile:
			data = json.load(jsonfile)
			for x in data:
				finalmessages.append(x)
		await to_edit.edit(content=f"Done, loaded preloaded data: {len(finalmessages)} messages!")
	return finalmessages



@slash.slash(name='cloud', description="[ALPHA] Generate a worldcloud with your most recently said words!")
async def cloud(ctx: ComponentContext):
	try:
		await ctx.defer()
		print(f"A request has been made to generate a wordcloud by {ctx.author.display_name}")
		async with ctx.channel.typing():

			server: discord.Guild = ctx.channel.guild
			messages = await load(ctx, ctx.author.id)

			# messages_filtered = [message for message in messages if message[0] == ctx.author.id]

			counter = collections.Counter(messages)

			members = []
			# get all unique members targeted in this command
			# members = set(ctx.message.mentions)
			# for user_id in args:
			# 	try:
			# 		member = server.get_member(int(user_id))
			# 		if member:
			# 			members.add(member)
			# 	except ValueError:
			# 		pass
			# if there's none it's for the author of the command
			if not members:
				members.append(ctx.author)

			for member in members:
				#image = result[0]
				text = f"{member.mention}'s Word cloud: \n"
				for item in counter.most_common(10):
					text += f"[{item[0]}] - {item[1]} times used \n"
				
				text += f"Checked {len(messages)} messages for this user"

				await ctx.send(
					content=text, allowed_mentions=discord.AllowedMentions.none(),
					# file=discord.File(fp=image, filename=f"{member.display_name}_word_cloud.png")
				)
	except:
		print("FAILED CLOUD ERROR:", sys.exc_info()[0])
		await ctx.send(content="Failed to create a worldcloud for you :(")
		raise


@cloud.error
async def cloud_error(ctx, error):
	if isinstance(error, NoPrivateMessage):
		await ctx.channel.send("This command can only be used in a server channel")
	else:
		traceback.print_exc()
