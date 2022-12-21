using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class PackDef : Def
    {
        public Type packWorkerType = typeof(PackWorker);

        public int maxSize = 5;
        public int MaxSize => maxSize;

        public List<HediffDef> memberHediffs = new List<HediffDef>();

        public PackWorker GetNewPackWorker() => packWorkerType.GetConstructor(new Type[] { typeof(PackDef) }).Invoke(new object[] { this }) as PackWorker;
    }
}
