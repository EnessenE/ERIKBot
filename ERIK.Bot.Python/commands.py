import time
from discord_slash.context import ComponentContext
from loader import *
from discord_slash.utils.manage_commands import create_option
import numpy as np
from discord_slash.model import ButtonStyle, SlashCommandPermissionType
from discord_slash.utils.manage_components import spread_to_rows, create_button
import math

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
    'Valkyrie',
    'Seeker'
]
ApexSentence = ['Have you tried', 'How about', 'Try', 'Maybe', 'Ever tried', 'Hmmm, ', 'Maybe try', 'Just play',
                'Have you tried pressing Q quicker with', 'Slide more as', 'Abuse the charge tower with', 'Lose your rp with', 'Complain about balancing with',
                'Get oneshot with', 'Lose with', 'Win with', 'Go back to the lobby with', 'Rant about playing with masters because of poor matchmaking with',
                'Misstime your ultimate with', "Thought about", "Get spotted through a wall with"]

rolebuttons = []

class Commands():
    def __init__(self):
        print("init")

    @slash.slash(name='test',
                 description='This is just a test command, nothing more.',
                 options=[
                     create_option(
                         name='optone',
                         description='This is the first option we have.',
                         option_type=3,
                         required=True
                     )
                 ])
    async def test(ctx, optone: str):
        await ctx.send(content=f'I got you, you said {optone}!')

    @slash.slash(name='age', description="How long have you been here?")
    async def age(ctx:ComponentContext):
        await ctx.send(content=f"You have joined <t:{math.floor(time.mktime(ctx.author.joined_at.timetuple()))}:R>. Since {ctx.author.joined_at} exactly.")
   
    @slash.slash(name='ping', description="Shows the bot latency")
    async def ping(ctx):
        await ctx.send(f'Pong! ({client.latency*1000} ms)')

    @slash.slash(name='role', description="Add or remove yourself from a gamerole")
    async def rolesetter(ctx):
        roles = ctx.guild.roles
        rows = []
        tempButtons = []

        rolecount = 0
        for role in roles:
            rolecount+=1
            if rolecount < 25 and "?!" in role.name:
                button = create_button(style = ButtonStyle.green, label = role.name)
                rolebuttons.append(button)
                tempButtons.append(button)

        rows = spread_to_rows(*tempButtons)

        await ctx.send("Choose the role you want to change", components=rows)
        
    async def HandleInteraction(self, button_ctx: ComponentContext):
        # button_ctx: ComponentContext = await wait_for_component(client, components=rows)
        await button_ctx.defer()

        # await button_ctx.origin_message.add_reaction("🤔")

        roles = button_ctx.guild.roles
        try:
            chosenButton = next(rolebutton for rolebutton in rolebuttons if rolebutton["custom_id"] == button_ctx.custom_id )
        except:
            chosenButton = None

        if chosenButton == None:
            await button_ctx.origin_message.delete()
            await button_ctx.send("Please re-execute this command. This message has expired", hidden = True)
        else:
            chosenRole = next(role for role in roles if role.name == chosenButton["label"])
            interacter = button_ctx.author
            reasoning = "Requested through the slash command /role"

            print(f"{interacter.display_name} wants to change role {chosenRole.name}")
            text = "??"
            if (chosenRole in interacter.roles):
                await interacter.remove_roles(chosenRole, reason = reasoning)
                text = f"{interacter.display_name} left {chosenRole.name} ✅"
            else:
                await interacter.add_roles(chosenRole, reason= reasoning)
                text = f"{interacter.display_name} joined {chosenRole.name} ✅"
            await button_ctx.send(text, hidden = True)

    @slash.slash(name='apex', description="Returns a random apex character for you to play.")
    async def ping(ctx):
        char = np.random.choice(ApexCharacters)
        sent = np.random.choice(ApexSentence)
        await ctx.send(f'{str(sent)} **{str(char)}**?')
