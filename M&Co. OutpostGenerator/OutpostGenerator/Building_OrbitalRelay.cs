using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

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

        // List of reinforcement requests (list of pawn types who should arrive in next supply ship).
        private List<string> reinforcementRequestsList = new List<string>();
        private const int supplyShipLandingOnPeriod = 5000; // TODO: adjust it. Every 5 days?
        private int ticksToNextSupplyShipLandingOn = supplyShipLandingOnPeriod;
        private const int troopsReviewPeriod = 250; // TODO: adjust it. Every 10 days?
        private int ticksToNextTroopsReview = troopsReviewPeriod;

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
                this.ticksToNextSupplyShipLandingOn--;
                if (this.ticksToNextSupplyShipLandingOn <= 0)
                {
                    this.ticksToNextSupplyShipLandingOn = supplyShipLandingOnPeriod;
                    SpawnSupplyShip();
                    UnforbidAnyWeaponOrApparelInOutpost();
                }

                this.ticksToNextTroopsReview--;
                if (ticksToNextTroopsReview <= 0)
                {
                    this.ticksToNextTroopsReview = troopsReviewPeriod;
                    PerformTroopsReview();
                }
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
        
        public void InitializeLandingData(IntVec3 center, Rot4 rotation)
        {
            this.landingPadCenter = center;
            this.landingPadRotation = rotation;
        }

        public void RequestReinforcement(PawnKindDef pawnType)
        {
            Log.Message("Adding reinforcement request " + pawnType.ToString());
            this.reinforcementRequestsList.Add(pawnType.ToString());
        }

        public void GetAndClearReinforcementRequestsList(out List<PawnKindDef> reinforcementRequestsList)
        {
            reinforcementRequestsList = new List<PawnKindDef>();
            foreach (string pawnType in this.reinforcementRequestsList)
            {
                reinforcementRequestsList.Add(PawnKindDef.Named(pawnType));
            }
            this.reinforcementRequestsList.Clear();
        }

        private void SpawnSupplyShip()
        {
            SupplyShipLandingOn supplyShip = ThingMaker.MakeThing(OG_Util.SupplyShipLandingOnDef) as SupplyShipLandingOn;
            supplyShip.InitializeLandingData(this.landingPadCenter, this.landingPadRotation);
            supplyShip.SetFaction(this.Faction);
            GenSpawn.Spawn(supplyShip, this.landingPadCenter);
        }

        private void UnforbidAnyWeaponOrApparelInOutpost()
        {
            // TODO: unforbid weapon or apparel so it can be carried to supply ship.
        }

        private void PerformTroopsReview()
        {
            // TODO: count alive M&Co. emmployees + the ones stored in cryptosleep sarcophagi. Add missing ones to the reinforcementRequestsList.
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
            Scribe_Values.LookValue<int>(ref this.ticksToNextSupplyShipLandingOn, "ticksToNextSupplyShipLandingOn");
            Scribe_Values.LookValue<int>(ref this.ticksToNextTroopsReview, "ticksToNextTroopsReview");
            Scribe_Values.LookValue<int>(ref this.ticksToNextRotation, "ticksToNextRotation");
            Scribe_Values.LookValue<float>(ref this.dishRotation, "dishRotation");
            Scribe_Values.LookValue<bool>(ref this.clockwiseRotation, "clockwiseRotation");
            Scribe_Values.LookValue<int>(ref this.ticksToRotationEnd, "ticksToRotationEnd");
            Scribe_Collections.LookList<string>(ref this.reinforcementRequestsList, "requestedReinforcementList", LookMode.Value);
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
