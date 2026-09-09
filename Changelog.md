## Added
- Added commands:
  - /test hugewaveevent: Triggers the huge wave event.
  - /test finalwaveevent：Triggers the final wave event.
  - /blueprint slots：Sets the slot count for blueprints.
  - /artifact slots：Sets the slot count for artifacts.
  - /money set：Sets money.
  - /money add：Increases money.
- Added entity material "Apparition" and "Wither Bone", removed entity material "Nether".
  - The material of Soul Furnace has been changed to "Wood".
  - The material of Ghost, Ghast, and Napstablook has been changed to "Apparition".
  - The materials of Mesmerizer, Berserker, Dullahan, Dullahan Head, Hacker, Bedserker, Nether Hunter, Nether Mage, Wither, and Pink Wither have been changed to "Wither Bone".
  - The material of the Wisp Fly has been changed to "Flesh".
- Added entity attribute "Fire Immunity".
  - Applied to Soul Furnace and Wisp Fly.
- Added screenshot function.
- Added textures for Moonlight Sensors in their daytime form.

## Modified
- When a projectile entity undergoes angular rotation due to directional deflection, its hitbox will now rotate correctly.
- Necrotombstone and Mine TNT now destroy themselves if they're on a slab tile.
- Adjusted the smashing sfx of Mutant Zombie in water.
- Adjusted the texture of the tiles destroyed by Nukes, to enhance their visibility against dark backgrounds.
- Adjusted the textures of Stone Shield and Emperor Zombie.
- The cursor for Force Pad's evocation and Skyward Beacon now has a unique sprite.
- Achievement descriptions are now hidden until the prerequisites for unlocking the achievement have been met (such as not having the means to complete the achievement).

### Balance
- Adventure
  - Reduced the enemy amount for Chapter 1.
  - Reduced the enemy amount for the first half of Chapter 5.
  - Gives 5000 money after firstly clearing a Boss level.
- Final Defense Carts
  - Money given after level clear: 10 -> 50
- Soul Furnace
  - Adjusted the contraption quality algorithm to get fuel.
    - The cost of a contraption required to fill an empty Soul Furnace:
      - Short recharge time: 330 -> 319
      - Long recharge time: 90 -> 180
      - Very Long recharge time: 30 -> 126
  - When evoked, if the fuel is less than 50%, it will now first replenish the fuel to 50%.
  - Fuel can no longer be replenished during the evocation.
- Silvenser
  - Z-axis volume of knife projectiles: 2 -> 6
  - Damage: 15 -> 20。
  - Reduced the amount in I, Zombie mode.
- Pistenser
  - Evocation target count: 10 -> ∞
  - Evocation will now predict the landing point based on the target's speed.
- Totenser
  - Effect of weakness: Damage lowered to 0 -> Melee attack damage lowered to 0
- Dream Crystal
  - Toughness: 2000 -> 3000
- Transfenser
  - Laser Damage: 5 -> 3
- Amethyst Pylon
  - Cost: 125 -> 175
  - Evocation will now be affected by damage bonuses as usual.
- Black Hole Bomb
  - Recharge Time: Very Long -> Long
- Nuke
  - Evocation: Breaks its own tile -> Breaks all tiles within a 3x3 range
- Nether Reactor Core
  - Fastest production time: 15s -> 6s
  - Adjusted the contraption quality algorithm to get production speed.
- Infectenser
  - Damage: 20 -> 30
  - Range: 3.5 tiles -> 4.5 tiles
  - Parabots fired by parabots will only hit the direct target they are aiming at, ignoring other enemies.
- Flag Zombie
  - Base movement speed: 1 -> 2
  - The random movement speed factor for them is no longer fixed at 200%.
- Pop Captain
  - Now blocks Skeleton Horses from jumping.
- Zombie Cat
  - Stun duration after revival: 1 second -> 0 second
  - Probability of Wisp Fly Spawn: 50% -> 100% (Hard), 50% (Other Difficulties)
  - Cost in I, Zombie mode: 150 -> 125 
- Mutant Zombie
  - Now blocks Skeleton Horses from jumping.
