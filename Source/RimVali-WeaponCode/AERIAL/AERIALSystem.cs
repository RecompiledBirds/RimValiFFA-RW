using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using RimValiFFARW.Nexus;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

// A lot of this is copied from decompiled Building_TurretGun, is that necessary?

namespace RimValiFFARW
{
    [StaticConstructorOnStartup]
    public class AERIALSystem : Building_TurretGun
    {
        private bool holdFire;
        [AllowNull]
        private CompPowerTrader power;
        [AllowNull]
        private AERIALChangeableProjectile changeableProjectile;

        public AERIALSystem()
        {
            top = new TurretTop(this);
        }

        public AERIALChangeableProjectile ChangeableProjectile
        {
            get
            {
                if (changeableProjectile == null)
                    changeableProjectile = gun.TryGetComp<AERIALChangeableProjectile>();

                return changeableProjectile;
            }
        }

        public override LocalTargetInfo CurrentTarget => currentTargetInt;


        private bool WarmingUp => burstWarmupTicksLeft > 0;


        public override Verb AttackVerb => GunCompEq.PrimaryVerb;
        private float WarmUpTicks
        {
            get
            {
                return def.building.turretBurstWarmupTime.max;
            }
        }

        private bool PlayerControlled => (Faction == Faction.OfPlayer);
        private new bool CanSetForcedTarget => PlayerControlled && ChangeableProjectile.Loaded;

        private bool CanToggleHoldFire => PlayerControlled;

        private bool CanExtractShell
        {
            get
            {
                if (!PlayerControlled)
                {
                    return false;
                }

                var compChangeableProjectile = ChangeableProjectile;
                return compChangeableProjectile != null && compChangeableProjectile.Loaded;
            }
        }




        public bool NexusConnected
        {
            get
            {
                IEnumerable<ThingWithComps> connectedNexuses = power.PowerNet.connectors
                    .Where(connector => connector.parent.def == NexusDefOf.RVFFA_Nexus)
                    .Select(compPower => compPower.parent);

                return connectedNexuses.Any(nexus =>
                    nexus.TryGetComp<CompPowerTrader>()?.PowerOn == true);
            }
        }

        public bool Powered
        {
            get
            {
                return power.PowerOn;
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            ResetCurrentTarget();

            progressBarEffecter?.Cleanup();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft");
            Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft");
            Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
            Scribe_Values.Look(ref holdFire, "holdFire");
            Scribe_Deep.Look(ref gun, "gun", []);
            BackCompatibility.PostExposeData(this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UpdateGunVerbs();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            power = this.TryGetComp<CompPowerTrader>();
            base.SpawnSetup(map, respawningAfterLoad);
        }



        public override void OrderAttack(LocalTargetInfo targ)
        {
            if (!targ.IsValid)
            {
                if (forcedTarget.IsValid)
                {
                    ResetForcedTarget();
                }

                return;
            }

            if ((targ.Cell - Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            if ((targ.Cell - Position).LengthHorizontal > AttackVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            if (forcedTarget != targ)
            {
                forcedTarget = targ;
                if (burstCooldownTicksLeft <= 0)
                {
                    TryStartShootSomethingAERIAL(false);
                }
            }

            if (holdFire)
            {
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this,
                    MessageTypeDefOf.RejectInput, false);
            }
        }

        
        public new void Tick()
        {
            if (Powered)
            {
                if (forcedTarget.IsValid && !CanSetForcedTarget)
                {
                    ResetForcedTarget();
                }

                if (!CanToggleHoldFire)
                {
                    holdFire = false;
                }

                if (forcedTarget.ThingDestroyed)
                {
                    ResetForcedTarget();
                }

                if (Active && !this.IsStunned && Spawned)
                {
                    GunCompEq.verbTracker.VerbsTick();
                    if (AttackVerb.state != VerbState.Bursting)
                    {
                        if (WarmingUp)
                        {
                            burstWarmupTicksLeft--;
                            if (burstWarmupTicksLeft == 0)
                            {
                                BeginBurst();
                            }
                        }
                        else
                        {
                            if (burstCooldownTicksLeft > 0)
                            {
                                burstCooldownTicksLeft--;
                                    progressBarEffecter ??= EffecterDefOf.ProgressBar.Spawn();

                                    progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                                    MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0])
                                        .mote;
                                    mote.progress = 1f - Math.Max(burstCooldownTicksLeft, 0) /
                                        (float)BurstCooldownTime().SecondsToTicks();
                                    mote.offsetZ = -0.8f;
                            }

                            if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                            {
                                TryStartShootSomethingAERIAL(true);
                            }
                        }

                        top.TurretTopTick();
                    }
                }
                else
                {
                    ResetCurrentTarget();
                }
            }
        }


        protected void TryStartShootSomethingAERIAL(bool canBeginBurstImmediately)
        {
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }

            if (!Spawned || holdFire && CanToggleHoldFire ||
                AttackVerb.ProjectileFliesOverhead() && Map.roofGrid.Roofed(Position) || !AttackVerb.Available())
            {
                ResetCurrentTarget();
                return;
            }

            if (forcedTarget.IsValid)
            {
                currentTargetInt = forcedTarget;
            }
            else
            {
                currentTargetInt = TryFindNewTarget();
            }

            if (currentTargetInt.IsValid)
            {
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(Position, Map));
            }
            else
            {
                ResetCurrentTarget();
                return;
            }

            if (WarmUpTicks > 0f)
            {
                burstWarmupTicksLeft =WarmUpTicks.SecondsToTicks();
                return;
            }

            if (canBeginBurstImmediately)
            {
                BeginBurst();

                return;
            }

         
            burstWarmupTicksLeft = 1;
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = "";
            AERIALChangeableProjectile compChangeableProjectile = ChangeableProjectile;
            if (!inspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspectString);
            }

