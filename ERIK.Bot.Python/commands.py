from discord_slash.context import ComponentContext
from loader import *
from discord_slash.utils.manage_commands import create_option
import numpy as np
from discord_slash.utils.manage_components import create_button, create_actionrow
from discord_slash.model import ButtonStyle
from discord_slash.utils.manage_components import wait_for_component, spread_to_rows

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
ApexSentence = ['Have you tried', 'How about', 'Try', 'Maybe', 'Ever tried', '', 'Maybe try', 'Just play',
                'Have you tried pressing Q quicker with', 'Slide more as', 'Abuse the charge tower with', 'Lose your rp with']


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

    @slash.slash(name='ping', description="Shows the bot latency")
    async def ping(ctx):
        await ctx.send(f'Pong! ({client.latency*1000}ms)')

    @slash.slash(name='role', description="Add or remove yourself from a gamerole", guild_ids=guild_ids)
    async def rolesetter(ctx):
        roles = ctx.guild.roles
        rolebuttons = []
        rows = []

        for role in roles:
            button = create_button(style = ButtonStyle.green, label = role.name)
            rolebuttons.append(button)

        # round(rolebuttons.count / 5)
        rows = spread_to_rows(*rolebuttons)

        await ctx.send("Choose the role you want to change", components=rows)
        
        button_ctx: ComponentContext = await wait_for_component(client, components=rows)
        await button_ctx.origin_message.add_reaction("🤔")

        chosenButton = next(rolebutton for rolebutton in rolebuttons if rolebutton["custom_id"] == button_ctx.custom_id )
        chosenRole = next(role for role in roles if role.name == chosenButton["label"])
        interacter = button_ctx.author

        print(f"{interacter.display_name} wants to change role {chosenRole.name}")
        await interacter.add_roles(chosenRole, reason= "Requested through the slash command /role")
        await button_ctx.origin_message.add_reaction("✅")
        

    @slash.slash(name='apex', description="Returns a random apex character for you to play.")
    async def ping(ctx):
        char = np.random.choice(ApexCharacters)
        sent = np.random.choice(ApexSentence)
        await ctx.send(f'{str(sent)} **{str(char)}**?')

