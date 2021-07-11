from RepeatedTimer import RepeatedTimer
import numpy as np    
import discord
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

class StatusChanger():

  async def SetStatus(self):
      status = np.random.choice(Statuses)  
      await client.change_presence(activity=discord.Game(name=status))
      print(f"Setting status to: {status}")

  async def Start(self):
    await self.SetStatus()
    RepeatedTimer(10000, self.SetStatus, "Status")