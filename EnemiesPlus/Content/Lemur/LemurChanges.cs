using UnityEngine.AddressableAssets;
using RoR2.Skills;

namespace EnemiesPlus.Content.Lemur
{
    internal class LemurChanges
    {
        private SkillDef LemBite => Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Lemurian/LemurianBodyBite.asset").WaitForCompletion();

        public static LemurChanges Instance { get; private set; }
        public static void Init() => Instance ??= new LemurChanges();

        private LemurChanges()
        {
            On.EntityStates.LemurianMonster.Bite.OnEnter += BiteLeap;
            LemBite.baseRechargeInterval = 1f;
        }


        private static void BiteLeap(On.EntityStates.LemurianMonster.Bite.orig_OnEnter orig, EntityStates.LemurianMonster.Bite self)
        {
            orig(self);

            var leapDirection = self.GetAimRay().direction;
            leapDirection.y = 0f;

            var magnitude = leapDirection.magnitude;
            if (magnitude > 0f)
                leapDirection /= magnitude;

            self.characterMotor.velocity = (leapDirection * self.characterBody.moveSpeed * 2f) with
            {
                y = self.characterBody.jumpPower * 0.25f
            };
            self.characterMotor.Motor.ForceUnground();
        }
    }
}
