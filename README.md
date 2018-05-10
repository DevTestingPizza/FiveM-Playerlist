# FiveM-Playerlist
GTA:O styled playerlist for FiveM servers (future proof: supports > 32 players).

### Preview:

![](https://www.vespura.com/hi/i/2018-05-10_20-49_%25pn_2847cbc57cd7dffc8f.png)

### Features:

- Configurable rows through a server event or export.
- Configured rows update live as soon as the event/export is called, even if someone has the playerlist open at that time.
- GTA:O scaleform used, so it looks exactly like GTA:O.
- Future proof, it supports more than 32 online players.
- "Max Players" indicator updates with the "sv_maxClients" convar, so no need to configure this manually.

### Installation

Download the latest version, drag the folder from the zip file into your resources folder and add `start playerlist` to your server.cfg file.


### Configuration

There is no config file to change any of the "visual" settings for player rows. You will have to create your own script to modify the rows through the playerlist api.


### Developer info

To change the player's row settings, trigger this server event:
```lua
TriggerEvent("fs:setPlayerRowConfig", 1, "SNAIL", 50, true)
```


#### Parameters

|type|name|description|
|:-|:-|:-|
|_int_|**playerServerId**|This is the player's server id for the player you want to change the row of.|
|_string_|**crewText**|The text to display for the "crew" tag behind the player's username. Pass an empty string (`""`) to disable the crew label.|
|_int_|**jobPointsAmount**|The number to display for the "job points (jp)" value. Set to -1 to disable.|
|_bool_|**showJobPointsIcon**|Should the "(JP)" icon be visible next to the job points number?|


You can access this event from both C# or Lua scripts. By default the crew tag, job points amount and job points logo are all hidden for all players, only if you add them using the event will it make them visible for that specific player row. (syncing for all clients is managed by the resource)


You can also use the provided server export (`setPlayerRowConfig()`) however, due to some issues (possibly a bug with FiveM) this is kind of buggy now. Use the event for now, once I figure out why some parameters are not getting passed on when using the export I'll make sure to add documentation for the server export.


### Download / Source Code

Download the resource on [GitHub](https://github.com/TomGrobbe/FiveM-Playerlist). Make sure to go to the "releases" page and download the latest release, don't download the repository as that's useless if you don't know how to use visual studio or don't want to edit the resource.


### Usage in-game

When in-game, press "Z" to open the first page, press "Z" again to go to the next page (just like the playerlist in GTA:O). If you're at the last page, pressing "Z" will close the playerlist. If the playerlist is open and you don't close it yourself, then it will auto-close after a couple of seconds.
For controller support, use DPAD-DOWN.

Note, if other resources on your server disable the "Z" key or the "DPAD-DOWN" (`INPUT_MULTIPLAYER_INFO` / `20`) control, then you won't be able to toggle the playerlist.