- Eye of the Giant
  - Damage multiplier: 250% -> 200%
  - Attack speed multiplier: 50% -> 60%
- Broken Lock
  - Recharge time reduction ratio: 1:1 -> 2:1

## Fixed
- Fixed the issue where the charging animation of Tesla Coil would continue to play even when it was disabled.
- Fixed the issue where, after the Giant comes into contact with invincible contraptions, even if it manifests after leaving these contraptions, these contraptions will still be destroyed when not in an invincible state.
- Fixed the issue where Mine TNT will explode multiple times when it contacts with multiple enemies.
- Fixed the issue where the movement speed description of Skeleton Horses in Almanac is incorrect.
- Fixed the issue where Skeleton Horses have no cry sounds.
- Fixed the issue where the hyperlink for "Doremy" in the "Grave in Dream" entry in the Almanac led to Kourindou.
- Fixed the issue where the animation of Hell Chariots sinking in water was very strange.
- Fixed the issue where the material of Anvils is not metal.
- Fixed the issue where Black Hole Bombs cannot attract monsters from the upper row to the current row.
- Fixed the issue where Anvils do not destroy invincible contraptions upon landing.

---

# 0.6.4
## Fixes
- Fixed the issue where Nightmare Crushing Walls' direction being incorrect on the mobile devices.
- Fixed the issue of incorrect detection height for the Skeleton Mage when it is on a high slope.
- Fixed the issue where the Hellfire in the Mausoleum map does not animate.
- Text and localization fixes.

---

# 0.6.3
## Modified
- Swapped the order of Parasite Terror spawning and damage infliction when a monster is parasitized by Mother Terror, to prevent the Cursed Candle from failing to kill the spawned Parasite Terrors.

## Fixes
- Fixed an issue where saves could not be properly saved on some Android devices.
- Fixed an issue where the Soul Furnace could not ignite explosives.
- Fixed an issue where using a pickaxe or Starshard on a contraption inside a Stone Shield would incorrectly target the Stone Shield when the mouse pointer is at a certain upper position.
- Fixed an issue where Nightmare’s hit material was "Flesh" instead of "Normal".
- Fixed an issue where Nightmare Crushing Walls displayed incorrectly when the view was inverted.
- Fixed an issue where Ghosts would not spawn on empty paths.
- Fixed an issue where Napstablook would not float.
- Text and localization fixes.

---

# 0.6.2
## Modified
- Updated music "The Exacerbated Castle Keep".
- Changed the order of gem colors in the display text of UFO Toy from "red, green, blue, cyan" to "green, red, blue, cyan".

## Fixed
- Fixed an issue where the almanac description of Mother Terror is different in Simplified Chinese and English.
- Fixed an issue where the health bar of airborne monsters on the Palanquin Ship would keep shaking after they died.
- Fixed an issue where some rolling ball projectiles were rendered incorrectly on the Palanquin Ship.
- Fixed an issue where the Gargoyle Statue and Monster Spawner were missing the "taken tile layer" tag in the Almanac.
- Fixed an issue where the Reverse Satellite was missing the "flying" tag in the Almanac.
- Fixed an issue where the Skeleton Statue in its remains state would not disappear when submerged in water.
- Made another attempt to fix the unresponsiveness caused by save file failures.
- The daytime Nuke can no longer be ignited by fire or explosions.
- Fixed an issue where the Red Dragon's tail swipe would destroy contraptions.
- Fixed inaccurate tooltip descriptions for some artifacts.
- Text and localization fixes.

---

# 0.6.1
## Changes
- Changed the level layouts of I, Zombie levels "Among Us" and "Break Apart".
- Replaced a Transfenser and a Beacon of the Light Layout from I, Zombie Endless 2 with two Glowstones.
- In the console on mobile devices, the text box will now appear above the virtual keyboard.
- The Locked Chest no longer summons Balloons in the top lane.
- Speed of entrance and exit of characters is no longer affected by framerate.
- Missiles of Brick Cannon are no longer affected by Force Pad.
- Cooling Cell will now select the blueprint with the longest remaining recharge time among all valid blueprints as its target.
- Added several missing hyperlinks to the Almanac.

