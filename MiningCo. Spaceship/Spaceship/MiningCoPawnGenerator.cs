using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace Spaceship
{
    public static class MiningCoPawnGenerator
    {
        // Predefined colors.
        static Color colorArmyGreenBright = new Color(50f / 255f, 100f / 255f, 0);
        static Color colorArmyGreenDark = new Color(30f / 255f, 80f / 255f, 0);
        static Color colorArmyBrown = new Color(120f / 255f, 70f / 255f, 0);
        
        static Color colorArmyWhite = new Color(220f / 255f, 220f / 255f, 220f / 255f);
        static Color colorArmyGrey = new Color(200f / 255f, 200f / 255f, 200f / 255f);
        
        static Color colorArmyPaleSand = new Color(220f / 255f, 210f / 255f, 150f / 255f);
        static Color colorArmyBrownSand = new Color(215f / 255f, 180f / 255f, 120f / 255f);

        static Color colorCivilLightGrey = new Color(200f / 255f, 200f / 255f, 200f / 255f);
        static Color colorCivilGrey = new Color(160f / 255f, 160f / 255f, 160f / 255f);

        // Used colors.
        static Color pantColor;
        static Color shirtColor;
        static Color armorColor;
        static Color helmetColor;
        static bool needParka = false;

        public static Pawn GeneratePawn(PawnKindDef kindDef, Map map)
        {
            Pawn pawn = null;

            SetUniformColor(map.Biome, map.mapTemperature.SeasonalTemp);

            Predicate<Pawn> validator = null;
            if (kindDef == Util_PawnKindDefOf.Medic)
            {
                validator = delegate (Pawn medic)
                {
                    if (medic.story.WorkTagIsDisabled(WorkTags.Caring))
                    {
                        return false;
                    }
                    return true;
                };
            }
            PawnGenerationRequest request = new PawnGenerationRequest(
                kind: kindDef,
                faction: Util_Faction.MiningCoFaction,
                context: PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: true,
                colonistRelationChanceFactor: 0f,
                certainlyBeenInCryptosleep: true,
                validator: validator);
            pawn = PawnGenerator.GeneratePawn(request);
            SetMedicSkill(ref pawn);
            GeneratePawnApparelAndWeapon(ref pawn, kindDef, map.mapTemperature.SeasonalTemp);
            return pawn;
        }

        public static void SetUniformColor(BiomeDef biome, float temperature)
        {
            // Set uniforms color according to biome.
            string biomeDefName = biome.defName;
            if (biomeDefName == "IceSheet")
            {
                pantColor = colorArmyGrey;
                shirtColor = colorArmyWhite;
                armorColor = colorArmyWhite;
                helmetColor = colorArmyGrey;
                needParka = true;
            }
            else if ((biomeDefName == "AridShrubland")
                || (biomeDefName == "Desert")
                || (biomeDefName == "ExtremeDesert"))
            {
                pantColor = colorArmyBrownSand;
                shirtColor = colorArmyPaleSand;
                armorColor = colorArmyPaleSand;
                helmetColor = colorArmyBrownSand;
            }
            else if ((temperature < 0)
                && ((biomeDefName == "Tundra")
                || (biomeDefName == "BorealForest")
                || (biomeDefName == "TemperateForest")
                || (biomeDefName == "TropicalRainforest")))
            {
                // Uniform colors change according to temperature in these biomes.
                pantColor = colorArmyGrey;
                shirtColor = colorArmyWhite;
                armorColor = colorArmyWhite;
                helmetColor = colorArmyGrey;
                needParka = true;
            }
            else
            {
                pantColor = colorArmyBrown;
                shirtColor = colorArmyGreenBright;
                armorColor = colorArmyGreenBright;
                helmetColor = colorArmyGreenDark;
            }
        }

        public static void SetMedicSkill(ref Pawn pawn)
        {
            if (pawn.kindDef == Util_PawnKindDefOf.Medic)
            {
                pawn.skills.GetSkill(SkillDefOf.Medicine).passion = Passion.Major;
                if (pawn.skills.GetSkill(SkillDefOf.Medicine).Level < 14)
                {
                    pawn.skills.GetSkill(SkillDefOf.Medicine).Level = 14;
                }
            }
        }
        public static void GeneratePawnApparelAndWeapon(ref Pawn pawn, PawnKindDef kindDef, float temperature)
        {
            if (kindDef == Util_PawnKindDefOf.Technician)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), colorCivilLightGrey);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Synthread"), colorCivilLightGrey);

                if (temperature < 20f)
                {
                    // Only give a tuque if temperature is low enough.
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Tuque, ThingDef.Named("Synthread"), colorCivilLightGrey);
                }
                if (needParka)
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka, ThingDef.Named("Synthread"), armorColor);
                }
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Autopistol"));
            }
            else if (kindDef == Util_PawnKindDefOf.Miner)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), colorCivilLightGrey);
                if (Util_Misc.IsModActive("MiningCo. MiningHelmet"))
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("MiningHelmet"), null, Color.black, false);
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("MiningVest"), null, Color.black, false);
                }
                else
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), colorCivilLightGrey);
                    if (temperature < 20f)
                    {
                        // Only give a tuque if temperature is low enough.
                        GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Tuque, ThingDef.Named("Synthread"), colorCivilLightGrey);
                    }
                    if (needParka)
                    {
                        GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka, ThingDef.Named("Synthread"), armorColor);
                    }
                }
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_HeavySMG"));
            }
            else if (kindDef == Util_PawnKindDefOf.Geologist)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), colorCivilLightGrey);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Synthread"), colorCivilLightGrey);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Jacket"), ThingDef.Named("Synthread"), colorCivilGrey);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"), ThingDef.Named("Synthread"), colorCivilGrey);
                if (needParka)
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka, ThingDef.Named("Synthread"), armorColor);
                }
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_MachinePistol"));
            }
            else if (kindDef == Util_PawnKindDefOf.Medic)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Synthread"), shirtColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Duster"), ThingDef.Named("Synthread"), colorArmyWhite);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_SimpleHelmet"), ThingDefOf.Plasteel, colorArmyWhite);
                if (needParka)
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka, ThingDef.Named("Synthread"), colorArmyWhite);
                }
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_MachinePistol"));
            }
            else if ((kindDef == Util_PawnKindDefOf.Pilot)
                || (kindDef == Util_PawnKindDefOf.Scout))
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), shirtColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_VestPlate"), null, Color.black, false);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_SimpleHelmet"), ThingDefOf.Plasteel, helmetColor);
                if (needParka)
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka, ThingDef.Named("Synthread"), armorColor);
                }
                if (kindDef == Util_PawnKindDefOf.Pilot)
                {
                    GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Autopistol"));
                }
                else
                {
                    GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_AssaultRifle"));
                }
            }
            else if (kindDef == Util_PawnKindDefOf.Guard)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), shirtColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_AdvancedHelmet"), ThingDefOf.Plasteel, helmetColor);
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_ChargeRifle"));
                if (Rand.Value < 0.5f)
                {
                    // Chance to add a smokepop belt.
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_SmokepopBelt, null, armorColor);
                }
            }
            else if ((kindDef == Util_PawnKindDefOf.ShockTrooper)
                || (kindDef == Util_PawnKindDefOf.HeavyGuard))
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDef.Named("Synthread"), pantColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"), ThingDef.Named("Synthread"), shirtColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmorHelmet"), null, helmetColor);
                if (kindDef == Util_PawnKindDefOf.ShockTrooper)
                {
                    GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_ChainShotgun"));
                }
                else
                {
                    GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Minigun"));
                }
            }
            else if (kindDef == Util_PawnKindDefOf.Officer)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"), ThingDefOf.Hyperweave, pantColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"), ThingDefOf.Hyperweave, shirtColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_VestPlate"), null, Color.black, false);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Duster"), ThingDefOf.Hyperweave, armorColor);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"), ThingDefOf.Hyperweave, helmetColor);
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_SniperRifle"));
            }
            else
            {
                Log.ErrorOnce("MiningCo. Spaceship: unhandled PawnKindDef (" + kindDef.ToString() + ").", 123456786);
            }
        }
        
        public static void GeneratePawnApparel(ref Pawn pawn, QualityCategory apparelQuality, ThingDef apparelDef, ThingDef apparelStuff, Color apparelColor, bool applyColor = true)
        {
            Apparel apparel = ThingMaker.MakeThing(apparelDef, apparelStuff) as Apparel;
            if (applyColor)
            {
                apparel.SetColor(apparelColor);
            }
            apparel.TryGetComp<CompQuality>().SetQuality(apparelQuality, ArtGenerationContext.Outsider);
            pawn.apparel.Wear(apparel, false);
        }

        private static void GeneratePawnWeapon(ref Pawn pawn, QualityCategory weaponQuality, ThingDef weaponDef)
        {
            ThingWithComps weapon = ThingMaker.MakeThing(weaponDef) as ThingWithComps;
            weapon.TryGetComp<CompQuality>().SetQuality(weaponQuality, ArtGenerationContext.Outsider);
            pawn.equipment.AddEquipment(weapon);
        }
    }
}
