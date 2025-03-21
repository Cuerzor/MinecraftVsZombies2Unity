﻿using System.Xml;
using MVZ2.IO;
using MVZ2Logic;
using PVZEngine;

namespace MVZ2.TalkData
{
    public class TalkGroupArchiveInfo
    {
        public string name = string.Empty;
        public SpriteReference background;
        public NamespaceID unlock;
        public NamespaceID music;
        public XmlNode ToXmlNode(XmlDocument document)
        {
            XmlNode node = document.CreateElement("archive");
            node.CreateAttribute("name", name);
            node.CreateAttribute("background", background?.ToString());
            node.CreateAttribute("unlock", unlock?.ToString());
            node.CreateAttribute("music", music?.ToString());
            return node;
        }
        public static TalkGroupArchiveInfo FromXmlNode(XmlNode node, string defaultNsp)
        {
            var name = node.GetAttribute("name");
            var background = node.GetAttributeSpriteReference("background", defaultNsp);
            var unlock = node.GetAttributeNamespaceID("unlock", defaultNsp);
            var music = node.GetAttributeNamespaceID("music", defaultNsp);
            return new TalkGroupArchiveInfo()
            {
                name = name,
                background = background,
                unlock = unlock,
                music = music,
            };
        }
    }
}
