using EntityStates;
using EntityStates.Wisp1Monster;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EnemiesPlus.Content.Wisp
{
    internal class WispChanges
    {
        public static GameObject WispProjectile, WispGhostPrefab;
        internal SpawnCard GreaterWispCard => Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/GreaterWisp/cscGreaterWisp.asset").WaitForCompletion();
        internal GameObject WispMaster => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Wisp/WispMaster.prefab").WaitForCompletion();
        internal EntityStateConfiguration WispEmbers => Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Wisp/EntityStates.Wisp1Monster.FireEmbers.asset").WaitForCompletion();
        public static WispChanges Instance { get; private set; }

        public static void Init() => Instance ??= new WispChanges();

        private WispChanges()
        {
            if (EnemiesPlusConfig.wispChanges.Value)
            {
                CreateProjectile();
                WispEmbers.TryModifyFieldValue(nameof(FireEmbers.damageCoefficient), 1f);
                WispEmbers.TryModifyFieldValue(nameof(FireEmbers.bulletCount), 6);

                var ai = WispMaster.GetComponent<BaseAI>();
                ai.aimVectorDampTime = 0.1f;
                ai.aimVectorMaxSpeed = 180f;

                On.EntityStates.Wisp1Monster.ChargeEmbers.FixedUpdate += ChargeEmbers_FixedUpdate;
            }

            if (EnemiesPlusConfig.greaterWispChanges.Value)
                GreaterWispCard.directorCreditCost = 120;
        }

        private void CreateProjectile()
        {
            WispProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/Fireball.prefab").WaitForCompletion(), "ScoreWispFireball", true);

            WispGhostPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/FireballGhost.prefab").WaitForCompletion(), "ScoreWispFireballGhost", false);
            WispGhostPrefab.transform.GetChild(0).transform.localScale = Vector3.one * 0.35f;
            WispGhostPrefab.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Wisp/matWispEmber.mat").WaitForCompletion();

            WispProjectile.GetComponent<ProjectileController>().ghostPrefab = WispGhostPrefab;
            WispProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 250f;
            if (WispProjectile.TryGetComponent<ProjectileSingleTargetImpact>(out var impact))
                impact.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Wisp/ImpactWispEmber.prefab").WaitForCompletion();

            ContentAddition.AddProjectile(WispProjectile);
            ContentAddition.AddEntityState<FireBlast>(out _);
            FireBlast.projectilePrefab = WispProjectile;
        }

        private void ChargeEmbers_FixedUpdate(On.EntityStates.Wisp1Monster.ChargeEmbers.orig_FixedUpdate orig, ChargeEmbers self)
        {
            if (self.stopwatch + Time.fixedDeltaTime >= self.duration && self.isAuthority)
            {
                self.outer.SetNextState(new FireBlast());
            }
            else
            {
                orig.Invoke(self);
            }
        }
    }
}
