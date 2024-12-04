using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimValiFFARW
{
    public class RefuelAERIALJobDriver : JobDriver
    {
        private static bool GunNeedsLoading(Building b, out int ammoNeeded)
        {
            ammoNeeded = 0;
            if (!(b is AERIALSystem aerialSystem))
            {
                return false;
            }

            var compChangeableProjectile = aerialSystem.gun.TryGetComp<AERIALChangeableProjectile>();
            if (compChangeableProjectile == null)
            {
                return false;
            }

            ammoNeeded = AERIALChangeableProjectile.maxShells - compChangeableProjectile.ShellsLoaded;
            return !compChangeableProjectile.FullyLoaded;
        }

        public static Thing FindAmmo(Pawn pawn, AERIALSystem aerial)
        {
            StorageSettings? allowed = pawn.IsColonist ? aerial.gun.TryGetComp<AERIALChangeableProjectile>().allowedShellsSettings : null;

            bool Validator(Thing t)
            {
                return !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1) &&
                       (allowed == null || allowed.AllowedToAccept(t));
            }

            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 40f,
                Validator);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            int ammoNeeded = 0;

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            var loadIfNeeded = new Toil();
            loadIfNeeded.initAction = delegate
            {
                Pawn actor = loadIfNeeded.actor;
                var building = (Building)actor.CurJob.targetA.Thing;
                AERIALSystem building_TurretGun = (AERIALSystem)building;
                if (!GunNeedsLoading(building, out ammoNeeded))
                    //this.JumpToToil(gotoTurret);
                {
                    return;
                }

                Thing? thing = FindAmmo(pawn, building_TurretGun);
                if (thing == null)
                {
                    if (actor.Faction == Faction.OfPlayer)
                    {
                        Messages.Message(
                            "MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, building_TurretGun.Label,
                                actor.Named("PAWN"), building_TurretGun.Named("GUN")).CapitalizeFirst(),
                            building_TurretGun, MessageTypeDefOf.NegativeEvent);
                    }

                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {

                    actor.CurJob.targetB = thing;
                    actor.CurJob.count = Math.Min(thing.stackCount, ammoNeeded);
                }
            };
            yield return loadIfNeeded;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 10, GetActor().CurJob.count);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = loadIfNeeded.actor;
                    AERIALSystem building_TurretGun = (AERIALSystem)actor.CurJob.targetA.Thing;

                    building_TurretGun.gun.TryGetComp<AERIALChangeableProjectile>()
                        .NewLoadShell(actor.CurJob.targetB.Thing.def, actor.CurJob.count);
                    actor.carryTracker.innerContainer.ClearAndDestroyContents();
                },
            };
            //yield return gotoTurret;
        }
    }
}
