﻿using System.Collections.Generic;
using MVZ2.Managers;
using MVZ2.UI;
using MVZ2.Vanilla;
using PVZEngine;
using UnityEngine;

namespace MVZ2.Scenes
{
    public class AchievementHintController : MonoBehaviour
    {
        public void Show(IEnumerable<NamespaceID> achievements)
        {
            foreach (var achievement in achievements)
            {
                achievementEarnQueue.Enqueue(achievement);
            }
        }
        private void Awake()
        {
            ui.OnClick += () => showTimeout = 0;
        }
        private void Update()
        {
            if (showBlend <= 0.001f && achievementEarnQueue.Count > 0)
            {
                var achievement = achievementEarnQueue.Dequeue();
                var meta = main.ResourceManager.GetAchievementMeta(achievement);
                Sprite icon = null;
                string name = "???";
                if (meta != null)
                {
                    name = main.LanguageManager._p(VanillaStrings.CONTEXT_ACHIEVEMENT, meta.Name ?? string.Empty);
                    icon = main.GetFinalSprite(meta.Icon);
                }
                ui.UpdateAchievement(icon, name);
                showTimeout = maxShowTimeout;
            }
            if (showTimeout > 0)
            {
                showTimeout -= Time.deltaTime;
                showBlend = showBlend * (1 - showSpeed) + 1 * showSpeed;
            }
            else
            {
                showBlend = showBlend * (1 - showSpeed);
            }
            ui.SetShowValue(showBlend);
        }
        private MainManager main => MainManager.Instance;
        [SerializeField]
        private AchievementHint ui;
        [SerializeField]
        private float maxShowTimeout = 5;
        [SerializeField]
        private float showSpeed = 0.25f;
        private float showTimeout = 0;
        private Queue<NamespaceID> achievementEarnQueue = new Queue<NamespaceID>();
        private float showBlend = 0;
    }
}
