using System.Reflection;
using System.Threading.Tasks;
using MVZ2.Managers;
using MVZ2.Modding;
using MVZ2.Vanilla;
using MVZ2Logic;
using MVZ2Logic.Games;
using UnityEngine;

namespace MVZ2.Tests
{
    public class TestEntrance : MonoBehaviour
    {
        public async Task Init()
        {
            ModManager.OnRegisterMods += RegisterMod;

            mainGame.SetActive(true);
            await main.Initialize();
            main.InitLoad();
            var initTask = main.GetInitTask();
            if (initTask != null)
            {
                await initTask;
            }
        }
        private void RegisterMod(IModManager manager)
        {
            var main = MainManager.Instance;
            var modLoader = new ModLoader(main);
            RegisterMods(modLoader, main.Game, manager);
            LoadMods(modLoader, main.Game);
        }
        private void RegisterMods(ModLoader modLoader, IGlobalGame game, IModManager manager)
        {
            var mod = new VanillaMod();
            var assemblies = new Assembly[] { Assembly.GetAssembly(typeof(VanillaMod)), Assembly.GetAssembly(typeof(LogicMain)) };
            var main = MainManager.Instance;
            modLoader.RegisterMod(mod, assemblies);
            manager.RegisterMod(mod);
        }
        private void LoadMods(ModLoader modLoader, IGlobalGame game)
        {
            modLoader.LoadAll(game);
        }
        [SerializeField]
        private GameObject mainGame;
        [SerializeField]
        private MainManager main;
    }
}
