
### PLANS:

- Spectator: change mode: follow player, free world ; follow other objects also – those which have CameraController attached ;

- Scoreboard : add num of players for each team ? ; highlight local player ;

- console - add option to disable logging stack trace ; filter info, debug, error messages ; stack trace depth level ; collapse ; currently, console does not catch messages logged from Awake() of scripts executed before console ;

- settings menu – restore all settings to default ; 

- flood protection for : chat, commands, 

- auto team balance

- console commands – view log (prints whole log), 

- window manager – create window from existing canvas or panel

- organize Bullet script

- add all scripts to the project, and replace them with the ones from dll ; switch asset serialization mode to text - it will be more git-friendly, and you will be able to see what exactly was changed ;

- build for android

- add more information for teams : description, color, num wins, is visible in scoreboard, - teams should be GameObjects (or not - use dictionary as attributes) - that way you can add features to them

- MapCycle - it should have a list of map infos (info will contain map, picture, description, etc), not separate lists ; it should not store map name, but map path ; these modifications will require to change setup, among other things ;

- server should be able to create windows with options on client (it doesn’t have to be windows, it can be on-screen text) – allows for : team choosing, votemap, votekick, choose weapon, etc

- cvar registration order

- disable *start game* button while the server is starting ; disable *join game* button while connecting ; or just don’t call corresponding functions ;

- disable *end round* button when round system is disabled

- score canvas should be scaled with screen size (too small letters and too large scoreboard on full HD)

- choose-team window is too large on full HD


### BUGS:

- player prefs are not working on linux – randomly saves configuration to disk or adds new cvars


### TIPS:

- many scripts inherit from NetworkBehaviour only to have access to isServer and isClient – they can be easily changed to inherit MonoBehaviour – replace calls to extension methods IsServer() and IsClient() ;

- too many stuffs depend on teams : score drawing, spawn points, round system, player, damage system - it should be part of core


### ACTIVE:

- chat display scroll view should not have scrollbars and should not receive input ?

- generalize settings menu => allows to create menu with parameters anywhere (for example, start game/join game parameters) - ParametersView

- settings menu - add tabs (group settings by tabs)

***

- Player class - it’s work should be splitted – commands ; damage and health should be synced using controllable objects ;

- UI problems – console should be always on top ; when some menu is opened, bring windows to back ? ;

- editor tool for changing theme – check online

- add tooltip prefabs

- LAN : when there are mutliple interfaces, client on the same machine receives data through all of them - try to set MulticastLoopback to false ; set id in broadcast data ;

- player stats are not visible in spectator canvas, but only for the first time when spectator UI is shown

- scoreboard – should we bother with adapting to UI table ? no ; add option to override entry creation (for example, to add buttons)

- editor tool for setting map cycle from build settings - needs testing


<br>
<br>