## Fixed Issues
- Fixed the issue of being unable to adjust cursor position in the debug console on Android devices.
- Fixed the issue where sprites of the Nether Mage not changing depending on its types.
- Fixed the issue where material of the Nether Mage not being nether.
- Fixed the issue where resolution not updating after changing screen display settings.
- Fixed the issue where The Locked Chest being stuck in place if petrified when it jumps and attempts to place a Wither Skull while summoning the Wither.
- Fixed the issue of removing two Gravity Pads under a Brick Cannon with an pickaxe.
- Fixed the issue where Wooden Dropper and Golden Dropper not attacking Shadow Cell.
- Fixed the issue where resolution in settings displaying many decimal places.
- Fixed the issue of missing shop character sprites after shop cutscenes.
- Fixed the issue of skipping the first phase of the Giant fight by damaging it when it has very low health the moment it spawns.
- Fixed the issue where chapter 6 cannot begin due to files not saving properly.
- Fixed the issue where command Block-imitated blueprint pickups not being monochrome.
- Fixed the issue where pebbles split from boulders dealing only 5 damage.
- Fixed the issue where the rolling blocks summoned by The Locked Chest would not disappear after leaving the battlefield.
- Fixed the issue where Stone Shields provide explosion resistance to enemy obstacles.
- Fixed the issue where, even after Starshards were reset to zero or disabled, the held Starshard could still be used.
- Texts and localization fixes.

---

# 0.6.0
## Added
- Added chapter 6 - Palace of Earth Spirits.
- Added commands for Debug mode:
  - /gotolevel: Goto a level.
  - /izombie：Operate on the "I, Zombie" level.
  - /chaptertransition：Plays a chapter transition animation.
  - /unlock listlocked: Lists all locked game states.
  - /unlock all: Unlocks all game states.
  - /unlock none: Relocks all game states.
  - /test wave：Sets the current wave number.
  - /test flags：Sets the current flags.
  - /test spawnenemies：Spawns a page of different monsters.
- Added hyperlinks to Almanac, allowing users to navigate between different entries by clicking on text within the Almanac.
- Added 8 new Random China events.
- Add options for screen layout, VSync, and target frame rate.
- Add sound effects for the appearance of Ghasts and Pop Captains.
- Added glowing visual effects for Spiders, Cave Spiders, Mesmerizers, Berserkers and Bedserkers.
- Added 4 new I, Zombie levels, and I Zombie Endless 2.
- Added almanac entry for Bedserker.

## Modified
- Skyward Beacon's strike position now can be changed by just clicking on it instead of triggering it.
- After completing all levels in a map, entering the map will now set the view to the position of the endless level button.
- Random China now displays both of evocation event's name and description instead of its name only.
- Sweet Dream Pillow no longer triggers after all enemies in the level have been cleared.
- Improved the console's auto-completion feature. Now, code completion displays up to 10 suggestions at a time, with the ability to page through more.
- Added a description to Zombie Cloud's almanac entry, specifying its ability to absorb lightning and cold damage.
- Reduced the number of particles in the Glowstone halo.
- Update the texture of The Creature's Missile.

### Balance
- Moonlight Sensor
  - Material: Stone -> Wood
- Spike Block
  - Now can destroy multiple Hell Chariots at once.
- Stone Dropper
  - When making attack checks, use the volume of boulders instead of the volume of pebbles.
  - Damage of boulders: 40 -> 20
- Jeweled Pagoda
  - Recharge time: Very Long -> Short
  - Tile disabling duration: 4 minutes -> 2 minutes
- Command Block
  - Material: Stone -> Metal
- Dark Matter now make monsters invisible instead of making the battlefield dark.
- The total flag amount of level 1-3 is from 2 -> 1.
- These artifacts now only work for friendly contraptions:
  - Sweet Sleep Pillow
  - Dark Matter
  - Bottled Blackhold
  - Miracle Mallet
  - Broken Lantern
  - Lightbomb
  - Eye of the Giant
- The spawn rate of monsters during the boss battle in level 5-11 has been increased to 150% of the original rate.
- Undead Flying Objects are no longer undead mobs.

