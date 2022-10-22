using Verse;

namespace RimValiFFARW
{
    [StaticConstructorOnStartup]
    public static class HelloWorld
    {
        static HelloWorld()
        {
            Log.Message($"<color=orange>[RimValiFFARW]</color> Hello world!");
        }
    }
}
