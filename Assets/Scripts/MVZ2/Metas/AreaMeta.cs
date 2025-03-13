using System.Collections.Generic;
using System.Xml;
using MVZ2.IO;
using MVZ2Logic;
using MVZ2Logic.Level;
using PVZEngine;
using UnityEngine;

namespace MVZ2.Metas
{
    public class AreaMeta : IAreaMeta
    {
        public string ID { get; private set; }
        public NamespaceID ModelID { get; private set; }
        public NamespaceID MusicID { get; private set; }
        public NamespaceID Cart { get; private set; }
        public NamespaceID[] Tags { get; private set; }
        public SpriteReference StarshardIcon { get; private set; }

        public float EnemySpawnX { get; private set; }
        public float DoorZ { get; private set; }

        public Color BackgroundLight { get; private set; }
        public Color GlobalLight { get; private set; }

        public float GridWidth { get; private set; }
        public float GridHeight { get; private set; }
        public float GridLeftX { get; private set; }
        public float GridBottomZ { get; private set; }
        public int Lanes { get; private set; }
        public int Columns { get; private set; }

        public AreaGrid[] Grids { get; private set; }
        IAreaGridMeta[] IAreaMeta.Grids => Grids;
        public static AreaMeta FromXmlNode(XmlNode node, string defaultNsp)
        {
            var id = node.GetAttribute("id");
            var model = node.GetAttributeNamespaceID("model", defaultNsp);
            var music = node.GetAttributeNamespaceID("music", defaultNsp);
            var cart = node.GetAttributeNamespaceID("cart", defaultNsp);
            var starshard = node.GetAttributeSpriteReference("starshard", defaultNsp);
            var tags = node.GetAttributeNamespaceIDArray("tags", defaultNsp);

            float enemySpawnX = 1080;
            float doorZ = 240;
            var positionsNode = node["positions"];
            if (positionsNode != null)
            {
                enemySpawnX = positionsNode.GetAttributeFloat("enemySpawnX") ?? enemySpawnX;
                doorZ = positionsNode.GetAttributeFloat("doorZ") ?? doorZ;
            }

            Color backgroundLight = Color.white;
            Color globalLight = Color.white;
            var lightingNode = node["lighting"];
            if (lightingNode != null)
            {
                backgroundLight = lightingNode.GetAttributeColor("background") ?? Color.white;
                globalLight = lightingNode.GetAttributeColor("global") ?? Color.white;
            }

            float gridWidth = 80;
            float gridHeight = 80;
            float leftX = 260;
            float bottomZ = 80;
            int lanes = 5;
            int columns = 9;
            List<AreaGrid> grids = new List<AreaGrid>();
            var gridsNode = node["grids"];
            if (gridsNode != null)
            {
                gridWidth = gridsNode.GetAttributeFloat("width") ?? gridWidth;
                gridHeight = gridsNode.GetAttributeFloat("height") ?? gridHeight;
                leftX = gridsNode.GetAttributeFloat("leftX") ?? leftX;
                bottomZ = gridsNode.GetAttributeFloat("bottomZ") ?? bottomZ;
                lanes = gridsNode.GetAttributeInt("lanes") ?? lanes;
                columns = gridsNode.GetAttributeInt("columns") ?? columns;

                var childNodes = gridsNode.ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    var childNode = childNodes[i];
                    if (childNode.Name == "grid")
                    {
                        grids.Add(AreaGrid.FromXmlNode(childNode, defaultNsp));
                    }
                }
            }
            return new AreaMeta()
            {
                ID = id,
                ModelID = model,
                MusicID = music,
                StarshardIcon = starshard,
                Cart = cart,
                Tags = tags,

                EnemySpawnX = enemySpawnX,
                DoorZ = doorZ,

                BackgroundLight = backgroundLight,
                GlobalLight = globalLight,

                GridWidth = gridWidth,
                GridHeight = gridHeight,
                GridLeftX = leftX,
                GridBottomZ = bottomZ,
                Lanes = lanes,
                Columns = columns,
                Grids = grids.ToArray()

            };
        }
    }
    public class AreaGrid : IAreaGridMeta
    {
        public NamespaceID ID { get; set; }
        public float YOffset { get; set; }
        public SpriteReference Sprite { get; set; }
        public float Slope { get; set; }
        public static AreaGrid FromXmlNode(XmlNode node, string defaultNsp)
        {
            var id = node.GetAttributeNamespaceID("id", defaultNsp);
            var yOffset = node.GetAttributeFloat("yOffset") ?? 0;
            var sprite = node.GetAttributeSpriteReference("sprite", defaultNsp);
            var slope = node.GetAttributeFloat("slope") ?? 0;
            return new AreaGrid()
            {
                ID = id,
                YOffset = yOffset,
                Sprite = sprite,
                Slope = slope
            };
        }
    }
}
