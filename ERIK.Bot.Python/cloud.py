import collections
from enum import auto
import os

from PIL import Image
from discord_slash.utils.manage_components import create_actionrow, create_button
from preprocessing import resolve_tags
import sys
from typing import Dict, List
import json
from wordcloud import WordCloud

from discord_slash.context import ComponentContext
from discord_slash.model import ButtonStyle, SlashCommandOptionType
from discord_slash.utils.manage_commands import create_option

from loader import *
from datetime import datetime, timedelta
from tqdm import tqdm
from tqdm.asyncio import tqdm_asyncio
from discord.ext.commands import NoPrivateMessage
import traceback
from loader import *
import discord
import numpy as np

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


async def load(ctx, authorid, to_edit):
	finalmessages = []
	await to_edit.edit(content=f"Loading messages since the server creation")
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


@slash.slash(name='cloud', description="Generate a worldcloud with your most recently said words!",
			 options=[
				 create_option(
					 name="includecommands",
					 description="Include used commands?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="includefullsentences",
					 description="Check for entire sentences in messages instead of words? Enables ALL other includes",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="mostcommonsize",
					 description="How many words should be displayed in the text?",
					 option_type=SlashCommandOptionType.INTEGER,
					 required=False,
				 ),
				 create_option(
					 name="includeconnectives",
					 description="Include connectives(and or etc)?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="includearticles",
					 description="Include articles(a an the)?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="includecommonpronouns",
					 description="Include the most common pronouns(they, she, he)?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="includeprepositions",
					 description="Include the prepositions(after, above, after, etc)?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="includedemonstratives",
					 description="Include demonstratives(this, that)?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 ),
				 create_option(
					 name="includeverbs",
					 description="Include common verbs(be, is, bid, bite, etc)?",
					 option_type=SlashCommandOptionType.BOOLEAN,
					 required=False,
				 )
			 ]
			 )
async def cloud(ctx: ComponentContext, includecommands=False, includefullsentences=False, mostcommonsize=15, includeconnectives=False, includearticles=False, includecommonpronouns=False, includeprepositions=False, includedemonstratives=False, includeverbs = False):
	try:
		await ctx.defer()
		print(f"A request has been made to generate a wordcloud by {ctx.author.mention}")
		async with ctx.channel.typing():

			server: discord.Guild = ctx.channel.guild
			to_edit = await ctx.send("Starting data retrieval...")
			messages = await load(ctx, ctx.author.id, to_edit)

			await to_edit.edit(content="All required data retrieved, processing...")

			excludeConnectives = ["and", "also", "besides", "further", "furthermore", "too", "moreover", "in addition", "then", "of equal importance", "equally important", "another,next", "afterward", "finally", "later", "last", "lastly", "at last", "now", "subsequently", "then", "when", "soon", "thereafter", "after a short time", "the next week (month", "day", "etc.)", "a minute later", "in the meantime", "meanwhile", "on the following day", "at length", "ultimately", "presently,first", "second", "(etc.)", "finally", "hence", "next", "then", "from here on", "to begin with", "last of all", "after", "before", "as soon as", "in the end", "gradually,above", "behind", "below", "beyond", "here", "there", "to the right (left)", "nearby", "opposite", "on the other side", "in the background", "directly ahead", "along the wall", "as you turn right",
																																																																																								"at the top", "across the hall", "at this point", "adjacent to,for example", "to illustrate", "for instance", "to be specific", "such as", "moreover", "furthermore", "just as important", "similarly", "in the same way,as a result", "hence", "so", "accordingly", "as a consequence", "consequently", "thus", "since", "therefore", "for this reason", "because of this,to this end", "for this purpose", "with this in mind", "for this reason(s),like", "in the same manner (way)", "as so", "similarly,but", "in contrast", "conversely", "however", "still", "nevertheless", "nonetheless", "yet", "and yet", "on the other hand", "on the contrary", "or", "in spite of this", "actually", "in fact,in summary", "to sum up", "to repeat", "briefly", "in short", "finally", "on the whole", "therefore", "as I have said", "in conclusion", "as you can see"]
			excludeCommands = ["!", "?", "/", "$", ";", ":", ">", "~", "1", "."]
			excludeArticles = ["a", "an", "the"]
			excludeCommonPronouns = ["he", "she", "i", "you", "they", "it", "me", "her", "him", "us", "you", "them", "us", "your", "yours", "ours", "hers", "his", "its", "our", "theirs", "i'm", "am"]
			excludePrepositions = ["above", "across", "after", "at", "around", "before", "behind", "below", "beside", "between", "by", "down", "during", "for", "from", "in", "inside", "onto", "of", "off", "on", "out", "through", "to", "under", "up", "with"]
			excludeDemonstratives = [ "this", "that" ]
			excludeVerbs = ["be", "is", "am", "are", "nwas", "were", "been", "beat", "beat", "beaten", "become", "became", "become", "begin", "began", "begun", "bend", "bent", "bent", "bet", "bet", "bet", "bid", "bid", "bid", "bite", "bit", "bitten", "blow", "blew", "blown", "break", "broke", "broken", "bring", "brought", "brought", "build", "built", "built", "burn", "burned", "burnt", "burned", "burnt", "buy", "bought", "bought", "catch", "caught", "caught", "choose", "chose", "chosen", "come", "came", "come", "cost", "cost", "cost", "cut", "cut", "cut", "dig", "dug", "dug", "dive", "dove", "dived", "do", "did", "done", "draw", "drew", "drawn", "dream", "dreamed", "dreamt", "dreamed", "dreamt", "drive", "drove", "driven", "drink", "drank", "drunk", "eat", "ate", "eaten", "fall", "fell", "fallen", "feel", "felt", "felt", "fight", "fought", "fought", "find", "found", "found", "fly", "flew", "flown", "forget", "forgot", "forgotten", "forgive", "forgave", "forgiven", "freeze", "froze", "frozen", "get", "got", "gotten", "give", "gave", "given", "go", "went", "gone", "grow", "grew", "grown", "hang", "hung", "hung", "have", "had", "had", "hear", "heard", "heard", "hide", "hid", "hidden", "hit", "hit", "hit", "hold", "held", "held", "hurt", "hurt", "hurt", "keep", "kept", "kept", "know", "knew", "known", "lay", "laid", "laid", "lead", "led", "led", "leave", "left", "left", "lend", "lent", "lent", "let", "let", "let", "lie", "lay", "lain", "lose", "lost", "lost", "make", "made", "made", "mean", "meant", "meant", "meet", "met", "met", "pay", "paid", "paid", "put", "put", "put", "read", "read", "read", "ride", "rode", "ridden", "ring", "rang", "rung", "rise", "rose", "risen", "run", "ran", "run", "say", "said", "said", "see", "saw", "seen", "sell", "sold", "sold", "send", "sent", "sent", "show", "showed", "shown", "shut", "shut", "shut", "sing", "sang", "sung", "sit", "sat", "sat", "sleep", "slept", "slept", "speak", "spoke", "spoken", "spend", "spent", "spent", "stand", "stood", "stood", "swim", "swam", "swum", "take", "took", "taken", "teach", "taught", "taught", "tear", "tore", "torn", "tell", "told", "told", "think", "thought", "thought", "throw", "threw", "thrown", "understand", "understood", "understood", "wake", "woke", "woken", "wear", "wore", "worn", "win", "won", "won", "write", "wrote", "written"]

			messages = list(map(str.lower, messages))
			messages = resolve_tags(server, messages)

			if not includefullsentences:
				newMessages = []
				for message in messages:
					for x in message.split():
						newMessages.append(x)
				messages = newMessages
			else:
				includeconnectives = True
				includeconnectives = True
				includearticles = True
				includecommonpronouns = True
				includeprepositions = True
				includedemonstratives = True
				includeverbs = True

			if not includecommands:
				messages = [elem for elem in messages if elem[:1]not in excludeCommands and len(elem) < 8]


			if not includeconnectives:
				messages = [
					elem for elem in messages if elem not in excludeConnectives]

			if not includearticles:
				messages = [
					elem for elem in messages if elem not in excludeArticles]

			if not includecommonpronouns:
				messages = [
					elem for elem in messages if elem not in excludeCommonPronouns]

			if not includeprepositions:
				messages = [
					elem for elem in messages if elem not in excludePrepositions]

			if not includedemonstratives:
				messages = [
					elem for elem in messages if elem not in excludeDemonstratives]

			if not includeverbs:
				messages = [
					elem for elem in messages if elem not in excludeVerbs]

			counter = collections.Counter(messages)

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

			member = ctx.author
			# image = result[0]
			text = f"{member.mention}'s Word cloud: \n"
			for item in counter.most_common(mostcommonsize):
				itemtext = item[0]
				if len(itemtext) > 13:
					itemtext= itemtext[:10] + "..."
				text += f"[{itemtext}] - {item[1]} times used \n"

			text += f"Checked {len(messages)} items for this user"

			mask = np.array(Image.open("tree.png"))

			await to_edit.edit(content="All required data retrieved and processed, generating wordcloud...")

			wordcloud = WordCloud(scale=5, max_words=2000, mask=mask)
			wordcloud.generate_from_frequencies(counter)

			await to_edit.edit(content="Wordcloud generated, generating image...")

			filepath = f"images\{member.id}_word_cloud.png"
			wordcloud.to_file(filepath)
			
			buttons = [ create_button(style = ButtonStyle.green, label = "Want your image sent to your DM's?", custom_id = "dmcloudimage") ]
			action_row = create_actionrow(*buttons)

			with open(filepath, 'rb') as fp:
				await ctx.send(
					content=text, allowed_mentions=discord.AllowedMentions.none(),
					file=discord.File(
						fp, filename=f"{member.mention}_word_cloud.png"),
						components=[action_row]
				)

	except:
		print("FAILED CLOUD ERROR:", sys.exc_info()[0])
		await ctx.send(content="Failed to create a worldcloud for you :(")
		raise

@slash.component_callback()
async def dmcloudimage(ctx: ComponentContext):
	await ctx.author.send(content="Retrieving your image...")

	filepath = f"images\{ctx.author.id}_word_cloud.png"

	if os.path.isfile(filepath):
		with open(filepath, 'rb') as fp:
			await ctx.author.send(
				content="Here is your latest worldcloud!",
				file=discord.File(fp, filename=f"{ctx.author.id}_word_cloud.png"))
	else:
		await ctx.author.send(content="Sorry, couldn't find a image that is connected to your user. Have you run /cloud? If not, run it in the desired guild.")

@cloud.error
async def cloud_error(ctx, error):
	if isinstance(error, NoPrivateMessage):
		await ctx.channel.send("This command can only be used in a server channel")
	else:
		traceback.print_exc()
