﻿using RimWorld;
using UnityEngine;
using Verse;

namespace RimValiFFARW
{
    public class AERIALLaunch : Verb
    {
        public virtual ThingDef Projectile
        {
            get
            {
                AERIALChangeableProjectile comp = EquipmentSource.GetComp<AERIALChangeableProjectile>();
                if (comp?.Loaded == true&&comp.Projectile!=null)
                {
                    return comp.Projectile;
                }

                return verbProps.defaultProjectile;
            }
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster,
                currentTarget.HasThing ? currentTarget.Thing : null,
                EquipmentSource?.def, Projectile, ShotsPerBurst > 1));
        }

        protected override bool TryCastShot()
        {
            var comp = EquipmentSource.GetComp<AERIALChangeableProjectile>();
            Thing launcher = caster;
            if (comp.PeekNextProjectile == null)
                return false;
            Thing equipment = EquipmentSource;
            Vector3 drawPos = caster.DrawPos;
            TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine shootLine);
            float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius,
                currentTarget.Cell - caster.Position);
            int max = GenRadial.NumCellsInRadius(num);
            int num2 = Rand.Range(0, max);
            IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];


            ThrowDebugText("ToRadius");
            ThrowDebugText("Rad\nDest", c);
            var projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
            if (Rand.Chance(0.5f))
            {
                projectileHitFlags = ProjectileHitFlags.All;
            }

            if (!canHitNonTargetPawnsNow)
            {
                projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
            }

            IntVec3 pos = shootLine.Source;
            pos.x += 20;
            pos.z += 20;
            var projectile = (Projectile)GenSpawn.Spawn(comp.PollNextProjectile, pos, caster.Map);
            projectile.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment: equipment);
            

            if (comp.PeekNextProjectile!=null)
            {
                pos = shootLine.Source;
                pos.z -= 20;
                pos.x -= 20;
                projectile = (Projectile)GenSpawn.Spawn(comp.PollNextProjectile, pos, caster.Map);
                projectile.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment: equipment);
            }

            return true;
        }

        private void ThrowDebugText(string text)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(caster.DrawPos, caster.Map, text);
            }
        }

        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), caster.Map, text);
            }
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            return Projectile?.projectile.explosionRadius ?? 0f;
        }

        public override bool Available()
        {
            if (!base.Available())
            {
                return false;
            }

            if (CasterIsPawn)
            {
                Pawn casterPawn = CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat &&
                    casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }

            return Projectile != null;
        }
    }
}
