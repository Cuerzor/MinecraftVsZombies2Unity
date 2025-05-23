# 0.4.0

## Added
- Added Chapter 4: The Hall of Dreams' Great Mausoleum.
- Added credits section, accessible via the options menu on the main menu.
- Added fast-forward multiplier option.
- Added current version number text to the main menu.
- Pick Starshards while holding blueprints now immediately evokes the placed contraption.
- Added dialogue explanations about artifacts to Halloween Day 7.
- Added the zoom feature to the Misc Almanac.
- Added 6 new Random China events.

## Modified
- Renamed the "Evoke" action in Simplified Chinese.
- Gravity Pads can now pull down certain low-altitude floating enemies.
- TNT now detonates immediately upon being destroyed by fire or explosive damage.
- TNT struck by lightning damage now transforms into Charged TNT.
- Increased the energy cost of Tesla Coil from 175 to 250.
- Reduced the energy cost of Spiked Block from 100 to 75.
- Electric damage now deals 100% damage to ghosts.
- Spark particles from Nightmareaper's Death Wheels now glow.
- Adjusted lighting rendering effects.
- Mind Control Orbs now glows.
- Dropped pickups are no longer affected by darkness.
- Added missing bass instruments to the stage music of Shining Needle Castle.
- Reduced volume of the Seija boss battle music.
- Increased damage of Nightmareaper's Death Wheels by 40% in Hard Mode.
- Added usable blueprints to Stage 1-6: Glowstone, Obsidian, and Punchton.
- Adjusted physics for the Breakout Board in Stage 2-6.
- Added durability descriptions to the prologue's three device entries in the Almanac.
- Added artist credits to character entries in the Almanac.
- Tripled enemy spawn rate on Day 11 of each chapter.
- Added grid patterns to Shining Needle Castle's stage background.
- Changed post-blueprint-selection warnings to display enemy types instead of specific enemy names.
- Rain effects no longer update while the game is paused.
- Adjusted in-game text.

## Fixed
- Fixed issue where Mine TNT's evocation would incorrectly spawn new Mine TNTs in the row above when launching seeds from the bottommost row.
- Fixed crash caused by Nightmareaper attacking invincible contraptions.
- Fixed Anvils failing to deal damage during the instant they hit the ground.
- Fixed performance drops caused by prolonged light source existence.
- Fixed the failure to save-and-exit while stunned enemies were present.
- Fixed untranslated Blueprint names in Stage 2-6.
- Vortex Hopper can no longer be hypnotized using Dream Silk while evocation.
- Fixed issue where Breakout minigame pearls could be launched off-screen.
- Fixed edge case where defeating Seiga Kaku might trigger her Nimble Fabric effect.
- Fixed the issue where javelin throw animation of Totenser still plays while it's hypnotized by Dream Silk.
- Fixed the issue where player can close the menu dialog and continue game bypass restart/exit confirmation dialogs using hotkeys.

---

# 0.3.11

## Changes
* Reduced Ghast's attack interval from 6s to 5s.
* Increased Ghast fire charge damage from 100 to 150 (300 in Hard Mode).
* Gravity Pads are now immune to mind control.
* Seija's magic bomb visual effect now displays half particles (instead of complete disappearance) with particle effects disabled.

## Fixes
* Fixed the issue where skipping 2-6 dialogue by force-quitting causing subsequent levels to remain in normal world instead of Nightmare World.
* Fixed the issue where Dullahans have not damage-state sprites.
* Fixed the issue where the creator credit during startup overlaps with the error dialog when encountering savefile issues.
* Fixed the issue where game is softlocked when destroying specific contraptions by gargoyle statues in Halloween Endless mode.
* Fixed the issue where Gravity Pads slow enemies during sleep status.
* Fixed the issue where sometimes game is softlocked after entering the username.
* Fixed the issue where mouse cursor states are missing on PC version.

---

# 0.3.10

## Modified
* Picking up the clear level pickup now will cancel the holding item.

## Fixed
* Fixed the issue where play Crazy Dave in music room and pause it, then return to main menu, the music will also be paused.
* Fixed the issue where sometimes the game will attempt to set mouse cursor in mobile devices, which may cause the game to crash.

# 0.3.9

## Changes
* When exiting from "Addons" or "Archives" menus, it will return to the basement (instead of upper area).
* Reduced the frequency of error SFX when collecting Starshards.

