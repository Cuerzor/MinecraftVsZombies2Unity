using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MVZ2.Level;
using MVZ2.Managers;
using MVZ2.Modding;
using MVZ2.Vanilla;
using MVZ2Logic;
using MVZ2Logic.Games;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MVZ2.Tests
{
    public class TestEntrance : MonoBehaviour
    {
        public async Task Init()
        {
            ModManager.OnRegisterModInfos += OnRegisterModInfos;

            mainGame.SetActive(true);
            await main.Initialize();
            main.InitLoad();
            var initTask = main.GetInitTask();
            if (initTask != null)
            {
                await initTask;
            }
        }
        private async Task OnRegisterModInfos(IGlobalGame game, List<ModInfo> results)
        {
            var logic = new VanillaMod();
            var assemblies = new Assembly[] { Assembly.GetAssembly(typeof(VanillaMod)), Assembly.GetAssembly(typeof(LogicMain)) };
            var locator = await Addressables.InitializeAsync().Task;
            results.Add(new ModInfo(logic.Namespace, locator)
            {
                LevelDataVersion = LevelManager.CURRENT_DATA_VERSION,
                DisplayName = "Vanilla",
                CatalogPath = null,
                IsBuiltin = true,
                Logic = logic,
                Assemblies = assemblies
            });
        }
        [SerializeField]
        private GameObject mainGame;
        [SerializeField]
        private MainManager main;
    }
}
