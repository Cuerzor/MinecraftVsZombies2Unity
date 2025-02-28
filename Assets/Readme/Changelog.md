# Changelog

## 0.3.0

**Added**
* Added Chapter 3-Shinning Needle Castle.
* Added artifact system.
* Added Fast-forward function.
* Added repick function.
* Added language pack support.

**Modified**
* Carts now can be manually triggered by holding pointer on them.
* Update sprites for almost all contraptions and enemies.
* Redstone count spawned by Furnace's evocation: 12 -> 6
* Magichest's cost: 150 -> 200
* Magichest's recharge: Long -> Very Long
* Lily Pad's evocation: Spawn copies to 4 adjacent directions -> Spawn copies to 8 adjacent directions
* Gravity Pad and Force Pad now can be placed together with a normal contraption.
* Adjusted the motion path of Nightmareaper's "Wheel of Death".
* Removed the random event "Gauntlet Snap" for Random China.
* There will be Mutant Zombies and Mega Mutant Zombies in endless mode.

-----
  
## 0.2.5

**Bug Fixed**

* Fixed an issue where particles still exist after some entities were destroyed.

-----

## 0.2.4

**Bug Fixed**

* Fixed an issue that the effect of "Pandora's Box" of Slenderman changed into "clear all contraptions".
* Fixed an issue that would occur an error if a contraption was destroyed when a spider is climbing the contraption.
* Fixed an issue that map view moving and scaling don't work properly on mobile.
* Fixed an issue that you can open settings menu while doing transitions in Nightmare battle.
* Fixed an issue that dialogs after Dream World Day 10 will not be triggered.

-----

## 0.2.3

**Modified**

* Projectiles go vertically up or down now knockback enemies to the right by default.
* Bone Walls now will not be stunned.
* Now, after other fingers touch the screen, they will also trigger the swing of the sword, but the position of the sword will keep at the first finger.

**Bug Fixed**

* Fixed an issue that instantly kill Mother Terrors as soon as they was spawned will spawn Parasite Terrors on the leftest.
* Fixed an issue that Frankenstein will restore from faint immediately after being hurt by high-pressure TNT during punching.
* Fixed an issue that enemies in preview stage will also realign to their lanes.
* Fixed an issue that would occur errors when entering the store and returning when choosing blueprints.
* Fixed an issue that enemies on Gravity Pads will still be slowed after Gravity Pad was disabled.

-----

## 0.2.2

**Modified**

* Force Pad will now change the direction of projectiles, instead of warp their lanes.

**Bug Fixed**

* Fixed an issue that particles created by crushing contraption by statues will continue playing without stop.
* Fixed an issue that when an enemy stops at a Mine TNT, the Mine TNT will not be triggered.
* Fixed an issue that board in Breakout minigame will fall down and sink into water.
* Fixed an issue that caused the cursor to permanently change to the moving cursor after clicks buttons when moving the view of map.
* Fixed an issue that caused the mouse to become target when the Force Pad was destroyed when the Force Pad was evoked.
* Fixed an issue that cannot click enemies to view them in Almanac when choosing blueprints.
* Fixed an issue that items will not appear immediately after entering the store and buy items when choosing blueprints.
* Fixed an issue that you can use dream silk on enemy contraptions.

-----

## 0.2.1

**Modified**

* Modified the path shape of entities warping lanes by Force Pad.

**Bug Fixed**

* Fixed an issue that Doremy will be ordered before Orant in Almanac.
* Fixed an issue that contraptions damage by explosions will take damage again when another explosion created.
* Reduced the height of spice gas, now they can be normally ignited by fire, and can damage low-height contraptions.
* Fixed an issue that Hell Metal event triggered by Random China will equip leather caps for enemies instead of iron helmets.


-----

## 0.2.0

**Added**
* The whole dream world! Including 10 new contraptions, 5 new enemies and 1 boss!
* A new way to instantly trigger contraptions.
  * If you place a triggerable contraption normally, it will not be triggered and needs to be triggered manually.
  * If you pick up the trigger gear, triggerable contraption blueprints will start twinkling. Then click a contraption blueprint, the cursor will become this contraption and there is a gear icon at the left-top. If you place this contraption, it will be instantly triggered.
  * These operations can be swapped by the option "Swap Trigger".
  * These has been added into the triggering tutorial of 1-7.
* The external location of language files. This means that players can modify the language texts, textures and even add a language.

**Modified**
* Soul Furnaces are modified a lot:
  * Capacity: 
  		Burning for 60 seconds, does not consume fuel upon shoots
		To
  		Can shoot 60 shots, only consume fuel upon shoots
  * Fuel supply formula: 
  		Sacrificed Contraption Cost / 3 * Recharge Multiplier (1 for Short; 3 for Long; 6 for Very Long) + 10
		To
		Sacrificed Contraption Cost / 6 * Recharge Multiplier (1 for Short; 3 for Long; 6 for Very Long) + 5