### Commands
- Command "/spawn" can now specify the faction of the spawned entity.
- The effect of command "/test armor" has been changed to make all monsters wear specific armor.
- The effect of command "/test shield" has been changed to make all monsters hold specific shield.

## Fixed
- Fixed the issue where when Pop Captain knocked a contraption onto a lilypad with a contraption on it, the knocked contraption will fall into water instead.
- Fixed the issue where Pop Captain can knock a lily pad onto land.
- Fixed the issue where monsters sometimes continue to attack after leaving the attack range when attacking a contraption inside the stone shield during melee attacks.
- Fixed the issue where starshards at the topmost cannot be stolen by blue UFOs at the topmost.
- Fixed the issue where damage resistance of bosses works incorrectly.
- Fixed the issue where the texture in the command block blueprint pickup was not grayscale.
- Fixed the issue where Seija's cursed doll does not follow Seija's faction upon spawn.
- Fixed the issue where Seija's cursed doll can be blown away by Wooden Fans.
- Fixed the issue where boulders launched by the evocation of Stone Droppers only deal 10 damage.
- Fixed the issue of incorrect ground-smashing animation for The Creature at low animation frequencies.
- Fixed the issue where switching windows while dragging the map arrow to move the view would cause the dragging arrow to not disappear.
- Fixed the issue of instant evocation failure after placing a Force Pad.
- Fixed the issue where some water stains were not being cleared after the Red Dragon crashed.
- Fixed the issue of white edges in the button textures in the music room.
- Fixed the issue where the evoked Devourer would not attempt to adjust its vertical position.
- Fixed the issue of incorrect flag rendering when the flag zombie sank into the water.
- Fixed the issue where Shikaisen Staff was not immune to slowing effects.
- Fixed the issue where zombies, after their health turned negative, were resurrected by Shikaisen Staff and their health after resurrection was not full.

---

# 0.5.2

## Modified
- Adjusted the algorithm for the frequency of each blueprint appearing in conveyor belt levels.

## Fixed
- Fixed the issue where Beacon's attack detection is broken.
- Fixed the issue where zombie clouds maintain their previous inertia after being stunned, resulting in incorrect positioning.
- Fixed the issue where stunned Ghasts won't fall on cloud tiles.
- Fixed the issue where the black holes created by destroying Black Hole Bomb using fire, explosion or sacrificing deals huge damage.
- Fixed the issue where The Creature's missile arm points towards incorrect position.
- Fixed the issue where Tesla Coil's damage is still 80.
- Fixed the issue where Moonlight Sensor's almanac description has no "glowing" tag.

---

# 0.5.1

## Added
- Added the almanac entry for Boat Zombies and Kourindou.
- When flames appear for the first time in Palanquin Ship Day 11, there will now be an advice to extinguish the flames.

## Modified

- Adjusted the hitboxes of most projectiles to better match the visual effect.
- Adjusted the monster types to spawn in some levels.
- Red Dragon's meteors now have a new sprite.
- Reduced the appear chance for Stone Droppers, increased the appear chance for Tesla Coils in level 3-11.
- Adjusted the layout of some I, Zombie levels.

### Balance
- Magichest
  - Cost: 200 -> 175
- Tesla Coil
  - Damage: 80 -> 60
  - Cost: 250 -> 200
  - Recharge time: Long -> Short
- Beacon
  - Cost：125 -> 150
- Skyward Beacon
  - Cost: 275 -> 225
- Force Pad
  - Cost: 75 -> 125
- Dream Silk
  - Cost: 100 -> 75
  - Contraption sleep duration: 30 seconds -> 50 seconds
  - Recharge time: Long -> Very Long
- Note Block
  - Damage: 5 -> 3
  - Range: A whole lane -> 4 tiles
  - Recharge time: Long -> Short
  - +10 tiles range during evocation.
- Hell Chariot
  - Now immunes to cold slowing.
- Undead Flying Object
  - Friendly Blue Undead Flying Objects no longer steal pickups, they provide redstones instead.
- Cannonball Zombie
  - Speed: 2 -> 4
- Pop Captain
  - Speed: 1 -> 1.5
  - Can no longer change the lane of very heavy monsters.
- Zombie Cloud
  - Duration of water stains: 20 seconds -> 40 seconds
