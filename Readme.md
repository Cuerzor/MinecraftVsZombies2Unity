Language: English | [简体中文](Readme-ZH.md)

- [Overview](#overview)
- [Features](#features)
  - [Energy](#energy)
  - [Contraptions](#contraptions)
  - [Enemies](#enemies)
  - [Gems](#gems)
  - [Battlefields](#battlefields)
  - [Starshards](#starshards)
  - [Artifact](#artifact)
  - [Bosses](#bosses)
  - [Difficulties](#difficulties)
- [FAQ](#faq)
  - [Is This a Bug?](#is-this-a-bug)
  - [How to Report Bugs?](#how-to-report-bugs)
  - [Where Are Game Logs?](#where-are-game-logs)
    - [Windows](#windows)
    - [Android](#android)
  - [Where Are Save Files?](#where-are-save-files)
    - [File Structure](#file-structure)
    - [Windows Path](#windows-path)
    - [Android Path](#android-path)
- [Difficulty Differences](#difficulty-differences)
  - [Easy](#easy)
  - [Hard](#hard)

------

# Overview

**Minecraft vs Zombies 2**(MvZ2) is a fan game of Minecraft, Plants vs Zombies and Touhou Project, it is developed solely by Cuerzor.

The game features mechanics from Plants vs Zombies, elements from Minecraft and Touhou Project’s setting and characters. Players take on the role of Steve the mechanic as he travels to Gensokyo with his friend Crazy Villager to solve the Minecraft incident.

There are two versions of this game. The previous version made with Gamemaker Studio 2 was developed until chapter 4 and has been discontinued, whereas the current version made with Unity is still under development. Currently, it is expected to have around 112 contraptions, 64 enemies and 132 levels.

For news and updates about the game, please follow Cuerzor’s online activity, as well as the game’s [changelog](Changelog.md).

------

# Features

## Energy

**Energy** is a resource used to place contraptions, corresponds to **sun** from Plants vs Zombies.

Without energy, it is impossible to place contraptions. Additional energy can be obtained by collecting Redstone or by using contraptions that produce energy.

You can only have up to 9,990 energy at a time.

## Contraptions

**Contraptions** are the weapons you use to attack enemies, corresponds to **plants** from Plants vs Zombies.

There are many different types of contraptions, such as: 

- **Dispenser** for attacking enemies.
- **Furnace** for producing redstone.
- **Obsidian** for blocking enemies.
- **Mine TNT** for blowing up enemies. 
- etc.

A specified amount of energy must be spent to place a contraption. After you place a contraption, you must wait a set amount of time for the contraption to recharge before placing it again. The time it takes for a contraption to recharge varies.

## Enemies

**Enemies** are what you need to defeat, corresponds to **zombies** from Plants vs. Zombies.

There are various kinds of enemies, such as:

- **Zombie** which is the most common.
- **Iron Helmet Zombie** which is tougher.
- **Skeleton** which shoots arrows to your contraptions.
- etc.

The core of the game is to judiciously use contraptions to defeat enemies.

## Gems

**Gems** are collectable objects, corresponds to **coins** from Plants vs Zombies.

There are three types of gems: 

- **Emeralds** which are worth 10.
- **Rubies** which are worth 50.
- **Diamonds** which are worth 1000.

After collecting enough gems, you can buy and unlock items from the store or from events to increase your power.

## Battlefields

**Battlefields** are where battles take place. Different battlefields have different effects.

For example: 
**Halloween** takes place at night, so you can’t get extra energy from the mine, and at every wave two Gargoyles are summoned to disrupt your formation.

## Starshards

**Starshards** are items capable of shifting the entire battlefield, corresponds to **plant food** from Plants vs Zombies.

When you defeat an enemy that is glowing green, it will drop a starshard. You can also get additional starshards from contraptions that produce them.

After using a starshard on a contraption, the contraption is **evoked**, erupting with enough power to shift the tide of battle.

Different contraptions have different **evocation abilities**. Using starshards wisely can reduce the game’s difficulty.

## Artifact

**Artifacts** are a series of in-game items with passive effects. After unlocking Artifacts, you can equip them during battles to gain enhancements or special effects.

Different Artifacts provide distinct effects. As you progress through the game, you'll unlock more Artifacts, and the number of Artifacts you can equip per level will increase.

## Bosses

**Bosses** are the goal of each chapter in Adventure Mode. You must defeat a chapter’s boss to unlock the next chapter.

Each boss has great power, which is a big challenge for players. However, they can easily be defeated once you figure out their weakness.

## Difficulties

The **difficulties** option is a new system in this game. The game becomes harder or easier depending on the difficulty chosen.

There are three difficulties: 

- **Easy**
- **Normal**
- **Hard**

For the differences between difficulties, see the [Difficulty Differences](#difficulty-differences) section.

------

# FAQ

## Is This a Bug?
Some scenarios are intentional game mechanics rather than bugs. Examples include: 
- Infectenser under mind-control infecting contraptions.
- Reduced glowstone light range caused by the Broken Lantern artifact cant prevent mind-control.

If you're unsure whether a phenomenon is a bug, **search existing Issues** first as similar reports might exist.

If you encounter **game-breaking issues** (crashes/progression blocks), please follow the next section's instructions.

## How to Report Bugs?
Create a new Issue in this repository's Issues page **with the following required information** (missing details will hinder investigation):
- **Game version number**
- **Game logs**
- Detailed description of the issue
- Game state/environment when triggered (e.g. ongoing actions)
- Reproduction steps

Alternatively, email `mvz2feedback@qq.com` with all above details.

## Where Are Game Logs?
Log locations differ between Windows and Android:

### Windows
Path: `%HOMEPATH%\AppData\LocalLow\Cuerzor\MinecraftVSZombies2`  
*Copy-paste this path directly into File Explorer's address bar*  
- `Player.log` (latest session logs)
- `Player-prev.log` (previous session logs)

### Android
Path: `Android/data/com.cuerzor.MinecraftVSZombies2/files/`  
- `mvz2_log.log` (latest session logs)
- `mvz2_log-prev.log` (previous session logs)

**Note:** Some Android devices restrict access to this directory. Use methods like USB connection to access via PC if needed.

## Where Are Save Files?
For v0.3.0+ versions, save structures are cross-platform compatible (Windows/Android interchangeable):

### File Structure
- `users.dat`: User profile names
- `userX.dat`: Save data for slot X
- `xxx.lvl`: Level progress saves

### Windows Path
`%HOMEPATH%\AppData\LocalLow\Cuerzor\MinecraftVSZombies2\userdata`

### Android Path
`Android/data/com.cuerzor.MinecraftVSZombies2/files/userdata`

------

# Difficulty Differences

## Easy

**General**

- The recharge time of blueprints have been reduced to 80%.
- Starshard zombies have increased spawn rate. 
- Clearing a level that has already been cleared only gives 2 rubies, the remaining carts are settled as emeralds.

**Contraptions**

- x150% produce speed for contraptions.
- +1 grid of lightning radius for glowing contraptions.

**Enemies**

- x50% toughness for enemies' armors.
- Enemies drop more redstones in the levels which they will drop restones.
- -1 spawned Parasite Terrors from enemies.
- Skeleton Horses won't jump.

**Bosses**

- Frankenstein will not go into phase 2.
- Slenderman’s Fate Choosing will appear up to 3 times, with 4 choices.
- Nightmareaper's Crushing Wall closes slower.
- Nightmareaper's time limit is 2 minutes.
- Wither won't regenerate health.

**Misc**

- Paralysis time for the sword in Whack-A-Ghost level is 0.75 seconds.


## Hard

**General**

- No carts.
- More enemies will be spawned.
- Clearing a level that has already been cleared gives a diamond, if it has not been cleared 5 rubies will be given instead.

**Enemies**

- x150% attack speed for enemies.
- +1 spawned Parasite Terrors from enemies.
- Skeleton Horses jump twice.

**Bosses**

- Frankenstein starts at phase 2, and move 100% faster.
- Slenderman’s Fate Choosing will appear up to 5 times, with 2 choices.
- Slenderman will turn blueprints into zombies, and zombie blueprints will not be turned into other blueprints.
- Nightmareaper's Crushing Wall closes faster.
- Nightmareaper's time limit is 1 minute.
- Nightmareaper's Wheel of Death attack deals 40% more damage.
- Contraptions damaged by Wither Skulls will be withering, losing 10 HP per second in 30 seconds.

**Misc**

- Paralysis time for the sword in Whack-A-Ghost level is 2 seconds.