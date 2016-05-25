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
        private const int supplyShipLandingPeriod = 5000; // TODO: adjust it. Every 5 days?
        private int ticksToNextSupplyShipLanding = supplyShipLandingPeriod;

        // Lord data.
        private const int lordUpdatePeriodInTicks = GenTicks.TickRareInterval; // TODO: adjust it.
        private int nextLordUpdateInTicks = lordUpdatePeriodInTicks;
        private Lord lord = null;

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

            // TODO: look for lord in existing ones.
            //Find.LordManager.lords
        }

        public override void Tick()
        {
            base.Tick();

            if ((this.Faction != null)
                && (this.Faction == OG_Util.FactionOfMAndCo))
            {
                this.ticksToNextSupplyShipLanding--;
                if (this.ticksToNextSupplyShipLanding <= 0)
                {
                    this.ticksToNextSupplyShipLanding = supplyShipLandingPeriod;
                    SpawnSupplyShip();
                }
            }
            
            if (Find.TickManager.TicksGame >= this.nextLordUpdateInTicks)
            {
                UpdateLord();
                this.nextLordUpdateInTicks = Find.TickManager.TicksGame + lordUpdatePeriodInTicks;
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
            Log.Message("UpdateLord");
            // TODO: look for ennemies in range of outpost.
            if (this.lord == null)
            {
                LordJob_Joinable_DefendOutpost lordJob = new LordJob_Joinable_DefendOutpost(OG_Util.OutpostArea.ActiveCells.RandomElement());
                this.lord = LordMaker.MakeNewLord(OG_Util.FactionOfMAndCo, lordJob);
            }
        }

        public void InitializeLandingData(IntVec3 center, Rot4 rotation)
        {
            this.landingPadCenter = center;
            this.landingPadRotation = rotation;
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

            Log.Message("Necessary reinforcements: " + this.requestedOfficersNumber + "/" + this.requestedHeavyGuardsNumber + "/" + this.requestedGuardsNumber + "/" + this.requestedScoutsNumber + "/" + this.requestedTechniciansNumber);
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
           
            Scribe_Values.LookValue<int>(ref this.requestedOfficersNumber, "requestedOfficersNumber");
            Scribe_Values.LookValue<int>(ref this.requestedHeavyGuardsNumber, "requestedHeavyGuardsNumber");
            Scribe_Values.LookValue<int>(ref this.requestedGuardsNumber, "requestedGuardsNumber");
            Scribe_Values.LookValue<int>(ref this.requestedScoutsNumber, "requestedScoutsNumber");
            Scribe_Values.LookValue<int>(ref this.requestedTechniciansNumber, "requestedTechniciansNumber");
            Scribe_Values.LookValue<int>(ref this.ticksToNextSupplyShipLanding, "ticksToNextSupplyShipLandingOn");

            Scribe_Values.LookValue<int>(ref this.nextLordUpdateInTicks, "nextLordUpdateInTicks");

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
