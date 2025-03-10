﻿using System.Collections.Generic;
using System.Xml;

namespace MVZ2.Metas
{
    public class MainmenuViewMetaList
    {
        public MainmenuViewMeta[] Metas { get; private set; }
        public static MainmenuViewMetaList FromXmlNode(XmlNode node, string defaultNsp)
        {
            var tags = new List<MainmenuViewMeta>();
            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                var child = node.ChildNodes[i];
                if (child.Name == "view")
                {
                    tags.Add(MainmenuViewMeta.FromXmlNode(child, defaultNsp));
                }
            }
            return new MainmenuViewMetaList()
            {
                Metas = tags.ToArray()
            };
        }
    }
}
