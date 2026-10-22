using RoR2;

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