- Wither
  - Regeneration: 30/s -> 15/s
  - The damage of Bedserker's explosion and eating Golden Apple will now bypasses Wither's damage resistance.
  - Increased the damage of eating Golden Apple from 600 to 900.

### Monster Spawning
- Skeleton
  - Earliest spawn wave: 0 -> 5
  - Weight: 3500 -> 3000
- Spider
  - Cost: 2 -> 1
  - Earliest spawn wave: 0 -> 5
  - Weight: 2000 -> 3000
- Ghast
  - Earliest spawn wave: 10 -> 5
- Zombie Cloud
  - Cost: 3 -> 2
  - Earliest spawn wave: 10 -> 5
  - Weight: 2000 -> 3000
- Cannoneer Zombie
  - Weight: 1500 -> 2500
- Pop Captain
  - Weight: 1000 -> 2000
- Monsters carrying Starshards will now appear 3 waves earlier.

### I, Zombie
- In Easy difficulty, the amount of redstone given by a furnace decreases from 10 to 9 (225 energy).
- In Hard difficulty, the amount of redstone given by a furnace increases from 6 to 7 (175 energy).
- The number of additional furnaces in the reward rounds has been reduced from 5 to 2.

## Fixes
- Fixed the issue where Cannoneer Zombie still launches Cannonballs when stunned.
- Fixed the issue where Undead Flying Object's explosion radius on death is too small.
- Fixed the issue where monsters killed by Vortex Hopper will still spin after revival.
- Fixed the issue where friendly monsters can only melee-attack the giant while they are in the same lane.
- Fixed the issue where Skyward Beacon does not count as a contraption that counters short enemy or flying enemy.
- Fixed the issue where Wooden Fan will blow away just spawned monsters in Shinning Needle Castle.
- Fixed the issue where carts can crush entities that are not contraption, not enemy and not obstacle.
- Fixed the issue where using the fighting techniques of Sorcerer's Scroll in multi finger touch would result in incorrect positioning.
- Fixed the issue where the icon does not twinkle when selecting fighting techniques of Sorcerer's Scroll on mobile devices.
- Fixed the issue where gunpowder will deal 22500 damage when exploded.
- Fixed the issue where phase 1 red dragon will release 3 tornados at any difficulty.

---

# 0.5.0

## Added
- Added chapter 5 - Palanquin Ship.
- Added "HDR Lighting" option.
- Added "Height Indicator" option.
- Added playtime statistics.
- Added Debug Console, which can be enabled by creating a special save file named "Debug".
- Added blueprint hover descriptions in levels for mobile devices.
- Added hover descriptions for some options.
- Added "Lightning" material, used for Lightning Orb.
- Added 8 new Random China events.

## Modified

### Balance
- Moonlight Sensors's evocation production multiplier reduced from 250% to 200%.
- Soul Furnace is now no longer a nocturnal contraption, its gained fuel amount and capacity also get doubled.
- Silvenser's energy cost reduced from 175 to 150.
- Dream Crystal healing amount increased from 40/sec to 60/sec.
- Bottle Blackhole damage bonus reduced from 15% to 10%.
- Desire Pot's energy cost reduced from 200 to 125, recharge time reduced from Very Long to Long.
- Talisman Zombie toughness increased from 200 to 350.
- Zombies whose Divine Shield is broken can no longer be re-shielded by Emperor Zombie for 5 seconds.
- Gargoyle Statue and Spawner now prioritize spawning in locations without contraptions.
- Adjusted the appearance mechanism for Starshard monsters; now, one monster carrying a Starshard appears every 6 waves, and only one.
- Adjusted the Boss damage reduction mechanism; bosses now take a maximum of 600 damage within 0.5 seconds, instead of limiting single-hit damage to below 600.

### Graphics
- Adjusted the textures for buttons and scrollbars, as well as the style of some widgets in the options interface.
- Reworked the texture for Splitenser.
- Reworked the character texture for Seija.
- Adjusted the background darkness effect upon Nightmare's appearance and the background darkness effect after a large wave of monsters in level 4-11.

