from discord_slash.client import SlashCommand

global slash
global client
global guild_ids

def init(clientitem):
    global slash
    global client
    global guild_ids
    
    slash = SlashCommand(clientitem, sync_commands=True)
    client = clientitem
    guild_ids = [474590032267837471]


