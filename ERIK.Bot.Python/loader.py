from discord_slash.client import SlashCommand

global slash
global client
global guild_ids
global monitoredGuild
global flaggedUsers
global botOwner

def init(clientitem):
    global slash
    global client
    global guild_ids
    global monitoredGuild
    global flaggedUsers
    global botOwner

    slash = SlashCommand(clientitem, sync_commands=True)
    client = clientitem
    guild_ids = [474590032267837471]
    monitoredGuild = [250621419325489153, 474590032267837471]
    flaggedUsers = [260001778186059777, 282213489676648449, 401445057955102721, 289076093888233474, 324026507100028928, 140625547125456896, 136517108661092352, 728532946968903731, 81379701913812992, 434029810214502401, 186960541397286912]
    botOwner = 124928188647211009


