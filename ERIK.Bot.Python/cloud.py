import collections
import os
import sys
from typing import Dict, List
import json

from discord.http import json_or_text
import Image.make_image as make_image
from Image.emoji_loader import EmojiResolver
from Models.baseline import WCBaseline as WCModel
from preprocessing import resolve_tags, get_emojis
from loader import *
from datetime import datetime, timedelta
from tqdm import tqdm
from tqdm.asyncio import tqdm_asyncio
from discord.ext.commands import Bot, has_permissions, MissingPermissions, guild_only, NoPrivateMessage
import traceback
from loader import *
import discord


global models
global emojis
global emoji_resolver

emoji_resolver: EmojiResolver = None
models: Dict[discord.Guild, WCModel] = {}
emojis: Dict[discord.Guild, Dict[str, int]] = {}

async def emojiInitServer(server):
	emoji_resolver.load_server_emojis(server)

async def emojiInit():
	global emoji_resolver
	emoji_resolver = EmojiResolver(client)
	await emoji_resolver.load_server_emojis()

async def server_messages(server: discord.Guild, to_edit: discord.Message) -> list:
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
		async for message in tqdm_asyncio(
				channel.history(limit=100000, after=date_after), desc=channel.name, file=sys.stdout, leave=False
		):
			if not message.author.bot:
				messages.append((message.author.id, message.content))
			for reaction in message.reactions:
				reaction_str = str(reaction.emoji)
				async for user in reaction.users():
					messages.append((user.id, reaction_str))
	return messages


async def add_server(server: discord.Guild, messages):
	# create and train a new model
	models[server] = WCModel()
	models[server].train(messages)

	# count the emojis
	semojis = set(str(emoji) for emoji in server.emojis)
	emojis[server] = {e: 0 for e in semojis}
	for _, message in messages:
		for emoji in get_emojis(message, semojis):
			emojis[server][emoji] += 1


async def load(ctx, days=None):
	finalmessages = []
	await ctx.channel.send(f"Loading messages since the server creation", delete_after=20 * 60)
	to_edit = await ctx.channel.send(".", delete_after=20 * 60)
	server = ctx.guild
	
	preload = False
	filepath = f"files/save_{server.id}.json"

	if (os.path.isfile(filepath)):
		preload = True

	if not preload:
		messages = await server_messages(server, to_edit)
		# we save messages for fast reloading
		with open(filepath, "w") as fmessages:
			json.dump(messages, fmessages)
		finalmessages = messages
		await to_edit.edit(content=f"Done, loaded new data from {len(server.channels)} channels!")
	else:
		await to_edit.edit(content=f"Loading preloaded data...")
		with open(filepath) as jsonfile:
			data = json.load(jsonfile)
			for x in data:
				finalmessages.append((x[0], x[1]))
		await to_edit.edit(content=f"Done, loaded preloaded data: {len(finalmessages)} messages!")
	await add_server(server, finalmessages)
	return finalmessages



@slash.slash(name='cloud', description="Generate a worldcloud with your most recently said words!")
async def cloud(ctx):
	try:
		await ctx.defer()
		server: discord.Guild = ctx.channel.guild
		messages = await load(ctx)

		print(f"TYPE IS ({type(messages)})")

		messages_filtered = [message for message in messages if message[0] == ctx.author.id]

		counter = collections.Counter(messages_filtered)

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
		async with ctx.channel.typing():
			for member in members:
				wc = models[server].word_cloud(member.id)
				if not wc:
					await ctx.send(
						content=f"Sorry I don't have any data on {member.mention} ...",
						allowed_mentions=discord.AllowedMentions.none()
					)
					continue
				result = await make_image.wc_image(
					resolve_tags(server, wc),
					emoji_resolver
				)

				image = result[0]
				text = f"{member.mention}'s Word Cloud: \n"
				for item in counter.most_common(10):
					text += f"**{item[0][1]}** - {item[1]} used \n"
				
				text += f"Checked {len(messages_filtered)} messages"

				await ctx.send(
					content=text, allowed_mentions=discord.AllowedMentions.none(),
					file=discord.File(fp=image, filename=f"{member.display_name}_word_cloud.png")
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