## Fixes
* Fixed light sources penetrating through UI elements.
* Fixed the issue where sometimes pickaxe and starshard slot won't hide the x mark after Decrepify curse expires.
* Fixed the issue where enemy Drivensers still firing large arrows to the right.
* Fixed shader compilation errors causing crashes on certain devices.
* Fixed the issue where contraption on maps missing shadows.
* Fixed the issue where the Tesla Coil on Shining Needle Castle map is transparent.
* Fixed the issue where contraptions on Shining Needle Castle map blocking view drag.
* Fixed the issue where dead enemies can trigger Mine TNT.
* Fixed the issue where Seija's Cursed Doll move weirdly.
* Fixed the issue where Seija's projectiles are colored incorrectly.
* Fixed the issue where Seija's defeat visual effect is missing.
* Fixed the issue where there's a white square in Almanac.
* Fixed the issue where mobile cursor not instantly updating dual-layer target markers from Stone Shield when moving within the same grid cell.

---

# 0.3.8

## Fixed
* Fixed an issue where 2-6 sometimes cannot be saved and loaded correctly.
* Disabled Vulkan graphic compatiblility, to fix the issue where the game will crash or cause render issues on some devices.

---

# 0.3.7

## Added
- Added a language selection dialog when first entering the game.

## Modified
- Tesla Coils are now glowing contraptions.
- Entering a level now retains the previously selected artifacts.
- Reduced unnecessary graphical settings to optimize game performance.  
- Decreased the detection hitbox of Tesla Coil for items like pickaxe and starshards.
- Improved card selection interactions on mobile devices to prevent accidental taps during view scrolling.  

## Fixed
- Fixed username displaying as "RandmChina" during initial name input.  
- Fixed inverted entity textures on specific devices.  
- Fixed an issue where enemies between two lanes occasionally failed to properly reposition themselves.  
- Fixed an issue where selecting blueprints too quickly before a level could result in incorrect blueprint selection.
- Fixed an issue where when holding a blueprint while The Creature's Heart is present, if a contraption is destroyed causing the blueprint cost to become unaffordable, the held blueprint wouldn't be canceled properly.
- Fixed an issue where white grid markers on the battlefield wouldn't disappear after canceling blueprint selection on mobile devices.
  
---

# 0.3.6

## Modified
- Nightmareaper no longer revive skeletons in the air.

## Fixed
- Fixed the issue where global light does not avaliable on some devices.
- Fixed the issue where entity textures are inverted on some devices.

---

# 0.3.5

## Modified
- Expanded the range to select starshard UI for mobile devices.

## Fixed
- Removed the glowing effects for particles to prevent light source spam which may make memory crash.

---

# 0.3.4

## Modified
- Reworked lighting rendering effects.
- Gargoyle statues no longer destroy Stone Shields when spawning.
- Increased pickup range for pickups on mobile devices from 200% to 250% of the PC version's range.
- Mutant Zombies' actual attack range has been increased by half a grid.
- Skeleton Horses' horizontal movement speed during jumps has been reduced by 28.57%.
- Hell Chariots are now immune to stun effects.
- Stage 3-11 will no longer spawn Glowstones after 4 Glowstones have appeared.

## Fixes
- Fixed an issue where mobile players could not properly use the Force Pad evocation.
- Fixed the bottommost Force Pad teleporting enemies to the first lane.
- Fixed Spiked Blocks not attacking bosses.
- Fixed corrupted stage save files when exiting and saving the game without background music playing.
- Fixed layer display issues for Gargoyles protected by Stone Shields.
- Fixed the "Archive" button on the mainmenu being prioritized over the "Addons" button.
- Fixed rare spawns of certain contraptions in conveyor belt stages due to inaccurate weight randomization.

---

# 0.3.3

## Modified
- Seija's action interval has been increased from 1 second to 3 seconds.
- Contraptions no longer produce energy after the clear pickup is appeared.
- The conveyor belt no longer works after the level is cleared.

## Fixed
- Fixed an issue where enemies won't drop gems nor redstones.

---

# 0.3.2

## Modified
- Reworked Smart Phone artifact:
  - Before: Consumes all energy after level cleared, +10 money for every 10 energy consumed.
  - After: Gain 10 money at the end of the level for every 10 energy you have before the final wave begins.
- Removed the mechanism which contraptions stop producing after the final wave.
- Added Loading text at the starting screen.
  
## Fixed
- Fixed an issue where enemies controlled by Mesmerizers won't drop starshards when left.
- Fixed an issue where Slenderman's Pandora's Box option behaves incorrectly.
- Fixed some incorrect texts.
  
---

# 0.3.1

## Modified
- Nightmareaper's HP: 2500 -> 4000
- Wither's HP: 8000 -> 12000
- HP restorage of Wither's eat: 600 per contraption -> 300 per contraption
- Increased the detection window for achievement "Double Trouble". 

