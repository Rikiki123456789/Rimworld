using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class Building_SpaceshipDamaged : Building_Spaceship
    {
        public bool repairsAreStarted = false;
        public int initialHitPoints = 0;
        public Dictionary<ThingDef, int> neededMaterials = new Dictionary<ThingDef, int>();
        public Dictionary<ThingDef, int> availableMaterials = new Dictionary<ThingDef, int>();
        public const int availableMaterialsUpdatePeriodInTicks = GenTicks.TicksPerRealSecond + 2;
        public int nextAvailableMaterialsUpdateTick = 0;

        public override bool takeOffRequestIsEnabled
        {
            get
            {
                return true;
            }
        }

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            // Initialize needed materials.
            if (this.neededMaterials.Count == 0)
            {
                this.neededMaterials.Add(ThingDefOf.Component, Rand.RangeInclusive(2, 15));
                this.neededMaterials.Add(ThingDefOf.Steel, Rand.RangeInclusive(50, 250));
            }
            // Initialize available materials.
            foreach (ThingDef def in this.neededMaterials.Keys)
            {
                this.availableMaterials.Add(def, 0);
            }
        }

        public void InitializeData_Damaged(Faction faction, int hitPoints, int landingDuration, SpaceshipKind spaceshipKind, int initialHitPoints)
        {
            base.InitializeData(faction, hitPoints, landingDuration, spaceshipKind);
            this.initialHitPoints = initialHitPoints;
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.KillFinalize)
            {
                DetermineConsequences();
            }
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.repairsAreStarted, "repairsAreStarted");
            Scribe_Values.Look<int>(ref this.initialHitPoints, "initialHitPoints");
            Scribe_Collections.Look<ThingDef, int>(ref this.neededMaterials, "neededMaterials");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();
            
            if (this.DestroyedOrNull())
            {
                return;
            }
            if (Find.TickManager.TicksGame >= this.nextAvailableMaterialsUpdateTick)
            {
                this.nextAvailableMaterialsUpdateTick = Find.TickManager.TicksGame + availableMaterialsUpdatePeriodInTicks;
                UpdateAvailableMaterials();
            }
            if ((Find.TickManager.TicksGame % GenTicks.TickLongInterval) == 0)
            {
                if (this.HitPoints >= this.MaxHitPoints)
                {
                    this.RequestTakeOff();
                }
            }
        }

        // ===================== Other functions =====================
        public void UpdateAvailableMaterials()
        {
            Dictionary<ThingDef, int> localAvailableMaterials = new Dictionary<ThingDef, int>();
            foreach (ThingDef materialDef in this.neededMaterials.Keys)
            {
                localAvailableMaterials.Add(materialDef, 0);
            }
            foreach (Thing thing in GetNeededMaterialsAround())
            {
                {
                    localAvailableMaterials[thing.def] += thing.stackCount;
                }
            }
            this.availableMaterials = localAvailableMaterials;
        }

        public List<Thing> GetNeededMaterialsAround()
        {
            List<Thing> materialsAround = new List<Thing>();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, this.def.specialDisplayRadius, true))
            {
                foreach (Thing thing in cell.GetThingList(this.Map))
                {
                    if (this.neededMaterials.Keys.Contains(thing.def))
                    {
                        materialsAround.Add(thing);
                    }
                }
            }
            return materialsAround;
        }

        public bool AreNeededMaterialsAvailable()
        {
            foreach (ThingDef materialDef in this.neededMaterials.Keys)
            {
                if (this.availableMaterials[materialDef] < this.neededMaterials[materialDef])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Deliver needed materials and set faction to enable repairs.
        /// </summary>
        public void TryStartRepairs()
        {
            UpdateAvailableMaterials();
            if (AreNeededMaterialsAvailable() == false)
            {
                Messages.Message("Not enough materials to perform repairs.", this, MessageTypeDefOf.RejectInput);
                return;
            }
            this.repairsAreStarted = true;
            Dictionary<ThingDef, int> remainingNeededMaterials = new Dictionary<ThingDef, int>(this.neededMaterials);
            foreach (ThingDef materialDef in this.neededMaterials.Keys)
            {
                foreach (Thing thing in GetNeededMaterialsAround())
                {
                    if (thing.def == materialDef)
                    {
                        if (thing.stackCount > remainingNeededMaterials[materialDef])
                        {
                            thing.stackCount -= remainingNeededMaterials[materialDef];
                            remainingNeededMaterials[materialDef] = 0;
                        }
                        else
                        {
                            remainingNeededMaterials[materialDef] -= thing.stackCount;
                            thing.Destroy();
                        }
                        if (remainingNeededMaterials[materialDef] == 0)
                        {
                            break;
                        }
                    }
                }
            }
            this.SetFaction(Faction.OfPlayer);
            Messages.Message("Needed materials have been transferred to the damaged spaceship. You can now begin repairs.", this, MessageTypeDefOf.PositiveEvent);
        }

        public void DetermineConsequences()
        {
            if (this.repairsAreStarted)
            {
                if (this.HitPoints == this.MaxHitPoints)
                {
                    string letterText = "-- Comlink with MiningCo. --\n\n"
                        + "MiningCo. pilot:\n\n"
                        + "\"We are really grateful for your help, partner!\n"
                        + "Tonight, we will drink to your colony in the orbital ship mess.\n"
                        + "Here, take those crates as a compensation for your efforts. You have well earned it.\n\n"
                        + "See you soon partner!\"\n\n"
                        + "-- End of transmission --";
                    Find.LetterStack.ReceiveLetter("Repairs completed", letterText, LetterDefOf.PositiveEvent, new TargetInfo(this.Position, this.Map));
                    SpawnCargoContent(1f);
                    Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, 10f);
                }
                else if (this.HitPoints >= this.initialHitPoints)
                {
                    string letterText = "-- Comlink with MiningCo. --\n\n"
                        + "MiningCo. pilot:\n\n"
                        + "\"We are grateful for your help, partner!\n"
                        + "This is not perfect but at least, you tried.\n"
                        + "Here, take those crates as a compensation for your efforts.\n\n"
                        + "See you partner!\"\n\n"
                        + "-- End of transmission --";
                    Find.LetterStack.ReceiveLetter("Partial repairs", letterText, LetterDefOf.PositiveEvent, new TargetInfo(this.Position, this.Map));
                    SpawnCargoContent(0.5f);
                    Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, 5f);
                }
                else
                {
                    string letterText = "-- Comlink with MiningCo. --\n\n"
                        + "MiningCo. pilot:\n\n"
                        + "\"Well, it seems the repairs did not go as we expected.\n"
                        + "You have supplied us the materials but our ship is in worse condition than when we landed.\n"
                        + "You should really review the MiningCo. spaceship maintenance manual!\"\n\n"
                        + "See you partner!\"\n\n"
                        + "-- End of transmission --";
                    Find.LetterStack.ReceiveLetter("Failed repairs", letterText, LetterDefOf.NeutralEvent, new TargetInfo(this.Position, this.Map));
                }
                // No relation impact.
            }
            else
            {
                if (this.HitPoints >= this.initialHitPoints)
                {
                    string letterText = "-- Comlink with MiningCo. --\n\n"
                        + "MiningCo. pilot:\n\n"
                        + "\"Well, it seems you cannot help us. Headquarters will not be pleased to hear that.\n"
                        + "MiningCo. does not bother making business with weak partners.\n\n"
                        + "Do not count on our services for some time, \"partner\".\"\n\n"
                        + "-- End of transmission --\n\n"
                        + "Next cargo supply is delayed.\n"
                        + "Air strike is not available for some time.";
                    Find.LetterStack.ReceiveLetter("Repairs?!", letterText, LetterDefOf.NegativeEvent, new TargetInfo(this.Position, this.Map));
                    Util_Misc.Partnership.nextPeriodicSupplyTick[this.Map] = Find.TickManager.TicksGame + 2 * WorldComponent_Partnership.cargoSpaceshipPeriodicSupplyPeriodInTicks;
                    Util_Misc.Partnership.nextRequestedSupplyMinTick[this.Map] = Find.TickManager.TicksGame + 2 * WorldComponent_Partnership.cargoSpaceshipRequestedSupplyPeriodInTicks;
                    Util_Misc.Partnership.nextAirstrikeMinTick[this.Map] = Find.TickManager.TicksGame + 20 * GenDate.TicksPerDay;
                    Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, -10f);
                }
                else
                {
                    string letterText = "-- Comlink with MiningCo. --\n\n"
                        + "MiningCo. pilot:\n\n"
                        + "\"I can't believe it! Our ship is in worse condition than when we landed.\n"
                        + "I am not supplying this rathole any soon!\"\n\n"
                        + "(You can hear insults before the pilot disconnects.)\n\n"
                        + "-- End of transmission --\n\n"
                        + "Next cargo supply is greatly delayed.\n"
                        + "Air strike is not available for a long time.";
                    Find.LetterStack.ReceiveLetter("Repairs?!", letterText, LetterDefOf.NegativeEvent, new TargetInfo(this.Position, this.Map));
                    Util_Misc.Partnership.nextPeriodicSupplyTick[this.Map] = Find.TickManager.TicksGame + 4 * WorldComponent_Partnership.cargoSpaceshipPeriodicSupplyPeriodInTicks;
                    Util_Misc.Partnership.nextRequestedSupplyMinTick[this.Map] = Find.TickManager.TicksGame + 4 * WorldComponent_Partnership.cargoSpaceshipRequestedSupplyPeriodInTicks;
                    Util_Misc.Partnership.nextAirstrikeMinTick[this.Map] = Find.TickManager.TicksGame + 40 * GenDate.TicksPerDay;
                    Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, -20f);
                }
            }
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000106;

            if (this.repairsAreStarted == false)
            {
                Command_Action startRepairsButton = new Command_Action();
                startRepairsButton.icon = ContentFinder<Texture2D>.Get("Things/Item/Resource/MedicineGlitterworld");
                startRepairsButton.defaultLabel = "Start repairs";
                startRepairsButton.defaultDesc = "Deliver needed materials to repair the spaceship.";
                startRepairsButton.activateSound = SoundDef.Named("Click");
                startRepairsButton.action = new Action(TryStartRepairs);
                startRepairsButton.groupKey = groupKeyBase + 1;
                buttonList.Add(startRepairsButton);
            }

            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = basebuttonList.Concat(buttonList);
            }
            else
            {
                resultButtonList = buttonList;
            }
            return resultButtonList;
        }

        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (this.repairsAreStarted == false)
            {
                stringBuilder.Append("Needed materials:");
                foreach (ThingDef materialDef in this.neededMaterials.Keys)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append(materialDef.LabelCap + ": " + this.availableMaterials[materialDef] + "/" + this.neededMaterials[materialDef]);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
