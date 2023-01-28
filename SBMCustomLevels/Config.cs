using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBM_CustomLevels
{
    [Serializable()]
    public class Config
    {
        public Color32 bannerColor = new Color32(204, 0, 0, 255);

        public List<LevelCFG> levels = new List<LevelCFG>();
    }

    [Serializable()]
    public class LevelCFG
    {
        public float timeAttackSeconds = 30.0f;
        public float personalRecord = 0.0f;

        public bool compeltedCarrot = false;
        public bool completedLevel = false;
        public bool copletedTimeAttack = false;
    }
}
