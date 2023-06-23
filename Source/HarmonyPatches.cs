using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TinaTechnology
{
    [HarmonyPatch(typeof(Map), "get_PlayerWealthForStoryteller")]
    public static class TechIsWealth
    {
        static SimpleCurve wealthCurve = new SimpleCurve(new CurvePoint[] { new CurvePoint(0, 0), new CurvePoint(3800, 0), new CurvePoint(150000, 400000f), new CurvePoint(420000, 700000f), new CurvePoint(666666, 1000000f) });
        static SimpleCurve componentCurve = new SimpleCurve(new CurvePoint[] { new CurvePoint(0, 0), new CurvePoint(10, 5000), new CurvePoint(100, 25000), new CurvePoint(1000, 150000) });

        public static void Postfix(Map __instance, ref float __result)
        {
            if (Find.Storyteller.def != DataBank.StorytellerDefOf.Tina)
                return;

            float num = ResearchToWealthApparentCost();
            int numComponents = 0;
            foreach (Building building in __instance.listerBuildings.allBuildingsColonist.Where(b => b.def.costList != null))
            {
                if (building.def.costList.Any(tdc => tdc.thingDef == ThingDefOf.ComponentIndustrial))
                    numComponents++;
                if (building.def.costList.Any(tdc => tdc.thingDef == ThingDefOf.ComponentSpacer))
                    numComponents += 10;
            }
            num += componentCurve.Evaluate(numComponents);
            Log.Message("Tina Technology calculates threat points should be " + wealthCurve.Evaluate(num) + " based on " + ResearchToWealthApparentCost() + " research and " + numComponents + " component-based buildings");
            __result = wealthCurve.Evaluate(num);
        }

        static float ResearchToWealthBaseCost()
        {
            float num = 0;
            foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (proj.IsFinished)
                    num += proj.baseCost;
            }
            if (num > 100000)
                num = 100000;
            return num;
        }
        static float ResearchToWealthApparentCost()
        {
            float num = 0;
            foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (proj.IsFinished)
                    num += proj.CostApparent;
            }
            if (num > 100000)
                num = 100000;
            return num;
        }
    }
}
