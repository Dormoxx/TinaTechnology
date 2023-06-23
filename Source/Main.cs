using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace TinaTechnology
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("dormoxx.tinatechnology");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message(string.Format("[Tina Technology] : finished {0} Harmony patches.",
                harmony.GetPatchedMethods().Select(
                    new Func<MethodBase, Patches>(Harmony.GetPatchInfo))
                    .SelectMany((Patches p) => p.Prefixes.Concat(p.Postfixes))
                    .Count((Patch p) => p.owner.Contains(harmony.Id))));
        }
    }
}
