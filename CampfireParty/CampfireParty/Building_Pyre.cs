using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound
using RimWorld;      // RimWorld specific functions are found here
//using RimWorld.SquadAI;

namespace CampfireParty
{
    /// <summary>
    /// Building_Pyre class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_Pyre : Building
    {
        protected bool campfirePartyIsStarted = false;
        protected const int maxLifeTickCounter = 7500;
        protected int lifeTickCounter = 0;
        protected Thing pyreFire = null;

        // Gizmo textures.
        protected static Texture2D startCampfirePartyGizmoIcon = ContentFinder<Texture2D>.Get("Things/Building/Misc/Campfire");
        
        // Drawing.
        public const float partyAreaRadius = 7;
        public const float beerSearchAreaRadius = 15;
        protected const int fireMaxNumber = 5;
        protected int[] fireTickCounter = new int[fireMaxNumber];
        protected Material[] fireTexture = new Material[fireMaxNumber];
        protected Material fireTextureA = MaterialPool.MatFrom("Things/Special/FireA", ShaderDatabase.Transparent);
        protected Material fireTextureB = MaterialPool.MatFrom("Things/Special/FireB", ShaderDatabase.Transparent);
        protected Material fireTextureC = MaterialPool.MatFrom("Things/Special/FireC", ShaderDatabase.Transparent);
        protected Matrix4x4[] fireMatrix = new Matrix4x4[fireMaxNumber];
        protected Vector3 fireScale = new Vector3(1f, 1f, 1f);

        // ######## SpawnSetup ######## //

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            for (int fireIndex = 0; fireIndex < fireMaxNumber; fireIndex++)
            {
                this.fireTexture[fireIndex] = null;
                this.fireTickCounter[fireIndex] = Rand.Range(5, 15);
                this.fireMatrix[fireIndex] = default(Matrix4x4);
            }
            this.fireMatrix[0].SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(-0.1f, 0.1f, -0.1f), 0f.ToQuat(), this.fireScale);
            this.fireMatrix[1].SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(-0.7f, 0.1f, -0.7f), 0f.ToQuat(), this.fireScale);
            this.fireMatrix[2].SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0.8f, 0.1f, -0.8f), 0f.ToQuat(), this.fireScale);
            this.fireMatrix[3].SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0.3f, 0.1f, 0.4f), 0f.ToQuat(), this.fireScale);
            this.fireMatrix[4].SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0f, 0.1f, 1f), 0f.ToQuat(), this.fireScale);
        }

        // ######## Tick ######## //

        public override void Tick()
        {
            base.Tick();
            if (this.campfirePartyIsStarted)
            {
                this.lifeTickCounter++;
                if (this.lifeTickCounter >= maxLifeTickCounter)
                {
                    this.Destroy(DestroyMode.Vanish);
                }
                // Update fires textures.
                for (int fireIndex = 0; fireIndex < fireMaxNumber; fireIndex++)
                {
                    if (this.lifeTickCounter >= fireIndex * (maxLifeTickCounter / fireMaxNumber))
                    {
                        this.fireTickCounter[fireIndex]--;
                        if (this.fireTickCounter[fireIndex] <= 0)
                        {
                            this.fireTickCounter[fireIndex] = Rand.Range(5, 15);
                            ChangeFireTexture(ref this.fireTexture[fireIndex]);
                        }

                    }
                }
            }
        }

        protected void ChangeFireTexture(ref Material texture)
        {
            float textureSelector = Rand.Value;
            if (textureSelector < 0.33f)
            {
                texture = fireTextureA;
            }
            else if (textureSelector < 0.66f)
            {
                texture = fireTextureB;
            }
            else
            {
                texture = fireTextureC;
            }
        }

        // ######## ExposeData ######## //

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<bool>(ref campfirePartyIsStarted, "campfirePartyIsStarted", false);
            Scribe_Values.LookValue<int>(ref lifeTickCounter, "lifeTickCounter", 0);
            Scribe_References.LookReference<Thing>(ref pyreFire, "pyreFire");
        }
        
        // ######## Destroy ######## //

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.pyreFire != null)
            {
                this.pyreFire.Destroy(DestroyMode.Vanish);
            }

            base.Destroy(mode);
        }

        // ######## Gizmos ######## //

        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000200;

            List<Gizmo> gizmoList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                gizmoList.Add(gizmo);
            }
            if (this.Faction != Faction.OfPlayer)
            {
                return gizmoList;
            }

            if (this.campfirePartyIsStarted == false)
            {
                Command_Action startCampfirePartyGizmo = new Command_Action();
                startCampfirePartyGizmo.icon = startCampfirePartyGizmoIcon;
                startCampfirePartyGizmo.defaultDesc = "Start campfire party.";
                startCampfirePartyGizmo.defaultLabel = "Start campfire party.";
                startCampfirePartyGizmo.activateSound = SoundDef.Named("Click");
                startCampfirePartyGizmo.action = new Action(TryToStartCampfireParty);
                startCampfirePartyGizmo.groupKey = groupKeyBase + 1;
                gizmoList.Add(startCampfirePartyGizmo);
            }

            // TODO: add a tribal-style party. People run in circle around pyre and get a combat bonus! Only available if allied with a local tribe.

            return gizmoList;
        }

        // ######## Drawing functions ######## //

        public override void Draw()
        {
            base.Draw();

            for (int fireIndex = 0; fireIndex < fireMaxNumber; fireIndex++)
            {
                if (this.fireTexture[fireIndex] != null)
                {
                    Graphics.DrawMesh(MeshPool.plane10, this.fireMatrix[fireIndex], this.fireTexture[fireIndex], 0);
                }
            }

            // Draw party and beer search area when selected.
            if (Find.Selector.IsSelected(this))
            {
                this.DrawPartyAndBeerSearchAreas();
            }
        }

        public void DrawPartyAndBeerSearchAreas()
        {
            List<IntVec3> partyAreaCells = this.GetPartyAreaCells();
            GenDraw.DrawFieldEdges(partyAreaCells);
            List<IntVec3> beerSearchAreaCells = this.GetBeerSearchAreaCells();
            GenDraw.DrawFieldEdges(beerSearchAreaCells);
        }

        protected List<IntVec3> GetPartyAreaCells()
        {
            IEnumerable<IntVec3> cellsInRange = GenRadial.RadialCellsAround(this.Position, partyAreaRadius, true);
            List<IntVec3> partyAreaCells = new List<IntVec3>();
            foreach (IntVec3 cell in cellsInRange)
            {
                if (cell.GetRoom(this.Map) == this.Position.GetRoom(this.Map))
                {
                    partyAreaCells.Add(cell);
                }
            }
            return partyAreaCells;
        }

        protected List<IntVec3> GetBeerSearchAreaCells()
        {
            return GenRadial.RadialCellsAround(this.Position, beerSearchAreaRadius, true).ToList<IntVec3>();
        }

        public void TryToStartCampfireParty()
        {
            // Check there are at least 2 revelers near the pyre.
            int revelersCount = 0;
            List<Pawn> revelers = new List<Pawn>();
            foreach (Pawn colonist in Find.VisibleMap.mapPawns.FreeColonists)
            {
                List<IntVec3> partyAreaCells = this.GetPartyAreaCells();
                if ((partyAreaCells.Contains(colonist.Position))
                    && colonist.Drafted)
                {
                    revelersCount++;
                    revelers.Add(colonist);
                }
            }
            if (revelersCount < 2)
            {
                Messages.Message("Not enough revelers near pyre to start party.", MessageSound.RejectInput);
                return;
            }

            this.campfirePartyIsStarted = true;
            
            // Start party with the available nearby colonists.
            foreach (Pawn reveler in revelers)
            {
                reveler.jobs.StopAll();
                reveler.drafter.Drafted = false;
                Job job = new Job(Util_CampfireParty.Job_StartCampfireParty, this);
                reveler.jobs.StartJob(job);
            }
            // Start music according to the party style.
            Find.MusicManagerPlay.ForceStartSong(DefDatabase<SongDef>.GetNamed("Moon_Harvest"), false);
            
            // Spawn the heater/glower.
            this.pyreFire = GenSpawn.Spawn(Util_CampfireParty.Def_PyreFire, this.Position, this.Map);
        }
    }
}
