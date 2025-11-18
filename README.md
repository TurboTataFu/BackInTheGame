# Monobehaviour

BackInTheGame Version 1.0 - Old Version

TataFu/AshMatsuzaka 2025

This project contains a lot of obsolete code for various reasons and lacks a save function. You are free to modify and use it in your projects.

Below is a simple project overview:

Folders named V1 and V2 likely contain code that is still under maintenance. For example, EnterMessage.cs, located in Chat/V1/V1.5,

is based on code in Chat/V1. It is used to receive information transmitted after "ContactInformation.cs" is clicked in the chat dialogue function, then search for the JSON chat file and icon in the Resources folder, and instantiate a message object to implement "sending information according to the JSON script". It has the function of linking files A and B. ContactMemories.cs can remember which Contact contact you selected.

Your Resources chat JSON file will look something like this:

    "dialogues": [
    {
    "speaker": "MatsuzakaKumi", //Name

    "speakerAvatar": "KumiIcon", //Character portrait or avatar image name

    "content": "Good morning!", //Speech content

    "eventOnDisplay": "", //Under improvement

    "eventOnFinish": "", //Under improvement

    "autoPlayNext": false, //Under improvement, keep it false

    "textSpeed": 1.0 //Comment, the Chat system in the V1.5 folder has streaming text transmission; the smaller the TextSpeed ​​number, the faster the transmission speed

    },

Player/V1 contains scripts that may be used in first-person games, including movement scripts and vehicle control scripts, but their use is not recommended. These are junk code, buggy, and abandoned for maintenance.

The code in Player/V2 is related to building; currently, it only has placement functionality.

The code in Player/V1/BagManager is what I am currently maintaining. It can freely adjust the size of items (e.g., 2x1, 3x4, etc.) and has

material crafting logic (this is ancient code; I only recently started maintaining it again).

    { `inventoryCell.cs` is used for each cell, but it's not strictly encapsulated. `SetState(state, InventoryItemComponent.cs)` sets the new state and the new bound item.

    `ItemDragger` manages item drag events.

    `InventoryGrid.cs` is ancient code; I've forgotten most of it. It has basic functions for placing, removing, and initializing split inventory cells.

    `InventoryItemComponent.cs` stores all item information.

    `InventoryItem` is a class that inherits from `scriptableObject` and stores fixed data for the item.

    `ItemPlacer` is very important; this is the item placement script I recently rewrote. It contains a lot of debugging information. It's used to place items in the inventory and occupy inventory cells.

    `ItemFoundManager` can find items in your inventory and combine them into a dictionary `Dict`.

    `CraftManager` is very important; it carries the main item crafting logic. It can consume the quantity of items and place crafted items in the inventory.


The contents of the `Chat/V1` folder can be used for picture novels (like those in Galgames). I may expand its functionality again.

The code in the Chat/V1/V1.5 folder can be used for WeChat-style text dialogue. I am currently debugging and improving it (the current version may have shortcomings).

NPC/V1/AiWayPoint/NPCWander.cs is a simple NPC roaming script. The NPC will randomly find a target point and go there. It has logic to automatically check if the NPC encounters obstacles.

ObjTreeSystem/TreeLODManager.cs is abandoned and is a legacy from an old project. It can generate tree meshes and custom collision boxes at the location of empty objects (it has bugs and is not recommended for use).

Map\V1\Cloude is a simple cloud generator. It instantiates prefab image clouds through parameters. The clouds will be generated at high altitudes and follow the player. It is currently abandoned (simple and usable functionality).
