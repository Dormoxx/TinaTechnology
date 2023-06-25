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
        static SimpleCurve techBuildingCurve = new SimpleCurve(new CurvePoint[] { new CurvePoint(0, 0), new CurvePoint(10, 5000), new CurvePoint(100, 25000), new CurvePoint(1000, 150000) });
        const int AdvancedComponentMultiplier = 10;

        public static void Postfix(Map __instance, ref float __result)
        {
            if (Find.Storyteller.def != DataBank.StorytellerDefOf.Tina)
                return;

            float wealth = ResearchToWealth();
            int numTechBuildings = 0;

            foreach (Building building in __instance.listerBuildings.allBuildingsColonist.Where(b => b.def.costList != null))
            {
                if (building.def.costList.Any(tdc => tdc.thingDef == ThingDefOf.ComponentIndustrial))
                    numTechBuildings++;
                if (building.def.costList.Any(tdc => tdc.thingDef == ThingDefOf.ComponentSpacer))
                    numTechBuildings += AdvancedComponentMultiplier;
            }

            /*
            int numComponents = 0;
            foreach (Thing t in __instance.listerThings.ThingsOfDef(ThingDefOf.ComponentIndustrial))
                numComponents += t.stackCount;
            foreach (Thing t in __instance.listerThings.ThingsOfDef(ThingDefOf.ComponentSpacer))
                numComponents += t.stackCount * AdvancedComponentMultiplier;
            Log.Message("numComponents == " + numComponents);
            */

            wealth += techBuildingCurve.Evaluate(numTechBuildings);
            __result = wealthCurve.Evaluate(wealth);
            #if DEBUG
            Log.Message("Tina Technology calculates threat points should be " + __result + " based on " + 
                ResearchToWealth() + " research and " + numTechBuildings + " component-based buildings");
            #endif
        }

        static float ResearchToWealth()
        {
            float num = 0;
            foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                //using proj.CostApparent instead of proj.baseCost for compat with tech advancing
                //as that mod changes research cost based on tech level
                if (proj.IsFinished)
                    num += proj.CostApparent; 
            }
            if (num > 100000)
            {
                #if DEBUG
                    Log.Message("ResearchToWealth().num == " + num + " before capping at 100,000");
                #endif
                num = 100000;
            }
            return num;
        }
    }
}