### Almanac
- Adjusted the flavor text for Furnace in the Almanac.
- Changed the wording in some Almanac entries to "friendly units".
- Adjusted the monster thumbnails in the Almanac.
- Locked hidden artifacts now appear as silhouettes in the Almanac.

### Others
- Night-time contraptions during the day can no longer use the instant evoke function.
- All darkness effects are now canceled after a level is completed.
- Levels that have already been completed can no longer result in a game over due to certain monsters.
- Updated the place sound of Silvenser, Gravity Pad, Spike Block, Golden Apple, Force Pad and Golden Dropper.

## Fixes
- Fixed an issue where the "Earned!" text for achievements sometimes would not be translated according to the game language.
- Fixed an issue where the Dream Shield effect would not display when particles were turned off.
- Fixed an issue where contraptions could be placed on Command Blocks that were about to transform into Lily Pads.
- Fixed an issue where clicking the Nightmare fate choosing button while holding a contraption would immediately place the contraption at the pointer's position.
- Fixed an issue where gems dropped by a Berserker infected by an Infectenser would also incorrectly display the infection icon.
- Fixed an issue where two-finger tap on mobile devices was recognized as a mouse right-click.
- Fixed an issue where Reverse Satellite was incorrectly classified as undead.

---

# 0.4.5

## Added
- A new pause easter egg image.

## Modified
- Hellfires now react with snowballs, large snowballs, wooden balls and ice bolts.
- Adjusted the color of command block contraptions.
- Light emitted by Command block contraptions is now white.
- Changed the material of Hellfire to Netherrack.
- The Giant will now spawn a zombie for every 0.5 second when it reaches the leftmost in phase 3.
- Removed the mechanic where Dispensers' attack speed fluctuates randomly in I, Zombie mode.
- Repainted the texture of Marisa.

## Fixed
- The Giant will not decrease its damage reduction when it reaches the leftmost in phase 3 under hard difficulty.
- The hot key text will block the starshard panel to be clicked.
- The held contraption would be placed directly at the finger position when clicking the dialog button to cancel the pause in Android.
- Command block contraptions cannot be instant-triggered.
- Punchtons cannot damage enemies that are very close to the rightmost.
- Punchtons are not be affected by Eye of the Giant for 1 frame after they are just built.
- Entities which have changed toughness like Imp in I, Zombie or evoked Obsidian will scale toughness again after reloading the level.
- The increament of attack speed of enemies is unexpectedly high in I, Zombie levels when they have not taken damage for a while.
- Gargoyle Statue and Spawner has not "Carrier Layer" tag in Almanac.
- Entity texture in Almanac will be green due to pumpkin carriages' green light if the Almanac is opened in Halloween Endless.
- Zombie and Leather Cap Zombie no longer appear in later rounds of Endless levels.
- Enemies appear one wave ahead after every 10 rounds incorrectly, instead of one wave ahead after every 2 rounds in Endless levels.

---

# 0.4.4

## Added
- "Show FPS", "Show Hot Keys" and "Animation Frequency" option.
- Fully-Charged flash for blueprints.

## Modified

### Balancing
- Balanced some contraptions:
  
#### Gravity Pad
- Cost: 75 -> 100

#### Dream Silk
- Cost: 75 -> 100

#### Wooden Dropper
- Evocation: Fires 30 wooden balls instantly -> Fires 30 large wooden balls instantly, each deals 80 damage

#### Stone Dropper
- Knockback: 6 -> 5

#### Giant Bowl
- Cost: 100 -> 150

#### Note Block
- Recharge Time: Short -> Long
- Added damage reduction mechanic for notes. The notes heavily reduce their damage upon colliding with Note Block, and restore their damage over time.

#### Desire Pot
- Every Desire Pot can only duplicate up to 3 starshards.
- The starshard carried by an enemy will now trigger ALL Desire Pots at once, and will no longer trigger any Desire Pot thereafter.
- Evocation: Copies "The last 2 blueprints" instead of "The top 2 blueprints".

### Misc
- Wither Skeleton monsters are now 20% bigger than normal enemies.
- Adjusted the compression format for entity textures.
- Energy number will now flicker if you try to select a blueprint while you have not enough energy.
- Replaced the text of endless level button to ∞ from E.
- There will be a hint text at the start of endless level.

