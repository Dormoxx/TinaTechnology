using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TinaTechnology
{
    [HarmonyPatch(typeof(Map), "get_PlayerWealthForStoryteller")]
    public static class TechIsWealth
    {
        /*
         * vanilla starter wealth:
         *      Tribal: 2800 research, 0 threat
         *      Crashlanded: 4100 research, ~8982 threat points, 30 components
         * modpack starter wealth:
         *      Tribal: 3200 research
         *      Crashlanded: 3582 research, ~7533 threat points, 30 components
         * 
         * vanilla points per wealth curve:
         *      (0, 0)
         *      (14000, 0)
         *      (400000, 2,400)
         *      (700000, 3,600)
         *      (1000000, 4,200)
         */
        static SimpleCurve wealthCurve = new SimpleCurve(
            new CurvePoint[] { 
                new CurvePoint(0, 0),
                new CurvePoint(5000, 0),
                new CurvePoint(150000, 400000f),
                new CurvePoint(420000, 700000f),
                new CurvePoint(666666, 1000000f)
            });
        static SimpleCurve techBuildingCurve = new SimpleCurve(
            new CurvePoint[] {
                new CurvePoint(0, 0),
                new CurvePoint(10, 5000),
                new CurvePoint(100, 25000),
                new CurvePoint(1000, 150000)
            });
        const int AdvancedComponentMultiplier = 10;

        public static void Postfix(Map __instance, ref float __result)
        {
            #if DEBUG
                Log.Message("Wealth calculated as " + __result + " before Tina Technology Postfix");
            #endif

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


            float numComponents = 0;
            foreach (Thing t in __instance.listerThings.ThingsOfDef(ThingDefOf.ComponentIndustrial))
                numComponents += (t.stackCount * 0.5f);
            foreach (Thing t in __instance.listerThings.ThingsOfDef(ThingDefOf.ComponentSpacer))
                numComponents += ((t.stackCount * AdvancedComponentMultiplier) * 0.5f);


            wealth += techBuildingCurve.Evaluate(numTechBuildings);
            wealth += techBuildingCurve.Evaluate(numComponents);
            __result = wealthCurve.Evaluate(wealth);
            #if DEBUG
            Log.Message("Tina Technology calculates threat points should be " + __result + 
                        " based on " + ResearchToWealth() + " research, "
                        + numTechBuildings + " component-based buildings and "
                        + numComponents * 2 + " components.");
            #endif
        }

        static float ResearchToWealth()
        {
            float num = 0;
            foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                //using proj.CostApparent instead of proj.baseCost for compat with
                //tech advancing as that mod changes research cost based on tech level
                if (proj.IsFinished)
                    num += proj.CostApparent; 
            }
            //vanilla total research tree == 190,000
            //modpack total reseach tree == 292,139.1 
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
