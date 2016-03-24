using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI; // Needed when you do something with the squad AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_Inhabitants class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_Inhabitants
    {
        public static void GenerateInhabitants(ref OG_OutpostData outpostData)
        {
            Color colorArmyGreenBright = new Color(50f / 255f, 100f / 255f, 0);
            Color colorArmyGreenDark = new Color(30f / 255f, 80f / 255f, 0);
            Color colorArmyBrown = new Color(120f / 255f, 70f / 255f, 0);

            Color colorArmyWhite = new Color(220f / 255f, 220f / 255f, 220f / 255f);
            Color colorArmyGrey = new Color(200f / 255f, 200f / 255f, 200f / 255f);

            Color colorArmyPaleSand = new Color(220f / 255f, 210f / 255f, 150f / 255f);
            Color colorArmyBrownSand = new Color(215f / 255f, 180f / 255f, 120f / 255f);
            
            Color colorCivilGrey = new Color(160f / 255f, 160f / 255f, 160f / 255f);

            if (outpostData.isInhabited == false)
            {
                return;
            }

            // Create outpost allowed area.
            Area_Allowed outpostArea;
            Find.AreaManager.TryMakeNewAllowed(AllowedAreaMode.Humanlike, out outpostArea);
            outpostArea.SetLabel(OG_Util.OutpostAreaLabel);
            for (int xOffset = 0; xOffset < OG_BigOutpost.areaSideLength; xOffset++)
            {
                for (int zOffset = 0; zOffset < OG_BigOutpost.areaSideLength; zOffset++)
                {
                    outpostArea.Set(outpostData.areaSouthWestOrigin + new IntVec3(xOffset, 0, zOffset));
                }
            }

            // Set uniforms color according to biome.
            Color pantColor;
            Color shirtColor;
            Color armorColor;
            Color helmetColor;
            bool needParka = false;
            string biomeDefName = Find.Map.Biome.defName;
            if ((biomeDefName == "IceSheet")
                || (biomeDefName == "Tundra")
                || (biomeDefName == "BorealForest"))
            {
                pantColor = colorArmyGrey;
                shirtColor = colorArmyWhite;
                armorColor = colorArmyWhite;
                helmetColor = colorArmyGrey;
                needParka = true;
            }
            else if ((biomeDefName == "AridShrubland")
                || (biomeDefName == "Desert"))
            {
                pantColor = colorArmyBrownSand;
                shirtColor = colorArmyPaleSand;
                armorColor = colorArmyPaleSand;
                helmetColor = colorArmyBrownSand;
            }
            else // TemperateForest and TropicalRainforest.
            {
                pantColor = colorArmyBrown;
                shirtColor = colorArmyGreenBright;
                armorColor = colorArmyGreenBright;
                helmetColor = colorArmyGreenDark;
            }

            // Generate technicians.
            for (int pawnIndex = 0; pawnIndex < 4; pawnIndex++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostTechnicianDef, OG_Util.FactionOfMAndCo);
                GeneratePawnApparel(ref pawn, OG_Util.OutpostTechnicianDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), colorCivilGrey);
                GeneratePawnApparel(ref pawn, OG_Util.OutpostTechnicianDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Synthread"), colorCivilGrey);
                GeneratePawnApparel(ref pawn, OG_Util.OutpostTechnicianDef.itemQuality, ThingDef.Named("Apparel_Tuque"), ThingDef.Named("Synthread"), colorCivilGrey);
                GeneratePawnWeapon(ref pawn, OG_Util.OutpostTechnicianDef.itemQuality, ThingDef.Named("Gun_Pistol"));
                if (needParka)
                {
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostTechnicianDef.itemQuality, ThingDef.Named("Apparel_Parka"), ThingDef.Named("Synthread"), colorCivilGrey);
                }
                AddJoyAndComfortNeed(pawn);
                pawn.workSettings.EnableAndInitialize();
                GenSpawn.Spawn(pawn, outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5), 0, OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5)));
                pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                pawn.playerSettings.AreaRestriction = outpostArea;
                pawn.needs.mood.thoughts.TryGainThought(OG_Util.MAndCoEmployeeThoughtDef);
            }

            // Generate outpost guards.
            List<Pawn> guardsList = new List<Pawn>();
            for (int pawnIndex = 1; pawnIndex <= 8; pawnIndex++)
            {
                Pawn pawn = null;

                if (pawnIndex == 1)
                {
                    // Generate officer.
                    do
                    {
                        pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostOfficerDef, OG_Util.FactionOfMAndCo);
                    }
                    while (pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Hunting));

                    // TODO: regenerate if pawn cannot be violent.
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Hyperweave"), pantColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Hyperweave"), shirtColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_VestPlate"), null, Color.black, false);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_Duster"), ThingDef.Named("Hyperweave"), armorColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"), ThingDef.Named("Hyperweave"), helmetColor);
                    GeneratePawnWeapon(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Gun_SniperRifle"));
                }
                else if (pawnIndex == 2)
                {
                    // Generate minigun guard.
                    do
                    {
                        pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostGuardDef, OG_Util.FactionOfMAndCo);
                    }
                    while (pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Hunting));
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), shirtColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_PowerArmorHelmet"), null, helmetColor);
                    GeneratePawnWeapon(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Gun_Minigun"));
                }
                else if (pawnIndex <= 4)
                {
                    // Generate assault rifle guard.
                    do
                    {
                        pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostGuardDef, OG_Util.FactionOfMAndCo);
                    }
                    while (pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Hunting));
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), shirtColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_VestPlate"), null, Color.black, false);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_KevlarHelmet"), null, helmetColor);
                    GeneratePawnWeapon(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Gun_AssaultRifle"));
                    if (needParka)
                    {
                        GeneratePawnApparel(ref pawn, OG_Util.OutpostTechnicianDef.itemQuality, ThingDef.Named("Apparel_Parka"), ThingDef.Named("Synthread"), armorColor);
                    }
                }
                else if (pawnIndex <= 8)
                {
                    // Generate charge rifle guard.
                    do
                    {
                        pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostGuardDef, OG_Util.FactionOfMAndCo);
                    }
                    while (pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Hunting));
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), shirtColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
                    GeneratePawnApparel(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Apparel_PowerArmorHelmet"), null, helmetColor);
                    GeneratePawnWeapon(ref pawn, OG_Util.OutpostOfficerDef.itemQuality, ThingDef.Named("Gun_ChargeRifle"));
                }
                pawn.workSettings.EnableAndInitialize();
                // Change allowed works to only firefighting and doctor (to rescue downed pawns).
                List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                for (int workTypeIndex = 0; workTypeIndex < allDefsListForReading.Count; workTypeIndex++)
                {
                    WorkTypeDef workTypeDef = allDefsListForReading[workTypeIndex];
                    if ((workTypeDef == WorkTypeDefOf.Firefighter)
                        || (workTypeDef == WorkTypeDefOf.Doctor))
                    {
                        if (pawn.story.WorkTypeIsDisabled(workTypeDef) == false)
                        {
                            pawn.workSettings.SetPriority(workTypeDef, 1);
                        }
                    }
                    else
                    {
                        pawn.workSettings.Disable(workTypeDef);
                    }
                }
                AddJoyAndComfortNeed(pawn);
                GenSpawn.Spawn(pawn, outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5), 0, OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5)));
                pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                pawn.playerSettings.AreaRestriction = outpostArea;
                pawn.needs.mood.thoughts.TryGainThought(OG_Util.MAndCoEmployeeThoughtDef);
                guardsList.Add(pawn);
            }
            // Affect squad brain to outpost guards.
            State_DefendOutpost stateDefend = new State_DefendOutpost(outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2, 0, OG_BigOutpost.areaSideLength / 2), OG_BigOutpost.areaSideLength * (3 / 4));
            StateGraph stateGraph = GraphMaker.SingleStateGraph(stateDefend);
            BrainMaker.MakeNewBrain(OG_Util.FactionOfMAndCo, stateGraph, guardsList);
        }

        private static void AddJoyAndComfortNeed(Pawn pawn)
        {
            if (pawn.needs.joy == null)
            {
                var addNeed = typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.Instance | BindingFlags.NonPublic);
                addNeed.Invoke(pawn.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Joy") });
                pawn.needs.joy.CurLevel = Rand.Range(0.5f, 1f);
            }
            if (pawn.needs.comfort == null)
            {
                var addNeed = typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.Instance | BindingFlags.NonPublic);
                addNeed.Invoke(pawn.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Comfort") });
                pawn.needs.comfort.CurLevel = Rand.Range(0.75f, 1f);
            }
        }

        private static void GeneratePawnApparel(ref Pawn pawn, QualityCategory apparelQuality, ThingDef apparelDef, ThingDef apparelStuff, Color apparelColor, bool applyColor = true)
        {
            Apparel apparel = ThingMaker.MakeThing(apparelDef, apparelStuff) as Apparel;
            if (applyColor)
            {
                apparel.SetColor(apparelColor);
            }
            apparel.TryGetComp<CompQuality>().SetQuality(apparelQuality, ArtGenerationSource.Outsider);
            pawn.apparel.Wear(apparel);
        }

        private static void GeneratePawnWeapon(ref Pawn pawn, QualityCategory weaponQuality, ThingDef weaponDef)
        {
            ThingWithComps weapon = ThingMaker.MakeThing(weaponDef) as ThingWithComps;
            weapon.TryGetComp<CompQuality>().SetQuality(weaponQuality, ArtGenerationSource.Outsider);
            pawn.equipment.AddEquipment(weapon);
        }
    }
}
