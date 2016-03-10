using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI;
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_OutpostCommandConsole class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_OutpostCommandConsole : Building_CommsConsole
    {
        public List<Thing> outpostThingList = null;
        public IntVec3 dropZoneCenter = Find.Map.Center;
        protected int ticksSinceMealsDrop = 0;

        // TODO: remove this. Just a patch until ship is designed.
        public override void Tick()
        {
            ticksSinceMealsDrop++;
            if (ticksSinceMealsDrop >= 200)
            {
                ticksSinceMealsDrop = 0;
                DropPodInfo info = new DropPodInfo();
                Thing meals = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                meals.stackCount = ThingDefOf.MealSurvivalPack.stackLimit;
                meals.SetForbidden(true);
                info.SingleContainedThing = meals;
                DropPodUtility.MakeDropPodAt(this.dropZoneCenter + new IntVec3(Rand.RangeInclusive(-2, 2), 0, Rand.RangeInclusive(-2, 2)), info);
            }
        }

        public void TryToCaptureOutpost(string eventTitle, string eventText, LetterType letterType, Faction turretsNewFaction, bool deactivateTurrets,
            Faction doorsNewFaction, bool deactivateDoors, int dropPodsNumber, PawnKindDef securityForcesDef)
        {
            this.outpostThingList = OG_Util.RefreshThingList(this.outpostThingList);
            ChangeOutpostThingsFaction(null);
            ChangeOutpostTurretsFaction(turretsNewFaction, deactivateTurrets);
            ChangeOutpostDoorsFaction(doorsNewFaction, deactivateDoors);
            LaunchSecurityDropPods(dropPodsNumber, securityForcesDef, true);
            Area outpostArea = Find.AreaManager.GetLabeled(OG_Util.OutpostAreaLabel);
            if (outpostArea != null)
            {
                outpostArea.Delete();
            }
            Find.LetterStack.ReceiveLetter(eventTitle, eventText, letterType, new TargetInfo(this.Position));
        }

        public void ChangeOutpostThingsFaction(Faction newFaction)
        {
            if (this.outpostThingList.NullOrEmpty())
            {
                return;
            }
            foreach (Thing thing in this.outpostThingList)
            {
                thing.SetFaction(newFaction);
            }
        }

        void ChangeOutpostTurretsFaction(Faction turretsNewFaction, bool deactivateTurrets)
        {
            if (this.outpostThingList.NullOrEmpty())
            {
                return;
            }
            foreach (Thing thing in this.outpostThingList)
            {
                if (thing is Building_TurretGun)
                {
                    thing.SetFaction(turretsNewFaction);
                    if (deactivateTurrets)
                    {
                        CompPowerTrader powerComp = thing.TryGetComp<CompPowerTrader>();
                        if (powerComp != null)
                        {
                            powerComp.DoFlick();
                        }
                    }
                }
            }            
        }

        void ChangeOutpostDoorsFaction(Faction doorsNewFaction, bool deactivateDoors)
        {
            if (this.outpostThingList.NullOrEmpty())
            {
                return;
            }
            foreach (Thing thing in this.outpostThingList)
            {
                if (thing.def == ThingDef.Named("Autodoor"))
                {
                    thing.SetFaction(doorsNewFaction);
                    if (deactivateDoors)
                    {
                        CompPowerTrader powerComp = thing.TryGetComp<CompPowerTrader>();
                        if (powerComp != null)
                        {
                            powerComp.DoFlick();
                            (thing as Building_Door).StartManualOpenBy(null);
                        }
                    }
                }
            }
        }

        void LaunchSecurityDropPods(int dropPodsNumber, PawnKindDef securityForcesDef, bool assaultColony)
        {
            IntVec3 dropPodSpot;
            List<Pawn> securityForcesList = new List<Pawn>();
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile);

            if ((dropPodsNumber == 0) || (securityForcesDef == null))
            {
                return;
            }

            for (int soldierIndex = 0; soldierIndex < dropPodsNumber; soldierIndex++)
            {
                bool validDropPodCellIsFound = DropCellFinder.TryFindDropSpotNear(this.dropZoneCenter, out dropPodSpot, true, false);
                if (validDropPodCellIsFound)
                {
                    Pawn soldier = PawnGenerator.GeneratePawn(securityForcesDef, faction);
                    securityForcesList.Add(soldier);
                    DropPodUtility.MakeDropPodAt(dropPodSpot, new DropPodInfo
                    {
                        SingleContainedThing = soldier,
                        openDelay = 240,
                        leaveSlag = false
                    });
                }
            }
            StateGraph stateGraph = null;
            if (assaultColony)
            {
                stateGraph = GraphMaker.AssaultColonyGraph(faction, true, true);
            }
            else
            {
                stateGraph = GraphMaker.MechanoidsDefendShipGraph(this, 10f);
            }
            BrainMaker.MakeNewBrain(faction, stateGraph, securityForcesList);
        }
        
        public void TreatIntrusion(IntVec3 intrusionCell)
        {
            OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).goodwill = OG_Util.FactionOfMAndCo.def.startingGoodwill.min;
            Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).goodwill = OG_Util.FactionOfMAndCo.def.startingGoodwill.min;
            OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).hostile = true;
            Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).hostile = true;
            
            string text = "   M&Co. security message broadcast\n\n" +
                "Coralie here!\n" +
                "I have detected an intrusion in sub-sector " + intrusionCell.ToString() + ".\n\n" +
                "To all units in the sector, code Red is activated. All intruders are now priority targets.\n\n" +
                "--- End of transmission ---";
            Find.LetterStack.ReceiveLetter("Intrusion", text, LetterType.BadUrgent, new TargetInfo(intrusionCell));
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);

            this.outpostThingList = OG_Util.RefreshThingList(this.outpostThingList);
            string eventTitle = "Coralie out";
            string eventText = "   M&Co. system maintenance request\n\n" +
                "Request author: Coralie\n" +
                "Function: M&Co. outpost AI\n\n" +
                "Defect description:\n" +
                "Coralie here. I detect several severe dysfunctions.\n" +
                "- video sensors:    LINK DAMAGED\n" +
                "- threat sensors:   NO RESPONSE\n" +
                "- security systems: OFFLINE\n" +
                "- power status:     INTERNAL BAT LVL CRITICAL\n\n" +
                "I urgently request the sending of a repair tea-\n\n" +
                "*Grrz*... *Pchii*... *Fzzt*\n\n" +
                "---- End of transmision ---";
            if (this.Faction == Faction.OfColony)
            {
                Find.LetterStack.ReceiveLetter(eventTitle, eventText, LetterType.BadUrgent);
            }
            else
            {
                ChangeOutpostThingsFaction(null);
                LaunchSecurityDropPods(4, PawnKindDef.Named("MercenaryGunner"), false);
                Find.LetterStack.ReceiveLetter(eventTitle, eventText, LetterType.BadNonUrgent);
            }
            ChangeOutpostTurretsFaction(null, true);
            ChangeOutpostDoorsFaction(null, true);
            Thing coralie = ThingMaker.MakeThing(ThingDef.Named("AIPersonaCore"));
            GenSpawn.Spawn(coralie, this.Position);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            this.outpostThingList = OG_Util.RefreshThingList(this.outpostThingList);
            Scribe_Collections.LookList<Thing>(ref this.outpostThingList, "outpostThingList", LookMode.MapReference);
            Scribe_Values.LookValue<IntVec3>(ref this.dropZoneCenter, "dropZoneCenter");
            Scribe_Values.LookValue<int>(ref this.ticksSinceMealsDrop, "ticksUntilMealsDrop");
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (this.Faction == Faction.OfColony)
            {
                return base.GetFloatMenuOptions(myPawn);
            }
            else
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                CompPowerTrader powerComp = this.TryGetComp<CompPowerTrader>();
                if (myPawn.Dead
                    || myPawn.Downed
                    || myPawn.IsBurning())
                {
                    FloatMenuOption item = new FloatMenuOption("Cannot use (incapacitated)", null);
                    list.Add(item);
                }

                if (myPawn.CanReserve(this) == false)
                {
                    FloatMenuOption item = new FloatMenuOption("Cannot use (reserved)", null);
                    list.Add(item);
                }
                else if (myPawn.CanReach(this, PathEndMode.ClosestTouch, Danger.Some) == false)
                {
                    FloatMenuOption item = new FloatMenuOption("Cannot use (no path)", null);
                    list.Add(item);
                }
                else if (this.IsBurning())
                {
                    FloatMenuOption item = new FloatMenuOption("Cannot use (burning)", null);
                    list.Add(item);
                }
                else if (powerComp.PowerOn == false)
                {
                    FloatMenuOption item = new FloatMenuOption("Cannot use (no power)", null);
                    list.Add(item);
                }
                else
                {
                    Action action = delegate
                    {
                        Job job = new Job(DefDatabase<JobDef>.GetNamed(OG_Util.JobDefName_TryToCaptureOutpost), this);
                        myPawn.drafter.TakeOrderedJob(job);
                    };
                    FloatMenuOption item2 = new FloatMenuOption("Try to capture outpost", action);
                    list.Add(item2);
                }
                return list;
            }
        }
    }
}
