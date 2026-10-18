using System;
using System.Linq;
using EnemiesPlus.Content.Beetle;
using EntityStates;
using HG;
using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EnemiesPlus.Content.Worm
{
    internal class WormChanges
    {
        internal GameObject MagmaWorm => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormBody.prefab").WaitForCompletion();
        internal GameObject ElectricWorm => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormBody.prefab").WaitForCompletion();
        internal GameObject MagmaWormMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormMaster.prefab").WaitForCompletion();
        internal GameObject ElectricWormMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormMaster.prefab").WaitForCompletion();
        internal SkillDef WormBlinkSkillDef => Addressables.LoadAssetAsync<SkillDef>(RoR2_Base_MagmaWorm.MagmaWormBodyBlink_asset).WaitForCompletion();

        public static WormChanges Instance { get; private set; }

        public static void Init() => Instance ??= new WormChanges();

        private WormChanges()
        {
            if (EnemiesPlusConfig.wormTracking.Value)
            {
                EntityStates.MagmaWorm.SteerAtTarget.fastTurnRate = 120f;
                EntityStates.MagmaWorm.SteerAtTarget.slowTurnRate = 45f;
                On.EntityStates.MagmaWorm.SteerAtTarget.OnEnter += this.SteerAtTarget_OnEnter;
                //IL.RoR2.WormBodyPositionsDriver.FixedUpdateServer += this.WormBodyPositionsDriver_FixedUpdateServer;
            }

            if (EnemiesPlusConfig.wormLeap.Value)
            {
                var worm = MagmaWorm;
                var master = MagmaWormMaster;
                var magmaWormPS = worm.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in magmaWormPS)
                {
                    var main = ps.main;
                    main.startSizeMultiplier *= 2f;
                }

                var leapDef = ScriptableObject.CreateInstance<SkillDef>().CloneFrom(WormBlinkSkillDef);
                leapDef.activationState = ContentAddition.AddEntityState<EntityStates.MagmaWorm.Leap>(out _);
                leapDef.baseRechargeInterval = 60f;
                leapDef.activationStateMachineName = "Weapon";
                leapDef.skillName = "WormBodyLeap";
                leapDef.skillNameToken = "WormBodyLeap";
                (leapDef as ScriptableObject).name = "WormBodyLeapSkillDef";

                var leapDriver = master.CloneComponent(master.GetComponents<AISkillDriver>().Last());
                leapDriver.customName = "Leap";
                leapDriver.minDistance = 0f;
                leapDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
                leapDriver.shouldSprint = true;
                leapDriver.requireSkillReady = true;
                leapDriver.skillSlot = SkillSlot.Secondary;
                leapDriver.driverUpdateTimerOverride = 5f;
                master.ReorderSkillDrivers(leapDriver, 1);

                var skillLoc = worm.GetComponent<SkillLocator>();
                if (skillLoc.secondary == null)
                {
                    skillLoc.secondary = worm.AddComponent<GenericSkill>();
                    skillLoc.secondary.skillName = "WormBodyLeapSkill";
                }

                var secondaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
                (secondaryFamily as ScriptableObject).name = "WormBodyLeapFamily";
                secondaryFamily.variants = [new SkillFamily.Variant { skillDef = leapDef}];
                skillLoc.secondary._skillFamily = secondaryFamily;
                ContentAddition.AddSkillFamily(secondaryFamily);
            }
        }

        private void WormBodyPositionsDriver_FixedUpdateServer(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<WormBodyPositionsDriver>(nameof(WormBodyPositionsDriver.referenceTransform)),
                    x => x.MatchCallOrCallvirt(out _)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Vector3, WormBodyPositionsDriver, Vector3>>((pos, driver) =>
                {
                    if (driver.TryGetComponent<CharacterBody>(out var body) && body.master)
                    {
                        foreach (var ai in body.master.aiComponents)
                        {
                            if (!ai || ai.currentEnemy == null)
                                continue;

                            if (ai.currentEnemy.TestLOSNow() && ai.currentEnemy.GetBullseyePosition(out var newPos))
                                return newPos;
                        }
                    }
                    return pos;
                });
            }
        }

        private void SteerAtTarget_OnEnter(On.EntityStates.MagmaWorm.SteerAtTarget.orig_OnEnter orig, EntityStates.MagmaWorm.SteerAtTarget self)
        {
            orig(self);

            var master = self.characterBody ? self.characterBody.master : null;
            if (master)
            {
                foreach (var ai in master.aiComponents)
                {
                    if (!ai || ai.currentEnemy == null)
                        continue;

                    if (ai.currentEnemy.TestLOSNow())
                        self.wormBodyPositionsDriver.referenceTransform = ai.currentEnemy.bestHurtBox.transform;
                }
            }
        }
    }
}
