using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace Common
{
    /// <summary>
    /// Building_ChargingEnergyPack class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_ChargingEnergyPack : Building
    {
        private static readonly Vector2 barSize = new Vector2(0.4f, 0.1f);
        private static readonly Material barFilledColor = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));
        private static readonly Material barUnfilledColor = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        public static readonly int maxCharge = 100;
        public int currentCharge = 0;
        public CompPowerTrader powerComponent = null;
        
        /// <summary>
        /// Spawn the charging energy pack building.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            powerComponent = base.GetComp<CompPowerTrader>();
        }

        /// <summary>
        /// Periodically update the charge. When the charge is completed, the building is destroyed to spawn an energy pack item.
        /// </summary>
        public override void TickRare()
        {
            base.TickRare();

            if (powerComponent.PowerOn == true)
            {
                currentCharge++;
            }
            if (currentCharge >= maxCharge)
            {
                this.Destroy();
            }
        }

        /// <summary>
        /// Destroy the building and spawn an energy pack item if the charge was completed.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy();

            if (currentCharge >= maxCharge)
            {
                Thing energyPack = ThingMaker.MakeThing(ThingDef.Named("EnergyPack"));
                GenSpawn.Spawn(energyPack, this.Position);
            }
        }
        
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            // Save and load the work variables, so they don't default after loading.
            Scribe_Values.LookValue<int>(ref currentCharge, "currentCharge");
        }

        /// <summary>
        /// Build the string giving some basic information that is shown when selected.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(base.GetInspectString());
            int chargeInPercent = (int)((float)currentCharge / (float)maxCharge * 100);
            stringBuilder.AppendLine("\nCharge progress: " + chargeInPercent.ToString() + "%");

            return stringBuilder.ToString();
        }

        public override void Draw()
        {
            base.Draw();
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = this.DrawPos + new Vector3(0f, 0f, -0.15f) + Vector3.up * 0.1f;
            r.size = Building_ChargingEnergyPack.barSize;
            r.fillPercent = (float)currentCharge / (float)maxCharge;
            r.filledMat = Building_ChargingEnergyPack.barFilledColor;
            r.unfilledMat = Building_ChargingEnergyPack.barUnfilledColor;
            r.margin = 0.15f;
            Rot4 rotation = base.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }
}
