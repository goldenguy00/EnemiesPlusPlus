using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using HG;
using System.Linq;
using RoR2;
using EntityStates;
using RoR2.CharacterAI;
using MonoMod.Cil;
using Mono.Cecil.Cil;

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
            if (EnemiesPlusConfig.wormTracking.Value)
            {
                EntityStates.MagmaWorm.SteerAtTarget.fastTurnRate = 120f;
                EntityStates.MagmaWorm.SteerAtTarget.slowTurnRate = 45f;
                On.EntityStates.MagmaWorm.SteerAtTarget.OnEnter += this.SteerAtTarget_OnEnter;
                IL.RoR2.WormBodyPositionsDriver.FixedUpdateServer += this.WormBodyPositionsDriver_FixedUpdateServer;
            }

            if (EnemiesPlusConfig.wormLeap.Value)
            {
                MagmaWormMaster.ReorderSkillDrivers(1);

                var magmaWormPS = MagmaWorm.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in magmaWormPS)
                {
                    var main = ps.main;
                    main.startSizeMultiplier *= 2f;
                }

                var magmaWormUtilityDef = MagmaWorm.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef;
                magmaWormUtilityDef.activationState = new SerializableEntityStateType(typeof(EntityStates.MagmaWorm.Leap));
                magmaWormUtilityDef.baseRechargeInterval = 60f;
                magmaWormUtilityDef.activationStateMachineName = "Weapon";

                foreach (var driver in MagmaWormMaster.GetComponents<AISkillDriver>())
                {
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
                            if (ai && ai.currentEnemy?.unset == false && ai.currentEnemy.hasLoS)
                                ai.currentEnemy.GetBullseyePosition(out pos);
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
                    if (ai && ai.currentEnemy?.gameObject)
                        self.wormBodyPositionsDriver.referenceTransform = ai.currentEnemy.gameObject.transform;
                }
            }
        }
    }
}