## Fixed
- Level may failed to be loaded.
- Some entity animations play incorrectly faster.
- Mouse cursor on Windows displays incorrect in level.
- Unable to view an enemy in Almanac by clicking it while choosing blueprints.
- Flying enemy will not align to a lane if it's between 2 lanes.
- Projectiles boosted by Force Pad will make very heavy enemies fly.
- Obsidian will lose some toughness after breaking its netherite armor.

---

# 0.4.3

## Added
- Added the button to cancel all chosen blueprints.
- Added "Blueprint Warnings" and "Command Block Mode" option.

## Modified
- Seperated the repick button to "Repick previous blueprints" button and "Repick previous artifacts" button. 
- Adjusted the control method of zoom in Almanac.

## Fixed
- Force Pad does not affect friendly entities.
- Dark Matter makes the battlefield fully black on Android due to the lighting shader precision.

---

# 0.4.2-AndroidFix1

## Fixed
- Some textures are full black on some Android devices due to imcompatible texture compression format.

---

# 0.4.2

## Added
- Unique placement type tags in almanac for 4 upgrade contraptions.
- Pressing right mouse button during choosing blueprints now cancels all chosen blueprints.

## Modified
- Necrotombstone's recharge time extended to Long from Short, and will stop summoning if the count of skeleton warriors reaches its limit of 30.
- Increased the toughness of Diamond Spikes to 5400 from 2700, and crush damage it takes to 600 from 300.
- Added "Immobilization Immunity" tag to Diamond Spikes.
- Red fire arrows passing through Cursed Hellfire now transform into Cursed fire arrows.
- Slightly reduced the darkness opacity of Dark Matter artifact.
- Compressed game resource sizes.
- Optimized game performance.
  - Reduced entity animation update frequency when excessive entities are present.
- Adjusted the interface of Music Room, Added the texts for current time and total time, and removed the click sound for buttons.

## Fixes
- Re-entering an Endless level caused current Upgrade Contraption Blueprints to double their energy cost again.
- Entities transformed by Command Block failed to display red visual effects while under the Stone Shield evocation effect.
- Missing damage stage for Bone Wall.
- Certain difficulty effects failed to take effect.
- Almanac tags of an enemy will still display even if this enemy is not encountered yet.
- The contraption icon on command block blueprint pickup is not grayscale.

---

# 0.4.1

## Added
- Tag icons for almanac.
- Repair progress animation for Punchton and Thunder Drum.
- A pop-up to remind the player when a dialog is skipped by Skip Dialogs option.

## Modified
- Large arrows now can be ignited by Hellfire.
- Large arrows fired by Drivenser and Splitenser's evocation now deal 600 damage instead of 1000.
  - Nightmare can only take up to 600 damage in one hit, so Drivenser's evocation effect is not affected in boss battles of 2-11.
- Wicked Hermit Zombie now always teleports to the leftmost column regardless of whether it's hypnotised.
- Moved the tooltip of artifacts in the blueprint selection screen to be above the icon, such that it is no longer blocked by your finger on the touchscreen.
- Adjusted the layer of post-huge-wave visual effects in level 4-11.
- Added Gargoyle to I, Zombie level "Mineclear".
- The evocation of Force Pad now resets the velocity of target monsters.
- Adjusted the placing and damage sound effects of Hellfire.
- Adjusted the background texture for command block blueprints.
- Hellfire's material has been changed to Nether from Stone.
- Increased the volume height of Mummy Gas by 75%, such that Fire Skeleton Mage's fireballs can ignite it. Also optimized the display range of gas objects.
- Decreased the attack height of Skeleton Mages and the volume height of ice bolts, such that they can not hit the flying enemies.
- Improved the color of slow status effect.
- Adjusted the time points to save user data.

