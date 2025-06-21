using Verse;

namespace RimValiFFARW
{
    public class AERIALChangeable : CompProperties
    {
        public int maxShellCount = 6;

        public AERIALChangeable()
        {
            compClass = typeof(AERIALChangeableProjectile);
        }
    }
}
