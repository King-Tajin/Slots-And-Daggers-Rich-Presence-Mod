# Discord Presence

A BepInEx 5 plugin that adds Discord Rich Presence to Slots & Daggers.

## Features

- Shows the enemy you are fighting as the large image, with the logo as a small corner icon and the enemy name on hover
- Shows the current level, or "Arena" in Arena mode
- Shows what you are doing: fighting an enemy, between fights, visiting the shop or in the menus
- Shows your health as a percentage and your score
- Shows elapsed play time and clears the presence when you quit
- Most display options can be turned on or off in the config file

## Install

1. Install BepInEx 5 into the game folder.
2. Copy `DiscordPresence.dll`, `DiscordRPC.dll` and `Newtonsoft.Json.dll` into `BepInEx/plugins/DiscordPresence/`.
3. Launch the game.

## Build

1. Create a `libs/` folder next to the `.csproj` and copy in the BepInEx, Harmony, Unity and `Assembly-CSharp` DLLs from your game install.
2. Run `dotnet build -c Release`.