## Fixed
- Fixed an issue where you can still modify screen resolutions at map in Android.
- Fixed an issue where Seija's Cursed Doll will trigger carts.
- Fixed an issue where some texts and images are not translated.
  
---

# 0.3.0

## Added
- Added Chapter 3-Shinning Needle Castle.
- Added artifact system.
- Added Fast-forward function.
- Added repick function.
- Added language pack support.

## Modified
- Carts now can be manually triggered by holding pointer on them.
- Update sprites for almost all contraptions and enemies.
- Redstone count spawned by Furnace's evocation: 12 -> 6
- Magichest's cost: 150 -> 200
- Magichest's recharge: Long -> Very Long
- Lily Pad's evocation: Spawn copies to 4 adjacent directions -> Spawn copies to 8 adjacent directions
- Gravity Pad and Force Pad now can be placed together with a normal contraption.
- Adjusted the motion path of Nightmareaper's "Wheel of Death".
- Removed the random event "Gauntlet Snap" for Random China.
- There will be Mutant Zombies and Mega Mutant Zombies in endless mode.

---
  
# 0.2.5

## Bug Fixed

- Fixed an issue where particles still exist after some entities were destroyed.

---

# 0.2.4

## Bug Fixed

- Fixed an issue that the effect of "Pandora's Box" of Slenderman changed into "clear all contraptions".
- Fixed an issue that would occur an error if a contraption was destroyed when a spider is climbing the contraption.
- Fixed an issue that map view moving and scaling don't work properly on mobile.
- Fixed an issue that you can open settings menu while doing transitions in Nightmare battle.
- Fixed an issue that dialogs after Dream World Day 10 will not be triggered.

---

# 0.2.3

## Modified

- Projectiles go vertically up or down now knockback enemies to the right by default.
- Bone Walls now will not be stunned.
- Now, after other fingers touch the screen, they will also trigger the swing of the sword, but the position of the sword will keep at the first finger.

## Bug Fixed

- Fixed an issue that instantly kill Mother Terrors as soon as they was spawned will spawn Parasite Terrors on the leftest.
- Fixed an issue that Frankenstein will restore from faint immediately after being hurt by high-pressure TNT during punching.
- Fixed an issue that enemies in preview stage will also realign to their lanes.
- Fixed an issue that would occur errors when entering the store and returning when choosing blueprints.
- Fixed an issue that enemies on Gravity Pads will still be slowed after Gravity Pad was disabled.

---

# 0.2.2

## Modified

- Force Pad will now change the direction of projectiles, instead of warp their lanes.

## Bug Fixed

- Fixed an issue that particles created by crushing contraption by statues will continue playing without stop.
- Fixed an issue that when an enemy stops at a Mine TNT, the Mine TNT will not be triggered.
- Fixed an issue that board in Breakout minigame will fall down and sink into water.
- Fixed an issue that caused the cursor to permanently change to the moving cursor after clicks buttons when moving the view of map.
- Fixed an issue that caused the mouse to become target when the Force Pad was destroyed when the Force Pad was evoked.
- Fixed an issue that cannot click enemies to view them in Almanac when choosing blueprints.
- Fixed an issue that items will not appear immediately after entering the store and buy items when choosing blueprints.
- Fixed an issue that you can use dream silk on enemy contraptions.

---

# 0.2.1

## Modified

- Modified the path shape of entities warping lanes by Force Pad.

## Bug Fixed

- Fixed an issue that Doremy will be ordered before Orant in Almanac.
- Fixed an issue that contraptions damage by explosions will take damage again when another explosion created.
- Reduced the height of spice gas, now they can be normally ignited by fire, and can damage low-height contraptions.
- Fixed an issue that Hell Metal event triggered by Random China will equip leather caps for enemies instead of iron helmets.

---

# 0.2.0

## Added
- The whole dream world! Including 10 new contraptions, 5 new enemies and 1 boss!
- A new way to instantly trigger contraptions.
  - If you place a triggerable contraption normally, it will not be triggered and needs to be triggered manually.
  - If you pick up the trigger gear, triggerable contraption blueprints will start twinkling. Then click a contraption blueprint, the cursor will become this contraption and there is a gear icon at the left-top. If you place this contraption, it will be instantly triggered.
  - These operations can be swapped by the option "Swap Trigger".
  - These has been added into the triggering tutorial of 1-7.
- The external location of language files. This means that players can modify the language texts, textures and even add a language.

