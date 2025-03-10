﻿using System.Collections.Generic;
using System.Xml;

namespace MVZ2.Metas
{
    public class StatMetaList
    {
        public StatCategoryMeta[] categories;
        public static StatMetaList FromXmlNode(XmlNode node, string defaultNsp)
        {
            var categories = new List<StatCategoryMeta>();
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var childNode = node.ChildNodes[i];
                switch (childNode.Name)
                {
                    case "category":
                        var meta = StatCategoryMeta.FromXmlNode(node.ChildNodes[i], defaultNsp);
                        categories.Add(meta);
                        break;
                }
            }
            return new StatMetaList()
            {
                categories = categories.ToArray(),
            };
        }
    }
}
