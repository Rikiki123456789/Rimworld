using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI.
using Verse.AI.Group; // Needed when you do something with squad AI.
using Verse.Sound;    // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_OrbitalRelay class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_OrbitalRelay : Building
    {
        // Landing pad data.
        private IntVec3 landingPadCenter = Find.Map.Center;
        private Rot4 landingPadRotation = Rot4.North;
        private IntVec3 outpostCenter = Find.Map.Center;

        // Reinforcement data.
        public const int officersTargetNumber = 1;
        public const int heavyGuardsTargetNumber = 1;
        public const int guardsTargetNumber = 4;
        public const int scoutsTargetNumber = 2;
        public const int techniciansTargetNumber = 4;
        public int requestedOfficersNumber = 0;
        public int requestedHeavyGuardsNumber = 0;
        public int requestedGuardsNumber = 0;
        public int requestedScoutsNumber = 0;
        public int requestedTechniciansNumber = 0;
        private const int supplyShipLandingPeriodInTicks = 5000; // TODO: adjust it. Every 5 days?
        private int nextSupplyShipLandingDateInTicks = supplyShipLandingPeriodInTicks;

        // Lord data.
        private const int graceTimeInTicks = 60 * GenTicks.TicksPerRealSecond; // Grace time when game starts. Colonists are given a chance to escape alive!
        private const int lordUpdatePeriodInTicks = GenTicks.TicksPerRealSecond; // TODO: adjust it.
        private int nextLordUpdateDateInTicks = lordUpdatePeriodInTicks;

        // Dish periodical rotation.
        private const float turnRate = 0.06f;
        private const int rotationIntervalMin = 1200;
        private const int rotationIntervalMax = 2400;
        private int ticksToNextRotation = rotationIntervalMin;
        private const int rotationDurationMin = 500;
        private const int rotationDurationMax = 1000;
        private int ticksToRotationEnd = 0;
        private bool clockwiseRotation = true;

        // Power comp.
        private CompPowerTrader powerComp = null;

        // Texture.
        public static Material texture = null;

        // Sound.
        private Sustainer rotationSoundSustainer = null;

        // Texture.
        private float dishRotation = 0f;
        private static Vector3 dishScale = new Vector3(5f, 0, 5f);

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            powerComp = base.GetComp<CompPowerTrader>();
            Building_OrbitalRelay.texture = MaterialPool.MatFrom("Things/Building/Misc/OrbitalRelay");
        }

        public override void Tick()
        {
            base.Tick();

            if ((this.Faction != null)
                && (this.Faction == OG_Util.FactionOfMAndCo))
            {
                // No supply ship is sent once outpost has been captured.
                if (Find.TickManager.TicksGame >= this.nextSupplyShipLandingDateInTicks)
                {
                    this.nextSupplyShipLandingDateInTicks = Find.TickManager.TicksGame + supplyShipLandingPeriodInTicks;
                    SpawnSupplyShip();
                }
            }
            
            if (Find.TickManager.TicksGame >= this.nextLordUpdateDateInTicks)
            {
                this.nextLordUpdateDateInTicks = Find.TickManager.TicksGame + lordUpdatePeriodInTicks;
                UpdateLord();
            }

            if (powerComp.PowerOn)
            {
                UpdateDishRotation();
            }
            else
            {
                StopRotationSound();
            }
        }
        
        private void UpdateLord()
        {
            IntVec3 rallyPoint = IntVec3.Invalid;
            
            // Check there is no already existing defense lord.
            if (Find.LordManager.lords != null)
            {
                foreach (Lord lord in Find.LordManager.lords)
                {
                    if ((lord.faction != null)
                        && (lord.faction == OG_Util.FactionOfMAndCo))
                    {
                        return;
                    }
                }
            }
            // Look for hostile in outpost perimeter.
            IntVec3 hostilePosition = FindHostileInPerimeter();
            if (hostilePosition.IsValid)
            {
                if ((OG_Util.FindOutpostArea() != null)
                    && (OG_Util.FindOutpostArea().ActiveCells.Contains(hostilePosition)))
                {
                    // Ennemy is inside outpost area.
                    rallyPoint = hostilePosition;
                }
                else
                {
                    const int sectionsNumber = 100;
                    Vector3 sectionVector = (this.outpostCenter - hostilePosition).ToVector3();
                    sectionVector = sectionVector / sectionsNumber;
                    // Default value if OutpostArea does not exist (should not occur, just a safety...).
                    rallyPoint = (hostilePosition.ToVector3() + sectionVector * 0.2f * (float)sectionsNumber).ToIntVec3();
                    for (int i = 1; i <= sectionsNumber; i++)
                    {
                        Vector3 potentialRallyPoint = hostilePosition.ToVector3() + sectionVector * i;
                        if ((OG_Util.FindOutpostArea() != null)
                            && (OG_Util.FindOutpostArea().ActiveCells.Contains(potentialRallyPoint.ToIntVec3())))
                        {
                            // Ensure rallyPoint is completely inside the outpost area.
                            rallyPoint = (potentialRallyPoint + sectionVector * 0.1f * (float)sectionsNumber).ToIntVec3();
                            break;
                        }
                    }
                }
            }
            else
            {
                // Look for damaged turret to defend.
                Rot4 turretRotation = Rot4.Invalid;
                IntVec3 turretPosition = IntVec3.Invalid;
                FindDamagedTurret(out turretPosition, out turretRotation);
                if (turretPosition.IsValid)
                {
                    if (ModsConfig.IsActive("M&Co. ForceField"))
                    {
                        // Look for nearest force field to cover behind.
                        foreach (Thing thing in Find.ListerThings.ThingsOfDef(ThingDef.Named("ForceFieldGenerator")))
                        {
                            if (thing.Position.InHorDistOf(turretPosition, 23f))
                            {
                                rallyPoint = thing.Position + new IntVec3(0, 0, -2).RotatedBy(thing.Rotation);
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Just go near the attacked turret.
                        rallyPoint = turretPosition + new IntVec3(0, 0, -5).RotatedBy(turretRotation);
                    }
                }
            }
            
            if (rallyPoint.IsValid)
            {
                // Generate defense lord.
                // TODO: add a siren like in alert speaker!
                LordJob_Joinable_DefendOutpost lordJob = new LordJob_Joinable_DefendOutpost(rallyPoint);
                LordMaker.MakeNewLord(OG_Util.FactionOfMAndCo, lordJob);
                SoundDef soundDef = SoundDefOf.MessageSeriousAlert;
                soundDef.PlayOneShot(rallyPoint);
                // Stop all pawns job.
                foreach (Pawn pawn in Find.MapPawns.AllPawns)
                {
                    if ((pawn.Faction != null)
                        && (pawn.Faction == OG_Util.FactionOfMAndCo)
                        && (pawn.kindDef != OG_Util.OutpostTechnicianDef))
                    {
                        pawn.ClearMind();
                    }
                }
            }
        }

        private IntVec3 FindHostileInPerimeter()
        {
            foreach (Pawn pawn in Find.MapPawns.AllPawns)
            {
                if ((pawn.Position.IsValid == false)
                    || (pawn.Downed))
                {
                    continue;
                }
                if ((pawn.Faction != null)
                    && (pawn.Faction == Faction.OfColony)
                    && (Find.TickManager.TicksGame < graceTimeInTicks))
                {
                    continue;
                }
                if ((pawn.Faction != null)
                    && (pawn.Faction.HostileTo(OG_Util.FactionOfMAndCo)))
                {
                    if (IsInNoMansLand(pawn.Position))
                    {
                        return pawn.Position;
                    }
                }
            }
            return IntVec3.Invalid;
        }
        private bool IsInNoMansLand(IntVec3 position)
        {
            if ((position.x >= this.outpostCenter.x - (OG_BigOutpost.areaSideLength / 2 + 20))
                && (position.x <= this.outpostCenter.x + (OG_BigOutpost.areaSideLength / 2 + 20))
                && (position.z >= this.outpostCenter.z - (OG_BigOutpost.areaSideLength / 2 + 20))
                && (position.z <= this.outpostCenter.z + (OG_BigOutpost.areaSideLength / 2 + 20)))
            {
                return true;
            }
            return false;
        }

        private void FindDamagedTurret(out IntVec3 turretPosition, out Rot4 turretRotation)
        {
            turretPosition = IntVec3.Invalid;
            turretRotation = Rot4.Invalid;
            List<Thing> vulcanTurretsList = Find.ListerThings.ThingsOfDef(OG_Util.VulcanTurretDef);
            foreach (Thing turret in vulcanTurretsList)
            {
                if (turret.HitPoints < turret.MaxHitPoints)
                {
                    turretPosition = turret.Position;
                    turretRotation = turret.Rotation;
                }
            }
        }

        public void InitializeLandingAndOutpostData(IntVec3 landingPadCenter, Rot4 landingPadRotation, IntVec3 outpostCenter)
        {
            this.landingPadCenter = landingPadCenter;
            this.landingPadRotation = landingPadRotation;
            this.outpostCenter = outpostCenter;
        }

        public void UpdateRequestedReinforcements()
        {
            int officersNumber = 0;
            int heavyGuardsNumber = 0;
            int guardsNumber = 0;
            int scoutsNumber = 0;
            int techniciansNumber = 0;

            // Get the list of M&Co. pawns in the outpost area.
            foreach (Pawn pawn in Find.MapPawns.PawnsInFaction(OG_Util.FactionOfMAndCo))
            {
                if (pawn.kindDef == OG_Util.OutpostOfficerDef)
                {
                    officersNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostHeavyGuardDef)
                {
                    heavyGuardsNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostGuardDef)
                {
                    guardsNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostScoutDef)
                {
                    scoutsNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostTechnicianDef)
                {
                    techniciansNumber++;
                }
            }

            // Compute the number of necessary reinforcements.
            this.requestedOfficersNumber = officersTargetNumber - officersNumber;
            this.requestedHeavyGuardsNumber = heavyGuardsTargetNumber - heavyGuardsNumber;
            this.requestedGuardsNumber = guardsTargetNumber - guardsNumber;
            this.requestedScoutsNumber = scoutsTargetNumber - scoutsNumber;
            this.requestedTechniciansNumber = techniciansTargetNumber - techniciansNumber;
        }
        
        private void SpawnSupplyShip()
        {
            SupplyShipLandingOn supplyShip = ThingMaker.MakeThing(OG_Util.SupplyShipLandingOnDef) as SupplyShipLandingOn;
            supplyShip.InitializeLandingData(this.landingPadCenter, this.landingPadRotation);
            supplyShip.SetFaction(this.Faction);
            GenSpawn.Spawn(supplyShip, this.landingPadCenter);
        }
        
        private void UpdateDishRotation()
        {
            if (this.ticksToNextRotation > 0)
            {
                this.ticksToNextRotation--;
                if (this.ticksToNextRotation == 0)
                {
                    this.ticksToRotationEnd = Rand.RangeInclusive(rotationDurationMin, rotationDurationMax);
                    if (Rand.Value < 0.5f)
                    {
                        this.clockwiseRotation = true;
                    }
                    else
                    {
                        this.clockwiseRotation = false;
                    }
                    StartRotationSound();
                }
            }
            else
            {
                if (this.clockwiseRotation)
                {
                    this.dishRotation += turnRate;
                }
                else
                {
                    this.dishRotation -= turnRate;
                }
                this.ticksToRotationEnd--;
                if (this.ticksToRotationEnd == 0)
                {
                    this.ticksToNextRotation = Rand.RangeInclusive(rotationIntervalMin, rotationIntervalMax);
                    StopRotationSound();
                }
            }
        }
        
        private void StartRotationSound()
        {
            StopRotationSound();
            SoundInfo info = SoundInfo.InWorld(this, MaintenanceType.None);
            this.rotationSoundSustainer = this.def.building.soundDispense.TrySpawnSustainer(info);
        }

        private void StopRotationSound()
        {
            if (this.rotationSoundSustainer != null)
            {
                this.rotationSoundSustainer.End();
                this.rotationSoundSustainer = null;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            StopRotationSound();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<IntVec3>(ref this.landingPadCenter, "landingPadCenter");
            Scribe_Values.LookValue<Rot4>(ref this.landingPadRotation, "landingPadRotation");
            Scribe_Values.LookValue<IntVec3>(ref this.outpostCenter, "outpostCenter");
           
            Scribe_Values.LookValue<int>(ref this.requestedOfficersNumber, "requestedOfficersNumber");
            Scribe_Values.LookValue<int>(ref this.requestedHeavyGuardsNumber, "requestedHeavyGuardsNumber");
            Scribe_Values.LookValue<int>(ref this.requestedGuardsNumber, "requestedGuardsNumber");
            Scribe_Values.LookValue<int>(ref this.requestedScoutsNumber, "requestedScoutsNumber");
            Scribe_Values.LookValue<int>(ref this.requestedTechniciansNumber, "requestedTechniciansNumber");
            Scribe_Values.LookValue<int>(ref this.nextSupplyShipLandingDateInTicks, "nextSupplyShipLandingDateInTicks");

            Scribe_Values.LookValue<int>(ref this.nextLordUpdateDateInTicks, "nextLordUpdateDateInTicks");

            Scribe_Values.LookValue<int>(ref this.ticksToNextRotation, "ticksToNextRotation");
            Scribe_Values.LookValue<float>(ref this.dishRotation, "dishRotation");
            Scribe_Values.LookValue<bool>(ref this.clockwiseRotation, "clockwiseRotation");
            Scribe_Values.LookValue<int>(ref this.ticksToRotationEnd, "ticksToRotationEnd");
        }

        public override void Draw()
        {
            base.Draw();
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, this.dishRotation.ToQuat(), dishScale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Building_OrbitalRelay.texture, 0);
        }
    }
}
