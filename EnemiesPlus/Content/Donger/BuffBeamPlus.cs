using RoR2;
using System.Linq;
using UnityEngine;
using EntityStates.Bell.BellWeapon;

namespace EnemiesPlus.Content.Donger
{
    public class BuffBeamPlus : BuffBeam
    {
        public override void OnEnter()
        {
            if (base.characterBody)
            {
                attackSpeedStat = base.characterBody.attackSpeed;
                damageStat = base.characterBody.damage;
                critStat = base.characterBody.crit;
                moveSpeedStat = base.characterBody.moveSpeed;
            }

            Util.PlaySound(playBeamSoundString, base.gameObject);
            var aimRay = GetAimRay();
            var bs = new BullseyeSearch
            {
                filterByLoS = false,
                maxDistanceFilter = 50f,
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                sortMode = BullseyeSearch.SortMode.Angle,
                teamMaskFilter = TeamMask.none
            };
            if (base.teamComponent)
                bs.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);

            bs.RefreshCandidates();
            bs.FilterOutGameObject(base.gameObject);
            target = bs.GetResults().Where(x => x.healthComponent.body && x.healthComponent.body.bodyIndex != this.characterBody.bodyIndex).FirstOrDefault();

            if (target)
            {
                this.StartAimMode(BuffBeam.duration);
                targetBody = target.healthComponent.body;
                targetBody.AddBuff(RoR2Content.Buffs.ElephantArmorBoost.buffIndex);
            }
            else
            {
                skillLocator.secondary.AddOneStock();
                this.outer.SetNextStateToMain();
            }

            string childName = "Muzzle";
            var childLoc = GetModelChildLocator();
            if (childLoc)
            {
                muzzleTransform = childLoc.FindChild(childName);
                buffBeamInstance = Object.Instantiate(buffBeamPrefab);
                var beamChildLoc = buffBeamInstance.GetComponent<ChildLocator>();
                if (beamChildLoc)
                {
                    beamTipTransform = beamChildLoc.FindChild("BeamTip");
                }

                healBeamCurve = buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (targetBody)
            {
                targetBody.RemoveBuff(RoR2Content.Buffs.ElephantArmorBoost.buffIndex);
            }
        }
    }
}
