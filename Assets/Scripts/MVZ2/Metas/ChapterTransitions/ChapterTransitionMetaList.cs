﻿using System.Collections.Generic;
using System.Xml;

namespace MVZ2.Metas
{
    public class ChapterTransitionMetaList
    {
        public ChapterTransitionMeta[] Metas { get; private set; }
        public static ChapterTransitionMetaList FromXmlNode(XmlNode node, string defaultNsp)
        {
            var tags = new List<ChapterTransitionMeta>();
            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                var child = node.ChildNodes[i];
                if (child.Name == "transition")
                {
                    tags.Add(ChapterTransitionMeta.FromXmlNode(child, defaultNsp));
                }
            }
            return new ChapterTransitionMetaList()
            {
                Metas = tags.ToArray()
            };
        }
    }
}
