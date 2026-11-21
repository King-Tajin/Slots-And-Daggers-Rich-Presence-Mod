# Discord Presence

Adds Discord Rich Presence to Slots & Daggers.

## Features

- Shows the enemy you are fighting as the large image, with the game logo as a small corner icon and the enemy name on hover
- Shows the current level, or "Arena" in Arena mode
- Shows what you are doing: fighting an enemy, between fights, visiting the shop or in the menus
- Shows your health as a percentage and your score
- Shows elapsed play time and clears the presence when you quit
- Most display options can be turned on or off in the config file

## Requirements

- The Discord desktop app running on the same PC as the game
- "Display current activity as a status message" enabled in Discord under Settings > Activity Privacy

## Installation

Install with a mod manager such as r2modman or the Thunderstore Mod Manager, then launch the game.

Manual install: install BepInEx 5 into the game folder, then copy `DiscordPresence.dll`, `DiscordRPC.dll` and `Newtonsoft.Json.dll` into `BepInEx/plugins/DiscordPresence/`.

## Configuration

Launch the game once to generate the config file at `BepInEx/config/com.king_tajin.discordpresence.cfg`.

| Setting | Default | Description |
| --- | --- | --- |
| ShowEnemyImage | true | Show the art of the enemy you are fighting |
| ShowHealth | true | Show player health as a percentage |
| ShowScore | true | Show the current score |
| PollIntervalSeconds | 2 | How often the game state is checked |
| EnemyLingerSeconds | 8 | How long the last enemy stays shown after a fight ends |
| MinUpdateIntervalSeconds | 5 | Minimum time between presence updates, to stay under Discord rate limits |
| ApplicationId | built in | Discord application ID, only change this if you use your own Discord application |
| LargeImageKey | logo | Art asset key for the logo |

## Source

https://github.com/King-Tajin/Slots-And-Daggers-Rich-Presence-Mod

Licensed under the MIT License.
