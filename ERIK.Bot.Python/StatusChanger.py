from RepeatedTimer import RepeatedTimer
import numpy as np    
import discord
import time
from loader import client

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
    "Alpha Predators",
    "Goal 2",
    "Sid Meier's Hex Game",
    "Car League",
    "Tom Clancy's: I am long dead",
    "Cold Brass",
    "SWAT 5: The one with magic",
    "Risk of Storm 2",

    "Checkmate, the chessening",
    "Cup of losers",
    "with a berlin sausage",
    "with a german sausage",
    "with a french pastry",
    "with some noodles",
    "with some dumplings",
    "with delaying vital choices"

    "with some volatile compounds in storage",
    "with stability of a region full of war lords",
    "with a whiste",
    "with a toy plane",
    "with a toy train",
    "Nend Sudes",
    "Send Noods",
    "<3",
    "</3",
    "<?3",
    "KLM855",
    "with our privacy policy"
  ]

global tim

class StatusChanger():

  async def SetStatus(self):
      print(f"Attempting to set status to a status")
      status = np.random.choice(Statuses)  
      await client.change_presence(activity=discord.Game(name=status))
      print(f"Setting status to: {status}")

  async def Start(self) -> None:
    print(f"Started status changer")
    while 1:
        await self.SetStatus()
        time.sleep(3600)
