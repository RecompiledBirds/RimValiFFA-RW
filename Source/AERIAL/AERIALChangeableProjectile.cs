using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace RimValiFFARW
{
    public class AERIALChangeableProjectile : CompChangeableProjectile
    {
        public static int maxShells = 6;

        [AllowNull]
        public new StorageSettings allowedShellsSettings;

        private List<ThingDef> loadedShells = new List<ThingDef>();

        public List<ThingDef> LoadedShells { get { return loadedShells; } }

        public new AERIALChangeable Props => (AERIALChangeable)props;

        public new ThingDef? Projectile => !Loaded ? null : PeekNextProjectile;

        public new bool Loaded => loadedShells.Count > 0;

        public new bool StorageTabVisible => true;

        public bool FullyLoaded => loadedShells.Count >= maxShells;

        public string PeekNextLabel
        {
            get
            {
                ThingDef? thing = PeekNextProjectile;
                if (thing == null) return "None";
                return thing.LabelCap;
            }
        }
        public ThingDef? PeekNextProjectile
        {
            get
            {
                if (loadedShells.NullOrEmpty())
                    return null;
                ThingDef res = loadedShells[0].projectileWhenLoaded;
                return res;
            }
        }

        public int ShellsLoaded
        {
            get
            {
                return loadedShells.Count;
            }
        }

        public ThingDef? PollNextProjectile
        {
            get
            {
                ThingDef? res = PeekNextProjectile;
                if (res != null)
                    loadedShells.RemoveAt(0);
                return res;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref allowedShellsSettings, "allowedShellsSettings", Array.Empty<object>());
            Scribe_Collections.Look(ref loadedShells, "loadedShells");
            if (loadedShells == null)
            {
                loadedShells = new List<ThingDef>();
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            allowedShellsSettings = new StorageSettings(this);
            if (parent.def.building.defaultStorageSettings != null)
            {
                allowedShellsSettings.CopyFrom(parent.def.building.defaultStorageSettings);
            }
        }


        // Token: 0x060085FF RID: 34303 RVA: 0x00059D03 File Offset: 0x00057F03
        public override void Notify_ProjectileLaunched()
        {
            if (loadedCount > 0)
            {
                loadedCount--;
            }

            if (loadedCount <= 0)
            {
                loadedShells.RemoveAt(loadedCount - 1);
            }
        }

        public void NewLoadShell(ThingDef shell, int count)
        {
            for (int i = 0; i < count;i++)
            {
                loadedShells.Add(shell);
            }
        }

        public Thing NewRemoveShell()
        {
            Thing thing = ThingMaker.MakeThing(loadedShells[loadedShells.Count - 1]);
            thing.stackCount = 1;
            loadedShells.RemoveAt(loadedShells.Count - 1);
            return thing;
        }

        public new StorageSettings GetStoreSettings()
        {
            return allowedShellsSettings;
        }

        // Token: 0x06008603 RID: 34307 RVA: 0x00059D79 File Offset: 0x00057F79
        public new StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }
    }
}
