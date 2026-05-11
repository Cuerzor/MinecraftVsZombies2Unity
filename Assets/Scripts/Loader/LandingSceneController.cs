using System.Reflection;
using MVZ2.Managers;
using MVZ2.Modding;
using MVZ2.Vanilla;
using MVZ2Logic;
using MVZ2Logic.Games;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace MVZ2
{
    public class LandingSceneController : MonoBehaviour
    {
        void Start()
        {
            ModManager.OnRegisterMods += RegisterMod;
            Addressables.LoadSceneAsync("Main", LoadSceneMode.Single);
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
    }
}
