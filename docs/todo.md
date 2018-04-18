
### PLANS:

- Spectator: change mode: follow player, free world ; follow other objects also – those which have CameraController attached ;

- Scoreboard : add num of players for each team ? ; highlight local player ;

- console - add option to disable logging stack trace ; filter info, debug, error messages (from gui) ;

- settings menu – restore all settings to default ; 

- flood protection for : chat, commands, 

- auto team balance

- console commands – view log (prints whole log), 

- window manager – create window from existing canvas or panel

- organize Bullet script

- add more information for teams : description, color, num wins, is visible in scoreboard, - teams should be GameObjects - that way you can add features to them

- server should be able to create windows with options on client (it doesn’t have to be windows, it can be on-screen text) – allows for : team choosing, votemap, votekick, choose weapon, etc

- cvar registration order

- disable *start game* button while the server is starting ; disable *join game* button while connecting ; or just don’t call corresponding functions ;

- disable *end round* button when round system is disabled

- score canvas should be scaled with screen size (too small letters and too large scoreboard on full HD)

- choose-team window is too large on full HD

- fix flash which happens when exiting settings menu - possible cause for it is random execution order of scripts - Menu.Update() of the menu which should be opened, is executed 1 frame later, and hence the flash - menus should be enabled/disabled by MenuManager


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

- when cvar is modified, and it's new value is invalid, color of text field is set to red - but it is never restored - it should be done when settings menu is opened

- if FFA is on, spawn player when he logs in, and when scene changes - add separate script FFASpawnPlayer

- teams are not correct in clients’ scoreboard

- ignore network log message : host id {0} has been already deleted

- when starting server, check if online scene is assigned ; when starting client, check if ip address is valid

- scoreboard – should we bother with adapting to UI table ? no ; add option to override entry creation (for example, to add buttons)

- events in NetworkEventsDispatcher should be invoked exception safe

- LAN : test broadcasting on windows ;

- **editor tool for setting map cycle from build settings**

- **add builds on github – for linux, windows and android**


<br>
<br>


