using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    public class Building_SpaceshipMedical : Building_SpaceshipCargo
    {
        public const int medicsNumber = 4;
        public const int tendableColonistCheckPeriodInTicks = 4 * GenTicks.TicksPerRealSecond;
        public const int staffAboardHealPeriodInTicks = GenDate.TicksPerHour;
        
        List<Pawn> medics = new List<Pawn>();
        public int nextTendableColonistCheckTick = 0;
        public int nextStaffAboardHealTick = 0;
        public int availableMedikitsCount = 40;
        
        public const int orbitalHealingPawnsAboardMaxCount = 6;
        public int orbitalHealingPawnsAboardCount = 0;

        // ===================== Setup work =====================
        public void InitializeData_Medical(Faction faction, int hitPoints, int landingDuration, SpaceshipKind spaceshipKind)
        {
            base.InitializeData_Cargo(faction, hitPoints, landingDuration, spaceshipKind);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (respawningAfterLoad == false)
            {
                for (int pawnindex = 0; pawnindex < medicsNumber; pawnindex++)
                {
                    Pawn medic = MiningCoPawnGenerator.GeneratePawn(Util_PawnKindDefOf.Medic, this.Map);
                    this.medics.Add(medic);
                    this.pawnsAboard.Add(medic);
                }
            }
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.KillFinalize)
            {
                float shipHealthProportion = (float)this.HitPoints / this.MaxHitPoints;
                if (shipHealthProportion < 1f)
                {
                    int delayInTicks = Mathf.RoundToInt(2f * WorldComponent_Partnership.medicalSpaceshipRequestedSupplyPeriodInTicks * (1f - shipHealthProportion));
                    Util_Misc.Partnership.nextMedicalSupplyMinTick[this.Map] += delayInTicks;
                    string spaceshipDamagedText = "-- Comlink with MiningCo. --\n\n"
                    + "\"Our medical spaceship was damaged during the last supply.\n"
                    + "Repairs will take some times.\n\n"
                    + "Remember the MiningCo. partnership contract stipulates that you must ensure landing ships security!\"\n\n"
                    + "-- End of transmission --";
                    Find.LetterStack.ReceiveLetter("Medical spaceship damaged", spaceshipDamagedText, LetterDefOf.NegativeEvent, new TargetInfo(this.Position, this.Map));
                }

                if (Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
                {
                    EjectPlayerPawns();
                }

                // Transfer player pawns for orbital healing.
                List<Pawn> leftPawnsAboard = new List<Pawn>();
                foreach (Pawn pawn in this.pawnsAboard)
                {
                    if (pawn.Faction == Faction.OfPlayer)
                    {
                        Util_Misc.OrbitalHealing.Notify_PawnStartingOrbitalHealing(pawn, this.Map);
                    }
                    else
                    {
                        leftPawnsAboard.Add(pawn);
                    }
                }
                this.pawnsAboard = leftPawnsAboard.ListFullCopy();
            }

            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Collections.Look<Pawn>(ref this.medics, "medics", LookMode.Reference);
            Scribe_Values.Look<int>(ref this.availableMedikitsCount, "availableMedikitsCount");
            Scribe_Values.Look<int>(ref this.nextStaffAboardHealTick, "nextStaffAboardHealTick");
            Scribe_Values.Look<int>(ref this.orbitalHealingPawnsAboardCount, "orbitalHealingPawnsAboardCount");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();            
            HealStaffAboard();
            SendMedicIfNeeded();
        }

        public void HealStaffAboard()
        {
            if (Find.TickManager.TicksGame >= this.nextStaffAboardHealTick)
            {
                this.nextStaffAboardHealTick = Find.TickManager.TicksGame + staffAboardHealPeriodInTicks;
                
                foreach (Pawn pawn in this.pawnsAboard)
                {
                    if (pawn.Faction == Util_Faction.MiningCoFaction)
                    {
                        if (pawn.health.HasHediffsNeedingTend())
                        {
                            Hediff hediffToHeal = pawn.health.hediffSet.GetHediffsTendable().RandomElement();
                            pawn.health.RemoveHediff(hediffToHeal);
                        }
                    }
                }
            }
        }

        public void SendMedicIfNeeded()
        {
            if (Find.TickManager.TicksGame >= this.nextTendableColonistCheckTick)
            {
                this.nextTendableColonistCheckTick = Find.TickManager.TicksGame + tendableColonistCheckPeriodInTicks;
                // Check if spaceship is about to take off.
                if (IsTakeOffImminent(2 * Util_Spaceship.medicsRecallBeforeTakeOffMarginInTicks))
                {
                    return;
                }
                // Get a healthy medic.
                Pawn medic = null;
                foreach (Pawn pawn in this.medics.InRandomOrder())
                {
                    if (this.pawnsAboard.Contains(pawn)
                        && (pawn.health.HasHediffsNeedingTend() == false))
                    {
                        medic = pawn;
                        break;
                    }
                }
                if (medic == null)
                {
                    return;
                }
                // Look for tendable colonist.
                Pawn patient = JobGiver_HealColonists.GetTendableColonist(this.Position, this.Map);
                if (patient == null)
                {
                    return;
                }
                // Spawn needed medikits if available.
                if (this.availableMedikitsCount > 0)
                {
                    int neededMedikitsCount = Medicine.GetMedicineCountToFullyHeal(patient);
                    int medikitsToSpawnCount = Math.Min(this.availableMedikitsCount, neededMedikitsCount);
                    if (medikitsToSpawnCount > 0)
                    {
                        SpawnItem(ThingDefOf.MedicineIndustrial, null, medikitsToSpawnCount, this.Position, this.Map, 0f);
                        this.availableMedikitsCount -= medikitsToSpawnCount;
                    }
                }
                // Spawn medic.
                GenSpawn.Spawn(medic, this.Position, this.Map);
                this.pawnsAboard.Remove(medic);
                Lord lord = LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_HealColonists(this.Position), this.Map);
                lord.AddPawn(medic);
            }
        }

        // ===================== Other functions =====================
        public override void Notify_PawnBoarding(Pawn pawn, bool isLastLordPawn)
        {
            base.Notify_PawnBoarding(pawn, isLastLordPawn);
            if (pawn.kindDef == Util_PawnKindDefOf.Medic)
            {
                this.medics.Add(pawn);
            }
            if (pawn.Faction == Faction.OfPlayer)
            {
                this.orbitalHealingPawnsAboardCount++;
            }
            // Note: pawns are "paused" when aboard. No need to check if they are still alive before taking off.
        }

        public override void RequestTakeOff()
        {
            base.RequestTakeOff();

            // Check medics are aboard.
            foreach (Pawn medic in this.medics)
            {
                if ((medic.Dead == false)
                    && (this.pawnsAboard.Contains(medic) == false))
                {                    
                    // Notify lords that ship is taking off soon.
                    foreach (Lord lord in this.Map.lordManager.lords)
                    {
                        if (lord.LordJob is LordJob_HealColonists)
                        {
                            this.takeOffTick = Find.TickManager.TicksGame + Util_Spaceship.medicsRecallBeforeTakeOffMarginInTicks;
                            lord.ReceiveMemo("TakeOffImminent");
                        }
                    }
                    break;
                }
            }
        }

        // ===================== Float menu options =====================
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (selPawn.CanReach(this, PathEndMode.ClosestTouch, Danger.Some, false, TraverseMode.ByPawn) == false)
            {
                return GetFloatMenuOptionsCannotReach(selPawn);
            }

            // Base options.
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
            {
                options.Add(option);
            }

            // Board to get orbital healing option.
            if (this.IsBurning())
            {
                FloatMenuOption burningOption = new FloatMenuOption("CannotUseReason".Translate("BurningLower".Translate()), null);
                options.Add(burningOption);
            }
            else
            {
                FloatMenuOption boardOption = null;
                if (Util_Misc.OrbitalHealing.HasAnyTreatableHediff(selPawn))
                {
                    if (this.orbitalHealingPawnsAboardCount >= orbitalHealingPawnsAboardMaxCount)
                    {
                        string optionLabel = "Board medical spaceship (no free slot)";
                        boardOption = new FloatMenuOption(optionLabel, null);
                    }
                    else if (TradeUtility.ColonyHasEnoughSilver(this.Map, Util_Spaceship.orbitalHealingCost))
                    {
                        Action action = delegate
                        {
                            Job job = new Job(Util_JobDefOf.BoardMedicalSpaceship, this);
                            selPawn.jobs.TryTakeOrderedJob(job);
                        };
                        string optionLabel = "Board medical spaceship (" + Util_Spaceship.orbitalHealingCost + " silver)";
                        boardOption = new FloatMenuOption(optionLabel, action);
                    }
                    else
                    {
                        string optionLabel = "Board medical spaceship (" + Util_Spaceship.orbitalHealingCost + " silver) (not enough silver)";
                        boardOption = new FloatMenuOption(optionLabel, null);
                    }
                    options.Add(boardOption);
                }
            }
            return options;
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000113;

            Command_Action ejectPlayerPawnsButton = new Command_Action();
            ejectPlayerPawnsButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
            ejectPlayerPawnsButton.defaultLabel = "Disembark";
            ejectPlayerPawnsButton.defaultDesc = "Disembark pawns awaiting orbital healing.";
            ejectPlayerPawnsButton.activateSound = SoundDef.Named("Click");
            ejectPlayerPawnsButton.action = new Action(EjectPlayerPawns);
            ejectPlayerPawnsButton.groupKey = groupKeyBase + 1;
            buttonList.Add(ejectPlayerPawnsButton);

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

        public void EjectPlayerPawns()
        {
            List<Pawn> leftPawnsAboard = new List<Pawn>();
            foreach (Pawn pawn in this.pawnsAboard)
            {
                if (pawn.Faction == Faction.OfPlayer)
                {
                    this.orbitalHealingPawnsAboardCount--;
                    GenSpawn.Spawn(pawn, this.Position + IntVec3Utility.RandomHorizontalOffset(5f), this.Map);
                    if (Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer) == false)
                    {
                        // Only give refund if not ennemy to colony! :-D
                        SpawnItem(ThingDefOf.Silver, null, Util_Spaceship.orbitalHealingCost, this.Position, this.Map, 5f);
                    }
                }
                else
                {
                    leftPawnsAboard.Add(pawn);
                }
            }
            this.pawnsAboard = leftPawnsAboard;
        }

        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            stringBuilder.AppendLine();
            if (this.orbitalHealingPawnsAboardCount > 0)
            {
                stringBuilder.Append("Pawns aboard: ");
                StringBuilder pawnsName = new StringBuilder();
                foreach (Pawn pawn in this.pawnsAboard)
                {
                    if (pawn.Faction == Faction.OfPlayer)
                    {
                        pawnsName.AppendWithComma(pawn.Name.ToStringShort);
                    }
                }
                stringBuilder.Append(pawnsName.ToString());
            }
            else
            {
                stringBuilder.Append("Pawns aboard: none");
            }
            return stringBuilder.ToString();
        }
    }
}
