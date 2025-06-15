using UnityEngine;
using EntityStates;
using RoR2;
using System.Linq;

namespace EnemiesPlus.Content.Beetle
{
    public class BeetleBurrow : BaseState
    {
        public static float burrowAccuracyCoefficient = 0.3f;
        public static float baseBurrowDuration = 1f;
        public static float radius = 10f;

        private HurtBox target;
        private Vector3 predictedDestination;
        public float duration;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseBurrowDuration;
            var modelTransform = GetModelTransform();
            if (modelTransform)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }

            if (characterModel)
                characterModel.invisibilityCount++;
            if (hurtboxGroup)
                hurtboxGroup.hurtBoxesDeactivatorCounter++;

            gameObject.layer = LayerIndex.fakeActor.intVal;
            if (characterMotor)
            {
                characterMotor.enabled = false;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            CalculatePredictedDestination();
            Util.PlaySound("Play_magmaWorm_burrowed_loop", gameObject);
        }

        private void CalculatePredictedDestination()
        {
            var difference = Vector3.zero;
            var aimRay = GetAimRay();
            var bullseyeSearch = new BullseyeSearch
            {
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                maxDistanceFilter = 100f,
                teamMaskFilter = TeamMask.allButNeutral,
                filterByLoS = false,
                sortMode = BullseyeSearch.SortMode.Angle,
            };
            bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(gameObject));
            bullseyeSearch.RefreshCandidates();
            target = bullseyeSearch.GetResults().FirstOrDefault();
            if (target)
            {
                difference = target.transform.position - transform.position;
                var characterMotor = target.healthComponent?.body?.characterMotor;
                if (characterMotor)
                {
                    var moveDirection = characterMotor.moveDirection.normalized;
                    var estimatedTravelDistance = target.healthComponent.body.moveSpeed * duration;
                    var differenceFromMotor = difference + ((estimatedTravelDistance + radius) * moveDirection);
                    if (differenceFromMotor.sqrMagnitude <= radius * radius)
                        differenceFromMotor = difference - ((estimatedTravelDistance - radius) * moveDirection);
                    difference = differenceFromMotor;
                }
            }
            predictedDestination = transform.position + difference;
            characterDirection.forward = difference;
        }

        private Vector3 GetFinalPosition()
        {
            var finalDestination = target ? Vector3.Lerp(predictedDestination, target.transform.position, burrowAccuracyCoefficient) : predictedDestination;
            var groundNodes = SceneInfo.instance.groundNodes;
            var nodeIndex = groundNodes.FindClosestNode(finalDestination, characterBody.hullClassification);
            groundNodes.GetNodePosition(nodeIndex, out finalDestination);
            finalDestination += transform.position - characterBody.footPosition;
            return finalDestination;
        }

        private void SetPosition(Vector3 newPosition)
        {
            characterMotor?.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor)
                characterMotor.velocity = Vector3.zero;
            if (isAuthority && fixedAge >= duration)
                outer.SetNextState(new ExitBurrow());
        }

        public override void OnExit()
        {
            var finalPosition = GetFinalPosition();
            characterDirection.forward = finalPosition - transform.position;
            SetPosition(finalPosition);
            gameObject.layer = LayerIndex.defaultLayer.intVal;
            characterMotor?.Motor.RebuildCollidableLayers();
            
            if (characterModel)
                characterModel.invisibilityCount--;
            if (hurtboxGroup)
                hurtboxGroup.hurtBoxesDeactivatorCounter--;
            if (characterMotor)
                characterMotor.enabled = true;

            Util.PlaySound("Stop_magmaWorm_burrowed_loop", gameObject);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}
