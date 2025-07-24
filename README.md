# My-First-Unity-Game
This is a Unity-based multiplayer game powered by **Netcode for GameObjects (NGO)** and **Unity Transport (UTP)**. It supports both local multiplayer testing and remote client-server connections.

![A Screenshot of my game](https://github.com/titancoder12/My-First-Unity-Game/blob/main/Screen%20Shot%202025-07-21%20at%207.17.42%20PM.png "Screenshot")

## Features

- Multiplayer over LAN or Internet using Unity Transport
- Supports Host, Client, and Dedicated Server modes
- Player movement, shooting, and networked interaction
- Sound effects, synced game state, and more

---

## Requirements

- **Unity 2022.3 LTS** or newer
- Netcode for GameObjects (`com.unity.netcode.gameobjects`)
- Unity Transport (`com.unity.transport`)
- Optional: Multiplayer Tools for diagnostics (`com.unity.multiplayer.tools`)

---

## Installation & Setup

1. **Clone or download this repository**:
   ```
   bash
   git clone https://github.com/titancoder12/My-First-Unity-Game.git
   ```

2. Open the project in Unity Hub
    - Click add and select from disk in the dropdown menu.
      
    - Select (The directory you downloaded into)/My First Unity Game/Prototype 1. Not selecting this directory will yield an error from Unity Hub!
      
    - Open up the project by clicking on the title Prototype 1.
  
    - Do not update the code even if the system prompts you too, as this may break the game.
  
3. Add Scene into Hierachy
    - Go to assets/SCENE/, then click and drag the Scene object 'Playground' into the hierachy. You should now see the enviornment in the scene view.

3. Install required packages

    - Go to Window → Package Manager

    - Add:

        - com.unity.netcode.gameobjects

        - com.unity.transport

        - (Optional) com.unity.multiplayer.tools

4. Play the Game

    - Run the scene in Editor and click Start Host or Start Client

    - Or build the project and run as Host/Client from separate machines

## Hosting a Game Server
To host a dedicated server:
1. Go to ```File -> Build Settings```
2. Select your desired server platform (Linux, etc.)
3. Enable ```Headless Mode```
4. Build your server and run:
```
./MyGameServer.x86_64 -batchmode -nographics
```
### OR...

If you only want to test multiplayer functionality, you can also download the multiplayer play mode package by looking it up in the Unity package manager. This allows you to simulate multiple players without building a server.

## Controls
- WASD to move
- Left click to shoot
- R to reload
- Space to jump

## Links to additional resources
https://docs-multiplayer.unity3d.com/

https://docs.unity3d.com/Packages/com.unity.multiplayer.tools@2.2/manual/index.html


## Credits
Thank you to Unity Netcode for creating their GameObjects library, which was extensively used in this project. 
