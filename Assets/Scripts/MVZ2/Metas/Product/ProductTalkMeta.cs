﻿using System.Xml;
using MVZ2.IO;
using PVZEngine;

namespace MVZ2.Metas
{
    public class ProductTalkMeta
    {
        public NamespaceID Character { get; private set; }
        public string Text { get; private set; }
        public static ProductTalkMeta FromXmlNode(XmlNode node, string defaultNsp)
        {
            var character = node.GetAttributeNamespaceID("character", defaultNsp);
            var text = node.InnerText;

            return new ProductTalkMeta()
            {
                Character = character,
                Text = text
            };
        }
    }
}
