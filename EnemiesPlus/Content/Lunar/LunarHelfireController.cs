using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace EnemiesPlus.Content.Lunar
{
    public class LunarHelfireController : BurnEffectController
    {
        public DotController dotController;

        public void FixedUpdate()
        {
            if (!dotController.victimBody || !dotController.HasDotActive(LunarChanges.helfireDotIdx))
                HandleDestroy();
        }
    }
}
