using PVZEngine.Base;
using PVZEngine.Entities;
using Tools;

namespace MVZ2.Vanilla
{
    public abstract class RandomChinaEventDefinition : Definition
    {
        public RandomChinaEventDefinition(string nsp, string path, string name) : base(nsp, path)
        {
            Text = name;
        }
        public abstract void Run(Entity contraption, RandomGenerator rng);

        public override string GetDefinitionType() => VanillaDefinitionTypes.RANDOM_CHINA_EVENT;
        public string Text { get; set; }
    }
}
