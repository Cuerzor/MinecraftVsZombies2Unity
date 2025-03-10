﻿using MVZ2.Managers;
using UnityEngine;

namespace MVZ2.Localization
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleTextureTranslator : TranslateComponentSpriteMultiple<ParticleSystem>
    {
        protected override Sprite[] GetKeysInner()
        {
            var part = Component.textureSheetAnimation;
            var sprites = new Sprite[part.spriteCount];
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i] = part.GetSprite(i);
            }
            return sprites;
        }
        protected override void Translate(string language)
        {
            base.Translate(language);
            var part = Component.textureSheetAnimation;
            var sprites = Keys;
            for (int i = 0; i < sprites.Length; i++)
            {
                part.SetSprite(i, MainManager.Instance.GetFinalSprite(sprites[i], language));
            }
        }
    }
}