            if (AttackVerb.verbProps.minRange > 0f)
            {
                stringBuilder.AppendLine("MinimumRange".Translate() + ": " +
                                         AttackVerb.verbProps.minRange.ToString("F0"));
            }


            if (compChangeableProjectile != null)
            {
                if (compChangeableProjectile.Loaded)
                {
                    stringBuilder.AppendLine("RVFFA_AERIALSystem_AERIALNextShell".Translate(compChangeableProjectile.PeekNextLabel));
                    stringBuilder.AppendLine("RVFFA_AERIALSystem_AERIALShellSpaceLeft".Translate($"{compChangeableProjectile.ShellsLoaded}".Named("RVFFA_AERIAL_SPACE"), AERIALChangeableProjectile.maxShells));
                }
                else
                {
                    stringBuilder.AppendLine("ShellNotLoaded".Translate());
                }
            }

            if (Spawned && Position.Roofed(Map))
            {
                stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (Spawned && burstCooldownTicksLeft > 0 && BurstCooldownTime() > 5f)
            {
                stringBuilder.AppendLine("CanFireIn".Translate() + ": " +
                                         burstCooldownTicksLeft.ToStringSecondsFromTicks());
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            top.DrawTurret(drawLoc,Vector3.zero,0.0f);
            base.DrawAt(drawLoc,flip);
        }

        public override void Destroy(DestroyMode mode)
        {
            if (mode != DestroyMode.Deconstruct)
            {
                foreach (ThingDef def in ChangeableProjectile.LoadedShells)
                {
                    Thing thing = GenSpawn.Spawn(def, this.Position, this.Map);
                    thing.TryGetComp<CompExplosive>().StartWick();
                }
            }

            base.Destroy(mode);

            
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (CanExtractShell)
            {
                AERIALChangeableProjectile compChangeableProjectile = ChangeableProjectile;
                yield return new Command_Action
                {
                    defaultLabel = "CommandExtractShell".Translate(),
                    defaultDesc = "CommandExtractShellDesc".Translate(),
                    icon = compChangeableProjectile.PeekNextUiIcon,
                    iconAngle = compChangeableProjectile.PeekNextUiIconAngle,
                    iconOffset = compChangeableProjectile.PeekNextUiIconOffset,
                    iconDrawScale =
                        GenUI.IconDrawScale(
                            compChangeableProjectile.PeekNextProjectile),
                    action = ExtractShell,
                };
            }

            AERIALChangeableProjectile compChangeableProjectile2 =ChangeableProjectile;
            if (compChangeableProjectile2 != null)
            {
                StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
                foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
                {
                    yield return gizmo2;
                }
            }

            if (CanSetForcedTarget)
            {
                AERIALChangeableProjectile compChangeableProjectile = ChangeableProjectile;

                var command_VerbTarget = new Command_VerbTarget
                {
                    defaultLabel = "CommandSetForceAttackTarget".Translate(),
                    defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
                    icon = compChangeableProjectile != null ? ContentFinder<Texture2D>.Get(compChangeableProjectile.Projectile?.graphicData.texPath) : ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
                    verb = AttackVerb,
                    hotKey = KeyBindingDefOf.Misc4,
                    drawRadius = false
                };
                if (Spawned && Position.Roofed(Map))
                {
                    command_VerbTarget.Disable("CannotFire".Translate() + ": " +
                                               "Roofed".Translate().CapitalizeFirst());
                }
                if (!Powered)
                {
                    command_VerbTarget.Disable("RVFFA_AERIALNotPowered".Translate().CapitalizeFirst());
                }

                yield return command_VerbTarget;
            }

            if (forcedTarget.IsValid)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "CommandStopForceAttack".Translate(),
                    defaultDesc = "CommandStopForceAttackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
                    Disabled = !Powered,
                    action = delegate
                    {
                        ResetForcedTarget();
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    },
                };
                if (!forcedTarget.IsValid)
                {
                    command_Action.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                }

                command_Action.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Action;
            }

            if (CanToggleHoldFire)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandHoldFire".Translate(),
                    defaultDesc = "CommandHoldFireDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
                    hotKey = KeyBindingDefOf.Misc6,
                    toggleAction = delegate
                    {
                        holdFire = !holdFire;
                        if (holdFire)
                        {
                            ResetForcedTarget();
                        }
                    },
                    isActive = () => holdFire,
                };
            }
        }

        private void ExtractShell()
        {
            GenPlace.TryPlaceThing(ChangeableProjectile.NewRemoveShell(), Position, Map,
                ThingPlaceMode.Near);
        }

        private void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
            {
                TryStartShootSomething(false);
            }
        }

        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }


        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
            for (var i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = BurstComplete;
            }
        }
    }
}
