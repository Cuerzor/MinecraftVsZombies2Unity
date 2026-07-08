using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MVZ2.Level;
using MVZ2.Modding;
using MVZ2.OldSave;
using MVZ2.Saves;
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
            OldSaveDataManager.SetSaveDataImporter(new OldSaveDataImporter());
            ModManager.OnRegisterModInfos += OnRegisterModInfos;
            Addressables.LoadSceneAsync("Main", LoadSceneMode.Single);
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
    }
}
