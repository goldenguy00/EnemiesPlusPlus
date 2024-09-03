using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using HG;
using System.Linq;
using RoR2;
using EntityStates;
using RoR2.CharacterAI;

namespace EnemiesPlus.Content.Worm
{
    internal class WormChanges
    {
        internal GameObject MagmaWorm => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormBody.prefab").WaitForCompletion();
        internal GameObject ElectricWorm => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormBody.prefab").WaitForCompletion();
        internal GameObject MagmaWormMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormMaster.prefab").WaitForCompletion();
        internal GameObject ElectricWormMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormMaster.prefab").WaitForCompletion();

        public static WormChanges Instance { get; private set; }

        public static void Init() => Instance ??= new WormChanges();

        private WormChanges()
        {
            On.RoR2.WormBodyPositionsDriver.FixedUpdateServer += RemoveRandomTurns;

            MagmaWormMaster.ReorderSkillDrivers(1);

            var magmaWormPS = MagmaWorm.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in magmaWormPS)
                ps.startSize *= 2;

            var magmaWormUtilityDef = MagmaWorm.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef;
            magmaWormUtilityDef.activationState = new SerializableEntityStateType(typeof(EntityStates.MagmaWorm.Leap));
            magmaWormUtilityDef.baseRechargeInterval = 60f;
            magmaWormUtilityDef.activationStateMachineName = "Weapon";

            foreach (var driver in MagmaWormMaster.GetComponents<AISkillDriver>())
                switch (driver.customName)
                {
                    case "Blink":
                        driver.shouldSprint = true;
                        driver.minDistance = 0f;
                        driver.aimType = AISkillDriver.AimType.AtMoveTarget;
                        break;
                    default:
                        driver.skillSlot = SkillSlot.None;
                        break;
                }
        }

        private static void RemoveRandomTurns(On.RoR2.WormBodyPositionsDriver.orig_FixedUpdateServer orig, WormBodyPositionsDriver self)
        {
            var body = self.gameObject.GetComponent<CharacterBody>();
            var targetPosition = self.referenceTransform.position;

            if (body && body.master)
                if (self.gameObject.GetComponents<EntityStateMachine>().Where(machine => machine.customName == "Weapon").First().state.GetType() != typeof(EntityStates.MagmaWorm.Leap))
                {
                    var baseAI = body.masterObject.GetComponent<BaseAI>();
                    if (baseAI && baseAI.currentEnemy != null && baseAI.currentEnemy.characterBody != null)
                        targetPosition = baseAI.currentEnemy.characterBody.corePosition;
                }

            var speedMultiplier = self.wormBodyPositions.speedMultiplier;
            var normalized = (targetPosition - self.chaserPosition).normalized;
            var num1 = (float)((self.chaserIsUnderground ? self.maxTurnSpeed : self.maxTurnSpeed * (double)self.turnRateCoefficientAboveGround) * (Math.PI / 180.0));
            var vector3 = Vector3.RotateTowards(new Vector3(self.chaserVelocity.x, 0.0f, self.chaserVelocity.z), new Vector3(normalized.x, 0.0f, normalized.z) * speedMultiplier, num1 * Time.fixedDeltaTime, float.PositiveInfinity);
            vector3 = vector3.normalized * speedMultiplier;
            var num2 = targetPosition.y - self.chaserPosition.y;
            var num3 = -self.chaserVelocity.y * self.yDamperConstant;
            var num4 = num2 * self.ySpringConstant;
            if (self.allowShoving && (double)Mathf.Abs(self.chaserVelocity.y) < self.yShoveVelocityThreshold && (double)num2 > self.yShovePositionThreshold)
                self.chaserVelocity = self.chaserVelocity.XAZ(self.chaserVelocity.y + self.yShoveForce * Time.fixedDeltaTime);
            if (!self.chaserIsUnderground)
            {
                num4 *= self.wormForceCoefficientAboveGround;
                num3 *= self.wormForceCoefficientAboveGround;
            }
            self.chaserVelocity = self.chaserVelocity.XAZ(self.chaserVelocity.y + (num4 + num3) * Time.fixedDeltaTime);
            self.chaserVelocity += Physics.gravity * Time.fixedDeltaTime;
            self.chaserVelocity = new Vector3(vector3.x, self.chaserVelocity.y, vector3.z);
            self.chaserPosition += self.chaserVelocity * Time.fixedDeltaTime;
            self.chasePositionVisualizer.position = self.chaserPosition;
            self.chaserIsUnderground = -(double)num2 < self.wormBodyPositions.undergroundTestYOffset;
            self.keyFrameGenerationTimer -= Time.deltaTime;
            if (self.keyFrameGenerationTimer > 0.0)
                return;
            self.keyFrameGenerationTimer = self.keyFrameGenerationInterval;
            self.wormBodyPositions.AttemptToGenerateKeyFrame(self.wormBodyPositions.GetSynchronizedTimeStamp() + self.wormBodyPositions.followDelay, self.chaserPosition, self.chaserVelocity);
        }
    }
}