## Fixed
- Items like blueprint, pickaxe or starshard could not be canceled by re-clicking it.
- Monsters that died within the range of the Shikaisen Staff would be considered completely dead by the level, resulting in the level-clear pickup to be spawned.
- Killing all enemies before spawners spawned their enemies would instantly generate the level reward.
- Instant-evocation features could be used on Anvil.
- Dream Silk could be used on a Magichest which was devouring an enemy.
- The light range of Totenser's fire breath being incorrect.
- Totenser’s fire breath being in the wrong direction if mesmerized.
- Divine shields granted by Emperor Zombie could not block knives nor large arrows.
- The wrong tag of the "Zombie Scroll" dialog.
- Trigger tutorial can not be finished while another save file has "swap trigger" option enabled.
- Pistenser's evocation can not damage mesmerized contraptions.

---

# 0.4.0

## Added
- Added Chapter 4: The Hall of Dreams' Great Mausoleum.
- Added mass mechanic for monsters, which determines how far the monster will be knocked back.
- Added credits section, accessed through the options menu from the main menu.
- Added fast-forward multiplier option.
- Added current version number text to the main menu.
- Picking a blueprint while holding a Starshard now immediately evokes the placed contraption.
- Added dialogs to introduce artifacts in level 1-7.
- Added the zoom feature to the Misc Almanac.
- Added import and export features for user save data.
- Added 7 new Random China events.
- Added export feature for game log files.

## Modified

### Contraptions
- Gravity Pads can now pull down certain low-altitude floating enemies.
- TNT now detonates immediately upon being destroyed by fire or explosive damage.
- TNT struck by lightning damage now transforms into Charged TNT.
- Increased the energy cost of Tesla Coil from 175 to 250.
- Reduced the energy cost of Spiked Block from 100 to 75.
- Updated the texture of the stone shield to make the internal contraption easier to identify.

### Enemies
- Electric damage now deals 100% damage to ghosts.
- When melee monsters attack, their forward movement speed will rapidly decrease.
- Fire projectiles that hit Gargoyle or Hell Chariots no longer deal splash damage.

### Boss
- Increased damage of Nightmareaper's Death Wheels by 40% in Hard Mode.

### Lighting
- Adjusted lighting rendering effects.
- Spark particles from Nightmareaper's Death Wheels now glow.
- Mind Control Orbs now glow.
- Dropped pickups are no longer affected by darkness.

### Music
- Added missing bass instruments to the stage music of Shining Needle Castle.
- Reduced volume of the Seija boss battle music.

### Levels
- Added usable blueprints to level 1-6: Glowstone, Obsidian, and Punchton.
- Adjusted interaction logic of the Breakout Board in level 2-6.
- Tripled enemy spawn rate on Day 11 of each chapter.

### Almanac
- Added health entries to three contraptions from the prologue in the Almanac.
- Added artist credits to character entries in the Almanac.

### Misc
- Added grid patterns to Shining Needle Castle's stage background.
- Changed post-blueprint-selection warnings to display enemy types instead of specific enemy names.
- Rain effects no longer update while the game is paused.
- Renamed the "Evoke" action in Simplified Chinese.
- Adjusted in-game text.

## Fixed
- Fixed an issue where Mine TNT's evocation would incorrectly spawn new Mine TNTs in the row above when launching them from the bottom row.
- Fixed crash caused by Nightmareaper attacking invincible contraptions.
- Fixed an issue where Anvils fail to deal damage during the instant they hit the ground.
- Fixed the frame drops caused by prolonged light source existence.
- Fixed an issue of being unable to save and exit when there are stunned enemies.
- Fixed untranslated Blueprint names in level 2-6.
- Dream Silk can no longer be applied to spinning Vortex Hopper.
- Fixed an issue where Breakout minigame pearls could be launched off-screen.
- Fixed an edge case where defeating Seija might trigger her Nimble Fabric effect.
- Fixed an issue where the javelin throw animation of Totenser still plays while it's sleeping due to Dream Silk.
- Fixed the issue where players can close the menu dialog and continue the game using hotkeys, bypassing restart/exit confirmation pop-ups.
- Nyan Cat and Nyaightmare now will correctly switch between 2 phases of the Dream World.

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
- Fixed the "Archive" button on the main menu being prioritized over the "Addons" button.
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
- Game version now will be shown in the main menu.
- Changelog in Readme, and the upcoming features! Yeah you are reading one of them!
- The game version will now be displayed at the right-top of main menu.

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