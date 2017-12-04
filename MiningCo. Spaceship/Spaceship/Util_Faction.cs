using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship
{
    public static class Util_Faction
    {
        public static FactionDef MiningCoFactionDef
        {
            get
            {
                return FactionDef.Named("MiningCo");
            }
        }

        public static Faction MiningCoFaction
        {
            get
            {
                return Find.FactionManager.FirstFactionOfDef(MiningCoFactionDef);
            }
        }

        // ===================== Goodwill management =====================
        public static bool AffectFactionGoodwillWithOther(Faction faction, Faction other, float goodwillChange)
        {
            float num = faction.GoodwillWith(other);
            float value = num + goodwillChange;
            FactionRelation factionRelation = faction.RelationWith(other, false);
            factionRelation.goodwill = Mathf.Clamp(value, -100f, 100f);
            if (!faction.HostileTo(other) && faction.GoodwillWith(other) < -80f)
            {
                SetFactionHostileToOther(faction, other, true);
                if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenDown".Translate(new object[]
			        {
				        faction.Name
			        }), LetterDefOf.NegativeEvent, null);

                }
            }
            if (faction.HostileTo(other) && faction.GoodwillWith(other) >= 0f)
            {
                SetFactionHostileToOther(faction, other, false);
                if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeGood".Translate(), "RelationsWarmed".Translate(new object[]
			        {
				        faction.Name
			        }), LetterDefOf.PositiveEvent, null);
                }
            }
            return faction.def.appreciative && (goodwillChange > 0f || factionRelation.goodwill != num);
        }

        public static void SetFactionHostileToOther(Faction faction, Faction other, bool hostile)
        {
            FactionRelation factionRelation = faction.RelationWith(other, false);
            if (hostile)
            {
                if ((faction == Util_Faction.MiningCoFaction)
                    && (other == Faction.OfPlayer))
                {
                    Util_Misc.Partnership.globalGoodwillFeeInSilver = WorldComponent_Partnership.globalGoodwillCostInSilver;
                    Util_Misc.OrbitalHealing.Notify_BecameHostileToColony();
                    foreach (Map map in Find.Maps)
                    {
                        if (map.IsPlayerHome)
                        {
                            List<Building_Spaceship> takeOffRequestList = new List<Building_Spaceship>();
                            foreach (Thing thing in map.listerThings.AllThings)
                            {
                                if (thing is Building_Spaceship)
                                {
                                    takeOffRequestList.Add(thing as Building_Spaceship);
                                }
                            }
                            foreach (Building_Spaceship spaceship in takeOffRequestList)
                            {
                                spaceship.RequestTakeOff();
                            }
                        }
                    }
                }
                if (Current.ProgramState == ProgramState.Playing)
                {
                    foreach (Pawn current in PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList<Pawn>())
                    {
                        if ((current.Faction == faction && current.HostFaction == other) || (current.Faction == other && current.HostFaction == faction))
                        {
                            current.guest.SetGuestStatus(current.HostFaction, true);
                        }
                    }
                }
                if (!factionRelation.hostile)
                {
                    other.RelationWith(faction, false).hostile = true;
                    factionRelation.hostile = true;
                    if (factionRelation.goodwill > -80f)
                    {
                        factionRelation.goodwill = -80f;
                    }
                }
            }
            else
            {
                if (factionRelation.hostile)
                {
                    other.RelationWith(faction, false).hostile = false;
                    factionRelation.hostile = false;
                    if (factionRelation.goodwill < 0f)
                    {
                        factionRelation.goodwill = 0f;
                    }
                }
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    maps[i].attackTargetsCache.Notify_FactionHostilityChanged(faction, other);
                    LordManager lordManager = maps[i].lordManager;
                    for (int j = 0; j < lordManager.lords.Count; j++)
                    {
                        Lord lord = lordManager.lords[j];
                        if (lord.faction == other)
                        {
                            lord.Notify_FactionRelationsChanged(faction);
                        }
                        else
                        {
                            if (lord.faction == faction)
                            {
                                lord.Notify_FactionRelationsChanged(other);
                            }
                        }
                        if ((lord.faction == Util_Faction.MiningCoFaction)
                            && (other == Faction.OfPlayer))
                        {
                            lord.ReceiveMemo("BecameHostileToColony");
                        }
                    }
                }
            }
        }
    }
}
