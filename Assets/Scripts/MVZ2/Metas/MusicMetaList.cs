﻿using System.Xml;

namespace MVZ2.Metas
{
    public class MusicMetaList
    {
        public MusicMeta[] metas;
        public static MusicMetaList FromXmlNode(XmlNode node, string defaultNsp)
        {
            var resources = new MusicMeta[node.ChildNodes.Count];
            for (int i = 0; i < resources.Length; i++)
            {
                resources[i] = MusicMeta.FromXmlNode(node.ChildNodes[i], defaultNsp);
            }
            return new MusicMetaList()
            {
                metas = resources,
            };
        }
    }
}
