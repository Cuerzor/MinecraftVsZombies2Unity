using PVZEngine.Base;

namespace PVZEngine
{
    public interface IGameContent
    {
        T GetDefinition<T>(string type, NamespaceID defRef) where T : Definition;
        T[] GetDefinitions<T>(string type) where T : Definition;
        Definition[] GetDefinitions();
    }
}
