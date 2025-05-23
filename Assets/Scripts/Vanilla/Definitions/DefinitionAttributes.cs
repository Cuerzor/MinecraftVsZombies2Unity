using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVZ2Logic;
using PVZEngine;

namespace MVZ2.Vanilla
{
    public class RandomChinaEventDefinitionAttribute : DefinitionAttribute
    {
        public RandomChinaEventDefinitionAttribute(string name) : base(name, VanillaDefinitionTypes.RANDOM_CHINA_EVENT)
        {
        }
    }
}
