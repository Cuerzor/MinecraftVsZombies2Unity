using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVZ2.GameContent.Areas;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Stages;
using MVZ2.Level;
using MVZ2.Managers;
using MVZ2.Vanilla.Effects;
using MVZ2Logic;
using MVZ2Logic.Games;
using MVZ2Logic.Level;
using MVZ2Logic.Serialization;
using NUnit.Framework;
using PVZEngine;
using PVZEngine.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MVZ2.Tests
{
    using LevelEngine = PVZEngine.Level.LevelEngine;
    public static class SerializationTests
    {
        [UnityTest]
        public static IEnumerator EntityReferenceTest()
        {
            var areaId = VanillaAreaID.day;
            var stageId = VanillaStageID.prologue;
            yield return Init();
            yield return GotoLevel();
            var levelController = GetLevelController();
            InitLevel(levelController, areaId, stageId);

            var game = GetGame();
            var level = GetLevel();

            var definition = game.GetEntityDefinition(VanillaContraptionID.dispenser);
            var entity = level.Spawn(definition, new Vector3(500, 0, 300), null);

            EntityID id = entity.GetFragment();
            var json = id.ToBson();
            var seriId = SerializeHelper.FromBson<EntityID>(json);
            var json2 = seriId.ToBson();
            Assert.AreEqual(json, json2);
        }
        [UnityTest]
        public static IEnumerator EntitySerializationTest()
        {
            var areaId = VanillaAreaID.day;
            var stageId = VanillaStageID.prologue;
            yield return Init();
            yield return GotoLevel();
            var levelController = GetLevelController();
            InitLevel(levelController, areaId, stageId);

            var game = GetGame();
            var level = GetLevel();

            var definition = game.GetEntityDefinition(VanillaContraptionID.dispenser);
            var entity = level.Spawn(definition, new Vector3(500, 0, 300), null);

            SerializableEntity seriEnt = entity.ToSerializable();
            var json = seriEnt.ToBson();

            SerializableEntity seriEnt2 = SerializeHelper.FromBson<SerializableEntity>(json);
            var entity2 = Entity.CreateFromSerializable(seriEnt2, level);
            entity2.LoadFromSerializable(seriEnt2);

            SerializableEntity seriEnt3 = entity2.ToSerializable();
            var json2 = seriEnt3.ToBson();
            Assert.AreEqual(json, json2);
        }
        [UnityTest]
        public static IEnumerator LevelSerializationTest()
        {
            var areaId = VanillaAreaID.day;
            var stageId = VanillaStageID.prologue;
            yield return Init();
            yield return GotoLevel();
            var levelController = GetLevelController();
            InitLevel(levelController, areaId, stageId);

            var game = GetGame();
            var level = GetLevel();

            var definition = game.GetEntityDefinition(VanillaContraptionID.dispenser);
            var entity = level.Spawn(definition, new Vector3(500, 0, 300), null);

            SerializableLevelController seriLevel = levelController.SaveGame();
            var json = seriLevel.ToBson();
            var json1 = seriLevel.level.ToBson();

            SerializableLevelController seriLevel2 = SerializeHelper.FromBson<SerializableLevelController>(json);
            yield return GotoLevel();
            var levelController2 = GetLevelController();
            LoadLevel(levelController2, seriLevel2, areaId, stageId);

            SerializableLevelController seriLevel3 = levelController2.SaveGame();
            var json2 = seriLevel3.level.ToBson();

            Debug.Log(json1);
            Debug.Log(json2);
            Assert.AreEqual(json1, json2);
        }
        [UnityTest]
        public static IEnumerator LevelSerializationTestPrologue()
        {
            var areaId = VanillaAreaID.day;
            var stageId = VanillaStageID.prologue;
            yield return PrepareLevelSerializationTest(areaId, stageId);

            var game = GetGame();
            var level = GetLevel();


            level.Start();

            // 生成器械
            var dispenser = game.GetEntityDefinition(VanillaContraptionID.dispenser);
            var furnace = game.GetEntityDefinition(VanillaContraptionID.furnace);
            var obsidian = game.GetEntityDefinition(VanillaContraptionID.obsidian);
            var mineTNT = game.GetEntityDefinition(VanillaContraptionID.mineTNT);
            for (int lane = 0; lane < level.GetMaxLaneCount(); lane++)
            {
                for (int column = 0; column < 7; column++)
                {
                    EntityDefinition def;
                    if (column == 0 || column == 3 || column == 4)
                    {
                        def = dispenser;
                    }
                    else if (column > 0 && column < 3)
                    {
                        def = furnace;
                    }
                    else if (column == 5)
                    {
                        def = obsidian;
                    }
                    else
                    {
                        def = mineTNT;
                    }
                    var x = level.GetEntityColumnX(column);
                    var z = level.GetEntityLaneZ(lane);
                    var height = level.GetGroundY(x, z);
                    var position = new Vector3(x, height, z);
                    level.Spawn(def, position, null);
                }
            }
            // 生成僵尸
            var zombie = VanillaSpawnID.zombie;
            var leatherCap = VanillaSpawnID.leatherCappedZombie;
            var ironHelmet = VanillaSpawnID.ironHelmettedZombie;
            for (int t = 0; t < 3; t++)
            {
                NamespaceID def = zombie;
                switch (t)
                {
                    case 1:
                        def = leatherCap;
                        break;
                    case 2:
                        def = ironHelmet;
                        break;
                }
                for (int i = 0; i < 10; i++)
                {
                    var spawnDef = game.GetSpawnDefinition(def);
                    level.SpawnEnemyAtRandomLane(spawnDef);
                }
            }
            yield return LevelSerializationTest(areaId, stageId, 5);
        }
        [UnityTest]
        public static IEnumerator LevelSerializationTestHalloween()
        {
            var areaId = VanillaAreaID.halloween;
            var stageId = VanillaStageID.halloween10;
            yield return PrepareLevelSerializationTest(areaId, stageId);

            var game = GetGame();
            var level = GetLevel();


            level.Start();

            // 生成器械
            var smallDispenser = game.GetEntityDefinition(VanillaContraptionID.smallDispenser);
            var moonlightsensor = game.GetEntityDefinition(VanillaContraptionID.moonlightSensor);
            var glowstone = game.GetEntityDefinition(VanillaContraptionID.glowstone);
            var punchton = game.GetEntityDefinition(VanillaContraptionID.punchton);
            var tnt = game.GetEntityDefinition(VanillaContraptionID.tnt);
            var soulFurnace = game.GetEntityDefinition(VanillaContraptionID.soulFurnace);
            var silvenser = game.GetEntityDefinition(VanillaContraptionID.silvenser);
            var magichest = game.GetEntityDefinition(VanillaContraptionID.magichest);
            for (int lane = 0; lane < level.GetMaxLaneCount(); lane++)
            {
                for (int column = 0; column < 8; column++)
                {
                    EntityDefinition def;
                    if (column == 0)
                    {
                        def = silvenser;
                    }
                    else if (column == 1)
                    {
                        def = moonlightsensor;
                    }
                    else if (column == 2)
                    {
                        def = soulFurnace;
                    }
                    else if (column == 3)
                    {
                        def = tnt;
                    }
                    else if (column == 4)
                    {
                        def = glowstone;
                    }
                    else if (column == 5)
                    {
                        def = smallDispenser;
                    }
                    else if (column == 6)
                    {
                        def = punchton;
                    }
                    else
                    {
                        def = magichest;
                    }
                    var x = level.GetEntityColumnX(column);
                    var z = level.GetEntityLaneZ(lane);
                    var height = level.GetGroundY(x, z);
                    var position = new Vector3(x, height, z);
                    level.Spawn(def, position, null);
                }
            }
            // 生成僵尸
            var skeleton = VanillaSpawnID.skeleton;
            var ghost = VanillaSpawnID.ghost;
            var mummy = VanillaSpawnID.mummy;
            var necromancer = VanillaSpawnID.necromancer;
            for (int t = 0; t < 4; t++)
            {
                NamespaceID def = skeleton;
                switch (t)
                {
                    case 1:
                        def = ghost;
                        break;
                    case 2:
                        def = mummy;
                        break;
                    case 3:
                        def = necromancer;
                        break;
                }
                for (int i = 0; i < 10; i++)
                {
                    var spawnDef = game.GetSpawnDefinition(def);
                    level.SpawnEnemyAtRandomLane(spawnDef);
                }
            }
            yield return LevelSerializationTest(areaId, stageId, 5);
        }
        [UnityTest]
        public static IEnumerator LevelSerializationTestDream()
        {
            var areaId = VanillaAreaID.dream;
            var stageId = VanillaStageID.dream10;
            yield return PrepareLevelSerializationTest(areaId, stageId);

            var game = GetGame();
            var level = GetLevel();


            level.Start();

            // 生成器械
            var lilypad = game.GetEntityDefinition(VanillaContraptionID.lilyPad);
            var drivenser = game.GetEntityDefinition(VanillaContraptionID.drivenser);
            var gravityPad = game.GetEntityDefinition(VanillaContraptionID.gravityPad);
            var vortexHopper = game.GetEntityDefinition(VanillaContraptionID.vortexHopper);
            var pistenser = game.GetEntityDefinition(VanillaContraptionID.pistenser);
            var totenser = game.GetEntityDefinition(VanillaContraptionID.totenser);
            var dreamCrystal = game.GetEntityDefinition(VanillaContraptionID.dreamCrystal);
            EntityLayout[] dreamLayout = new EntityLayout[]
            {
                new EntityLayout() { definition = lilypad, column = 3, row = 1  },
                new EntityLayout() { definition = lilypad, column = 3, row = 2  },
                new EntityLayout() { definition = lilypad, column = 3, row = 3  },
                new EntityLayout() { definition = lilypad, column = 3, row = 4  },
                new EntityLayout() { definition = lilypad, column = 4, row = 1  },
                new EntityLayout() { definition = lilypad, column = 4, row = 2  },
                new EntityLayout() { definition = lilypad, column = 4, row = 3  },
                new EntityLayout() { definition = lilypad, column = 4, row = 4  },
                new EntityLayout() { definition = lilypad, column = 5, row = 1  },
                new EntityLayout() { definition = lilypad, column = 5, row = 2  },
                new EntityLayout() { definition = lilypad, column = 5, row = 3  },
                new EntityLayout() { definition = lilypad, column = 5, row = 4  },

                new EntityLayout() { definition = drivenser, column = 0, row = 0  },
                new EntityLayout() { definition = drivenser, column = 0, row = 1  },
                new EntityLayout() { definition = drivenser, column = 0, row = 2  },
                new EntityLayout() { definition = drivenser, column = 0, row = 3  },
                new EntityLayout() { definition = drivenser, column = 0, row = 4  },
                new EntityLayout() { definition = drivenser, column = 0, row = 5  },

                new EntityLayout() { definition = gravityPad, column = 6, row = 0  },
                new EntityLayout() { definition = gravityPad, column = 2, row = 1  },
                new EntityLayout() { definition = gravityPad, column = 2, row = 2  },
                new EntityLayout() { definition = gravityPad, column = 2, row = 3  },
                new EntityLayout() { definition = gravityPad, column = 2, row = 4  },
                new EntityLayout() { definition = gravityPad, column = 6, row = 5  },

                new EntityLayout() { definition = vortexHopper, column = 6, row = 1  },
                new EntityLayout() { definition = vortexHopper, column = 6, row = 2  },
                new EntityLayout() { definition = vortexHopper, column = 6, row = 3  },
                new EntityLayout() { definition = vortexHopper, column = 6, row = 4  },

                new EntityLayout() { definition = pistenser, column = 1, row = 0  },
                new EntityLayout() { definition = pistenser, column = 1, row = 1  },
                new EntityLayout() { definition = pistenser, column = 1, row = 2  },
                new EntityLayout() { definition = pistenser, column = 1, row = 3  },
                new EntityLayout() { definition = pistenser, column = 1, row = 4  },
                new EntityLayout() { definition = pistenser, column = 1, row = 5  },
                new EntityLayout() { definition = pistenser, column = 2, row = 0  },
                new EntityLayout() { definition = pistenser, column = 2, row = 1  },
                new EntityLayout() { definition = pistenser, column = 2, row = 2  },
                new EntityLayout() { definition = pistenser, column = 2, row = 3  },
                new EntityLayout() { definition = pistenser, column = 2, row = 4  },
                new EntityLayout() { definition = pistenser, column = 2, row = 5  },

                new EntityLayout() { definition = totenser, column = 3, row = 0  },
                new EntityLayout() { definition = totenser, column = 3, row = 1  },
                new EntityLayout() { definition = totenser, column = 3, row = 2  },
                new EntityLayout() { definition = totenser, column = 3, row = 3  },
                new EntityLayout() { definition = totenser, column = 3, row = 4  },
                new EntityLayout() { definition = totenser, column = 3, row = 5  },
                new EntityLayout() { definition = totenser, column = 4, row = 0  },
                new EntityLayout() { definition = totenser, column = 4, row = 1  },
                new EntityLayout() { definition = totenser, column = 4, row = 2  },
                new EntityLayout() { definition = totenser, column = 4, row = 3  },
                new EntityLayout() { definition = totenser, column = 4, row = 4  },
                new EntityLayout() { definition = totenser, column = 4, row = 5  },

                new EntityLayout() { definition = dreamCrystal, column = 5, row = 0  },
                new EntityLayout() { definition = dreamCrystal, column = 5, row = 1  },
                new EntityLayout() { definition = dreamCrystal, column = 5, row = 2  },
                new EntityLayout() { definition = dreamCrystal, column = 5, row = 3  },
                new EntityLayout() { definition = dreamCrystal, column = 5, row = 4  },
                new EntityLayout() { definition = dreamCrystal, column = 5, row = 5  },
            };
            foreach (var layout in dreamLayout)
            {
                var x = level.GetEntityColumnX(layout.column);
                var z = level.GetEntityLaneZ(layout.row);
                var height = level.GetGroundY(x, z);
                var position = new Vector3(x, height, z);
                level.Spawn(layout.definition, position, null);
            }
            // 生成僵尸
            var enemyPool = new NamespaceID[]
            {
                VanillaSpawnID.spider,
                VanillaSpawnID.caveSpider,
                VanillaSpawnID.ghast,
                VanillaSpawnID.motherTerror,
            };
            foreach (var id in enemyPool)
            {
                for (int i = 0; i < 10; i++)
                {
                    var spawnDef = game.GetSpawnDefinition(id);
                    level.SpawnEnemyAtRandomLane(spawnDef);
                }
            }
            yield return LevelSerializationTest(areaId, stageId, 5);
        }
        [UnityTest]
        public static IEnumerator LevelSerializationTestCastle()
        {
            var areaId = VanillaAreaID.castle;
            var stageId = VanillaStageID.castle10;
            yield return PrepareLevelSerializationTest(areaId, stageId);

            var game = GetGame();
            var level = GetLevel();


            level.Start();

            // 生成器械
            var woodenDropper = game.GetEntityDefinition(VanillaContraptionID.woodenDropper);
            var spikeBlock = game.GetEntityDefinition(VanillaContraptionID.spikeBlock);
            var stoneDropper = game.GetEntityDefinition(VanillaContraptionID.stoneDropper);
            var stoneShield = game.GetEntityDefinition(VanillaContraptionID.stoneShield);
            var goldenApple = game.GetEntityDefinition(VanillaContraptionID.goldenApple);
            var thunderDrum = game.GetEntityDefinition(VanillaContraptionID.thunderDrum);
            var teslaCoil = game.GetEntityDefinition(VanillaContraptionID.teslaCoil);
            var giantBowl = game.GetEntityDefinition(VanillaContraptionID.giantBowl);
            EntityLayout[] levelLayout = new EntityLayout[]
            {
                new EntityLayout() { definition = woodenDropper, column = 0, row = 0  },
                new EntityLayout() { definition = woodenDropper, column = 0, row = 1  },
                new EntityLayout() { definition = woodenDropper, column = 0, row = 2  },
                new EntityLayout() { definition = woodenDropper, column = 0, row = 3  },
                new EntityLayout() { definition = woodenDropper, column = 0, row = 4  },

                new EntityLayout() { definition = spikeBlock, column = 6, row = 0  },
                new EntityLayout() { definition = spikeBlock, column = 6, row = 1  },
                new EntityLayout() { definition = spikeBlock, column = 6, row = 2  },
                new EntityLayout() { definition = spikeBlock, column = 6, row = 3  },
                new EntityLayout() { definition = spikeBlock, column = 6, row = 4  },

                new EntityLayout() { definition = stoneDropper, column = 1, row = 0  },
                new EntityLayout() { definition = stoneDropper, column = 1, row = 1  },
                new EntityLayout() { definition = stoneDropper, column = 1, row = 2  },
                new EntityLayout() { definition = stoneDropper, column = 1, row = 3  },
                new EntityLayout() { definition = stoneDropper, column = 1, row = 4  },
                new EntityLayout() { definition = stoneDropper, column = 2, row = 0  },
                new EntityLayout() { definition = stoneDropper, column = 2, row = 1  },
                new EntityLayout() { definition = stoneDropper, column = 2, row = 2  },
                new EntityLayout() { definition = stoneDropper, column = 2, row = 3  },
                new EntityLayout() { definition = stoneDropper, column = 2, row = 4  },

                new EntityLayout() { definition = stoneShield, column = 0, row = 0  },
                new EntityLayout() { definition = stoneShield, column = 0, row = 1  },
                new EntityLayout() { definition = stoneShield, column = 0, row = 2  },
                new EntityLayout() { definition = stoneShield, column = 0, row = 3  },
                new EntityLayout() { definition = stoneShield, column = 0, row = 4  },
                new EntityLayout() { definition = stoneShield, column = 1, row = 0  },
                new EntityLayout() { definition = stoneShield, column = 1, row = 1  },
                new EntityLayout() { definition = stoneShield, column = 1, row = 2  },
                new EntityLayout() { definition = stoneShield, column = 1, row = 3  },
                new EntityLayout() { definition = stoneShield, column = 1, row = 4  },
                new EntityLayout() { definition = stoneShield, column = 2, row = 0  },
                new EntityLayout() { definition = stoneShield, column = 2, row = 1  },
                new EntityLayout() { definition = stoneShield, column = 2, row = 2  },
                new EntityLayout() { definition = stoneShield, column = 2, row = 3  },
                new EntityLayout() { definition = stoneShield, column = 2, row = 4  },
                new EntityLayout() { definition = stoneShield, column = 3, row = 0  },
                new EntityLayout() { definition = stoneShield, column = 3, row = 1  },
                new EntityLayout() { definition = stoneShield, column = 3, row = 2  },
                new EntityLayout() { definition = stoneShield, column = 3, row = 3  },
                new EntityLayout() { definition = stoneShield, column = 3, row = 4  },
                new EntityLayout() { definition = stoneShield, column = 4, row = 0  },
                new EntityLayout() { definition = stoneShield, column = 4, row = 1  },
                new EntityLayout() { definition = stoneShield, column = 4, row = 2  },
                new EntityLayout() { definition = stoneShield, column = 4, row = 3  },
                new EntityLayout() { definition = stoneShield, column = 4, row = 4  },
                new EntityLayout() { definition = stoneShield, column = 5, row = 0  },
                new EntityLayout() { definition = stoneShield, column = 5, row = 1  },
                new EntityLayout() { definition = stoneShield, column = 5, row = 2  },
                new EntityLayout() { definition = stoneShield, column = 5, row = 3  },
                new EntityLayout() { definition = stoneShield, column = 5, row = 4  },

                new EntityLayout() { definition = goldenApple, column = 7, row = 0  },
                new EntityLayout() { definition = goldenApple, column = 7, row = 1  },
                new EntityLayout() { definition = goldenApple, column = 7, row = 2  },
                new EntityLayout() { definition = goldenApple, column = 7, row = 3  },
                new EntityLayout() { definition = goldenApple, column = 7, row = 4  },

                new EntityLayout() { definition = thunderDrum, column = 3, row = 0  },
                new EntityLayout() { definition = thunderDrum, column = 3, row = 1  },
                new EntityLayout() { definition = thunderDrum, column = 3, row = 2  },
                new EntityLayout() { definition = thunderDrum, column = 3, row = 3  },
                new EntityLayout() { definition = thunderDrum, column = 3, row = 4  },

                new EntityLayout() { definition = teslaCoil, column = 5, row = 0  },
                new EntityLayout() { definition = teslaCoil, column = 5, row = 1  },
                new EntityLayout() { definition = teslaCoil, column = 5, row = 2  },
                new EntityLayout() { definition = teslaCoil, column = 5, row = 3  },
                new EntityLayout() { definition = teslaCoil, column = 5, row = 4  },

                new EntityLayout() { definition = giantBowl, column = 4, row = 0  },
                new EntityLayout() { definition = giantBowl, column = 4, row = 1  },
                new EntityLayout() { definition = giantBowl, column = 4, row = 2  },
                new EntityLayout() { definition = giantBowl, column = 4, row = 3  },
                new EntityLayout() { definition = giantBowl, column = 4, row = 4  },
            };
            foreach (var layout in levelLayout)
            {
                var x = level.GetEntityColumnX(layout.column);
                var z = level.GetEntityLaneZ(layout.row);
                var height = level.GetGroundY(x, z);
                var position = new Vector3(x, height, z);
                level.Spawn(layout.definition, position, null);
            }
            // 生成僵尸
            var enemyPool = new NamespaceID[]
            {
                VanillaSpawnID.mesmerizer,
                VanillaSpawnID.berserker,
                VanillaSpawnID.dullahan,
                VanillaSpawnID.hellChariot,
            };
            foreach (var id in enemyPool)
            {
                for (int i = 0; i < 10; i++)
                {
                    var spawnDef = game.GetSpawnDefinition(id);
                    level.SpawnEnemyAtRandomLane(spawnDef);
                }
            }
            yield return LevelSerializationTest(areaId, stageId, 5);
        }
        private static IEnumerator PrepareLevelSerializationTest(NamespaceID areaId, NamespaceID stageId)
        {
            yield return Init();
            yield return GotoLevel();
            var levelController = GetLevelController();
            InitLevel(levelController, areaId, stageId);
            var level = GetLevel();
            level.Start();
        }
        private static IEnumerator LevelSerializationTest(NamespaceID areaId, NamespaceID stageId, int seconds)
        {
            var levelController = GetLevelController();
            var level = GetLevel();
            var serializableBeforeSave = levelController.SaveGame();
            var jsonBeforeSave = serializableBeforeSave.ToBson();

            var serializableAfterLoad = SerializeHelper.FromBson<SerializableLevelController>(jsonBeforeSave);
            var jsonAfterLoad = serializableAfterLoad.ToBson();

            var engineJsonBeforeSave = serializableBeforeSave.level.ToBson();
            var engineJsonAfterLoad = serializableAfterLoad.level.ToBson();
            Assert.AreEqual(engineJsonBeforeSave, engineJsonAfterLoad);

            // 更新五秒
            for (int i = 0; i < Ticks.FromSeconds(seconds); i++)
            {
                levelController.UpdateLogic();
            }
            var serializableAfterUpdate1 = levelController.SaveGame();
            var jsonAfterUpdate1 = SerializeHelper.ToBson(serializableAfterUpdate1.level);

            yield return GotoLevel();
            var levelAfterLoad = GetLevelController();
            LoadLevel(levelAfterLoad, serializableAfterLoad, areaId, stageId);
            // 更新五秒
            for (int i = 0; i < Ticks.FromSeconds(seconds); i++)
            {
                levelAfterLoad.UpdateLogic();
            }
            var serializableAfterUpdate2 = levelAfterLoad.SaveGame();
            var jsonAfterUpdate2 = SerializeHelper.ToBson(serializableAfterUpdate2.level);

            Debug.Log($"{nameof(jsonBeforeSave)}: {jsonBeforeSave}");
            Debug.Log($"{nameof(jsonAfterLoad)}: {jsonAfterLoad}");
            Debug.Log($"{nameof(jsonAfterUpdate1)}: {jsonAfterUpdate1}");
            Debug.Log($"{nameof(jsonAfterUpdate2)}: {jsonAfterUpdate2}");
            Assert.AreEqual(jsonAfterUpdate1, jsonAfterUpdate2);
        }
        private static IEnumerator Init()
        {
            var task = InitAsync();
            while (!task.IsCompleted)
                yield return null;
        }
        private static async Task InitAsync()
        {
            if (inited)
                return;
            var op = Addressables.LoadSceneAsync("TestScene", LoadSceneMode.Single);
            var scene = await op.Task;
            TestEntrance entrance = null;
            foreach (var rootGameObj in scene.Scene.GetRootGameObjects())
            {
                entrance = rootGameObj.GetComponentInChildren<TestEntrance>();
                if (entrance)
                {
                    break;
                }
            }
            if (!entrance)
                throw new InvalidOperationException("Test Entrance not found.");

            await entrance.Init();
            inited = true;
        }
        private static IGlobalGame GetGame()
        {
            return Global.Game;
        }
        private static LevelEngine GetLevel()
        {
            return Global.Level.GetLevel();
        }
        private static LevelController GetLevelController()
        {
            return Main.LevelManager.GetLevelController();
        }
        private static void InitLevel(LevelController level, NamespaceID areaId, NamespaceID stageId)
        {
            level.InitLevel(Main.Game, areaId, stageId, 58115310);
            level.StartGame();
        }
        private static void LoadLevel(LevelController level, SerializableLevelController seri, NamespaceID areaId, NamespaceID stageId)
        {
            level.SetActive(true);
            level.LoadGame(seri, Main.Game, areaId, stageId);
            level.ResumeGame(9999);
        }
        private static Coroutine GotoLevel()
        {
            return Global.Scene.GotoLevelCoroutine();
        }
        private static MainManager Main => MainManager.Instance;
        private static bool inited = false;


        public struct EntityLayout
        {
            public EntityDefinition definition;
            public int column;
            public int row;
        }
    }
}
