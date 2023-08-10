
## Overview

![](pictures/mainpicture.jpg)

UGameCore is a powerful framework for Unity engine that contains core components of what every game should have. It takes care of stuff 'under the hood', allowing you to focus on main parts of the game, like game logic and design.

Built with multiplayer in mind, the framework is modular, extendable, with easy setup.


## Features

- Runtime Console : displays game log and allows custom commands to be entered

- CVars (configurable variables) : They store configuration and can be edited from Console, Settings menu, or command line. Similar to `PlayerPrefs`, but offer more features and customization.

- Settings menu : allows to edit configuration (cvars) in a separate menu, and save/load them to disk. UI is dynamically created from cvars, and doesn't require any manual setup.

- Chat system : Allows the players to send chat messages to the server, and the server to broadcast messages to players. Custom handlers can be registered to process chat messages and, for example, execute commands.

- Spectator system : Allows players to spectate other players while being dead (or being in a Spectator team). With UI controls for navigating between players. Similar to E-Sport games.

- Batch mode : support for running in batch mode (as a dedicated server)

- Scoreboard : simple scoreboard which displays players statistics




- Player management – per player data (e.g nickname), player logins, choosing teams, spawning playing object
- Teams management – every player can belong to a team (select team when joining, or change it later), or be a spectator
- Map cycle – automatic map changing on time interval
- Round system – round ends on time limit or on team victory, causing all players to be respawned
- FFA – free for all mode
- Menu system – hierarchical organization of menus – builtin main menu, start game menu, join  game menu, settings menu, pause menu, ingame menu
- Console commands – can be used for server management, interaction between client and server, etc
- Kill events – UI area for displaying kill events (when one player kills another)
- UI for using all available features
- Demo scene to get you started
- Extremely easy to extend


Framework is fully extendable. Scripts are communicating using broadcasted messages – new scripts can be added to catch these messages and change behaviour. You can detect almost any event in a game by subscribing to C# events.


## Demo

TODO: add WebGL demo


## Support

Non-stop discord support.

