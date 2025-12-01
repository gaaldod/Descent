# Descent

A survival horror game built with Unity (URP).

## Description

Descent is a first-person survival horror game where players must navigate through a dark environment, collect items, and avoid a hostile enemy.

## Features

- **First-Person Survival Horror Gameplay**: Navigate through dark environments with limited resources
- **Inventory System**: Collect and manage items including keys, notes, and consumables
- **Enemy AI**: Dynamic enemy behavior with patrol routes, line-of-sight detection, and chase mechanics
- **Hiding Mechanics**: Hide under objects to avoid detection
- **Save System**: Save your progress at designated save stations
- **Interactive Objects**: Doors, generators, and other environmental interactions
- **Note System**: Discover and read notes to uncover the story
- **Multiple Scenes**: Navigate through different areas including attic, main house, and basement

## Controls

- **WASD**: Move
- **Mouse**: Look around
- **E**: Interact with objects / Pick up items / Open doors
- **I**: Open/Close inventory
- **SHIFT**: Run
- **CTRL**: Crouch
- **Space**: Skip intro text / Continue dialogue
- **1**: Load last save (death screen)
- **2**: Return to main menu (death screen)

## Installation

1. Clone or download this repository
2. Open the project in Unity 6.0 or later
3. Ensure Universal Render Pipeline (URP) is installed
4. Open the scene you want to play (e.g., 'mainMenuScene' or 'preIntroScene' for the full game)
5. Press Play in the Unity Editor

## Requirements

- Unity 6.0 LTS or later
- Universal Render Pipeline (URP)

## Screenshots

<!-- Screenshots will be added here -->
<!-- 
![Game Screenshot 1](screenshots/screenshot1.png)
![Game Screenshot 2](screenshots/screenshot2.png)
![Game Screenshot 3](screenshots/screenshot3.png)
-->

## Technical Details

- **Engine**: Unity 6.0
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Inventory System**: Devion Games Item & Inventory System
- **AI**: Unity NavMesh System
- **Save Format**: JSON

## Credits

### Assets Used
- Devion Games - Item & Inventory System
- Modular Horror House 3D Model Pack
- Additional assets as credited in credits.txt

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**Note**: This project uses third-party assets with their own licenses:
- Devion Games Item & Inventory System - Check their license terms
- Modular Horror House 3D Model Pack - Check the asset's license terms
- Other third-party assets - See credits.txt for details

Please ensure you comply with all third-party asset licenses when using or distributing this project.

## Known Issues

- Enemy AI may occasionally get stuck near NavMesh edges (improvements implemented)
- Some UI elements may need adjustment for different screen resolutions


## Future Improvements

- Expanded furniture variation inside the building
- Better saving system
- Expanded story and lore
- Performance optimizations