* Modified the minimum aspect of screen to 8:5 in Android Version.
* Changed the version of user save files to 1.
* Enemies with same team will no longer generate useless collisions, greatly increased the performance when lots of enemies appears.
* Leather Hat Zombie now is called Leather Cap Zombie.

**Bug Fixed**
* Fixed an issue that if you clear 1-1 to 1-5 without reloading your save file, Orant will not appear in Almanac.
* Fixed an issue that the Vibration setting will be reset to on after you restart the game.
* Fixed an issue that contraption entry buttons in Almanac fills in the first line.
* Fixed an issue that Frankenstein will sometimes stop shooting after destorying his target.

## 0.1.2

**Bug Fixed**

* **Fixed an issue that saving the level in Endless Mode after each 2 flags is completed will cause this saving fails.**
* Fixed an issue that the data of moonlight sensors' particles and soulfire trails will not be saved.
* Fixed an issue that skeleton entry button in Almanac fills in the first line.
* Fixed an issue that you can still open Menu Dialog using Escape or Back button while the characters appear after you cleared a level.

## 0.1.1

**Added**

* You can intialize your prefered language after entering the game!
* Game version now will be shown in the mainmenu.
* Changelog in Readme, and the upcoming features! Yeah you are reading one of them!
* The game version will now be displayed at the right-top of mainmenu.

**Modified**

* Modified the lighting effects, and it can display a faint light even during the daytime.
* Modified the effect of evoked moonlight sensors, now it will emit stars instead of a blue halo.

**Known Issues**

* * Level saving sometimes will be failed, causing save files empty and will occur an issue after entering this level.
    * A detector of this issue has been added into the game, and will display an error message box for the player. If you see that, please send the log files to the creator.(See [FAQ](#FAQ))

**Bug Fixed**

* Fixed an issue that if a bone wall is destroyed by a Mine TNT, its bone particles will not be stopped.
* Fixed an issue that you can still pick the pickaxe or starshard up even while they are invisible.
* Fixed an issue that furnaces will not turn to white when they are going to produce, and will become white after being pointed with pickaxe or starshards.
* Fixed an issue that carts will still start their engines if they touched even dead enemies.
* Fixed an issue that the sword will be paralyzed forever if hit Napstablook ran away from the screen in Whack-A-Ghost Minigame.
* Fixed an issue that enemies will have wrong positions after they entering your house in halloween.
* Fixed an issue that you can no longer purchase the 7th contraption slot aftered you cleared 1-7.
* Fixed an issue that resource loading fails if the number format of operation system's culture is different from english (like Russian).

## 0.1.0

**Added**

* The whole Chapter 1!
* Ghosts, mummies and necromancers!
* Almanac!
* You can open Almanac in-level!
* The saving and loading of level progress!
* Options of Fullscreen, Difficuties, Volumes, Languages and Particle Counts!
* The shortcuts of contraptions!
* Different texts after different enemies entered your house...?
* Tooltips for contraptions!

**Modified**

* The screen will be lit after gargoyles are revived now.
* Moved the resource loading to the titlescreen.
* Reduced the frequency of enemy crying.

**Bug Fixed**

* Fixed an issue that UI images like "A huge wave of...", "final wave", or the pointing arrow still work while the game is paused.
* Fixed an issue that if you traveled into a scene which will play the traveling sound such as the map, the sound will be played twice. 
* Fixed an issue that sometimes UI will be blocked by the advice box and cannot be interacted.
* Fixed an issue that inputting colored Unicode characters for user name will be recorded, but cannot display normally.
* Fixed an issue that Iron Helmet Zombies will not appear in prologue.
* Fixed an issue that the resolution does not fit the window size.


## 0.0.4

**Added**

* Halloween Day 2.
* Skeletons! Statues! Gargoyles!
* All evocations of contraptions!

**Modified**

* Moved the pickaxe to the right-bottom of the screen in Android Edition.

**Bug Fixed**

* Fixed an issue that you can place contraptions while the game is paused.


## 0.0.3

**Added**

* Halloween Map and Day 1. (Though Day 1 cannot be finished.)
* Menu Button.
* Custom Cursors.
* Expandable resolutions.

**Modified**

* Flag Zombies now always walk in the frontest.
* Modified the textures of houses.
* A buzzer sound will be played after clicking a disabled blueprint.

**Bug Fixed**

* Fixed an issue that particles will not be stopped after contraptions are destroyed and pause then resume the game.