## Modified
- Soul Furnaces are modified a lot:
  - Capacity: `Burning for 60 seconds, does not consume fuel upon shoots` To `Can shoot 60 shots, only consume fuel upon shoots`
  - Fuel supply formula: `Sacrificed Contraption Cost / 3 * Recharge Multiplier (1 for Short; 3 for Long; 6 for Very Long) + 10` To `Sacrificed Contraption Cost / 6 * Recharge Multiplier (1 for Short; 3 for Long; 6 for Very Long) + 5`
- Modified the minimum aspect of screen to 8:5 in Android Version.
- Changed the version of user save files to 1.
- Enemies with same team will no longer generate useless collisions, greatly increased the performance when lots of enemies appears.
- Leather Hat Zombie now is called Leather Cap Zombie.

## Bug Fixed

- Fixed an issue that if you clear 1-1 to 1-5 without reloading your save file, Orant will not appear in Almanac.
- Fixed an issue that the Vibration setting will be reset to on after you restart the game.
- Fixed an issue that contraption entry buttons in Almanac fills in the first line.
- Fixed an issue that Frankenstein will sometimes stop shooting after destorying his target.

---

# 0.1.2

## Bug Fixed

- **Fixed an issue that saving the level in Endless Mode after each 2 flags is completed will cause this saving fails.**
- Fixed an issue that the data of moonlight sensors' particles and soulfire trails will not be saved.
- Fixed an issue that skeleton entry button in Almanac fills in the first line.
- Fixed an issue that you can still open Menu Dialog using Escape or Back button while the characters appear after you cleared a level.

---

# 0.1.1

## Added

- You can intialize your prefered language after entering the game!
- Game version now will be shown in the mainmenu.
- Changelog in Readme, and the upcoming features! Yeah you are reading one of them!
- The game version will now be displayed at the right-top of mainmenu.

## Modified

- Modified the lighting effects, and it can display a faint light even during the daytime.
- Modified the effect of evoked moonlight sensors, now it will emit stars instead of a blue halo.

## Known Issues

- Level saving sometimes will be failed, causing save files empty and will occur an issue after entering this level.
    - A detector of this issue has been added into the game, and will display an error message box for the player. If you see that, please send the log files to the creator.

## Bug Fixed

- Fixed an issue that if a bone wall is destroyed by a Mine TNT, its bone particles will not be stopped.
- Fixed an issue that you can still pick the pickaxe or starshard up even while they are invisible.
- Fixed an issue that furnaces will not turn to white when they are going to produce, and will become white after being pointed with pickaxe or starshards.
- Fixed an issue that carts will still start their engines if they touched even dead enemies.
- Fixed an issue that the sword will be paralyzed forever if hit Napstablook ran away from the screen in Whack-A-Ghost Minigame.
- Fixed an issue that enemies will have wrong positions after they entering your house in halloween.
- Fixed an issue that you can no longer purchase the 7th contraption slot aftered you cleared 1-7.
- Fixed an issue that resource loading fails if the number format of operation system's culture is different from english (like Russian).

---

# 0.1.0

## Added

- The whole Chapter 1!
- Ghosts, mummies and necromancers!
- Almanac!
- You can open Almanac in-level!
- The saving and loading of level progress!
- Options of Fullscreen, Difficuties, Volumes, Languages and Particle Counts!
- The shortcuts of contraptions!
- Different texts after different enemies entered your house...?
- Tooltips for contraptions!

## Modified

- The screen will be lit after gargoyles are revived now.
- Moved the resource loading to the titlescreen.
- Reduced the frequency of enemy crying.

## Bug Fixed

- Fixed an issue that UI images like "A huge wave of...", "final wave", or the pointing arrow still work while the game is paused.
- Fixed an issue that if you traveled into a scene which will play the traveling sound such as the map, the sound will be played twice. 
- Fixed an issue that sometimes UI will be blocked by the advice box and cannot be interacted.
- Fixed an issue that inputting colored Unicode characters for user name will be recorded, but cannot display normally.
- Fixed an issue that Iron Helmet Zombies will not appear in prologue.
- Fixed an issue that the resolution does not fit the window size.

---

# 0.0.4

## Added

- Halloween Day 2.
- Skeletons! Statues! Gargoyles!
- All evocations of contraptions!

## Modified

- Moved the pickaxe to the right-bottom of the screen in Android Edition.

## Bug Fixed

- Fixed an issue that you can place contraptions while the game is paused.

---

# 0.0.3

## Added

- Halloween Map and Day 1. (Though Day 1 cannot be finished.)
- Menu Button.
- Custom Cursors.
- Expandable resolutions.

## Modified

- Flag Zombies now always walk in the frontest.
- Modified the textures of houses.
- A buzzer sound will be played after clicking a disabled blueprint.

## Bug Fixed

- Fixed an issue that particles will not be stopped after contraptions are destroyed and pause then resume the game.