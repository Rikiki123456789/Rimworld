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
        public static bool AffectGoodwillWith(Faction faction, Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
        {
            if (goodwillChange == 0)
            {
                return true;
            }
            int num = faction.GoodwillWith(other);
            int num2 = Mathf.Clamp(num + goodwillChange, -100, 100);
            if (num == num2)
            {
                return true;
            }
            FactionRelation factionRelation = faction.RelationWith(other, false);
            factionRelation.goodwill = num2;
            bool flag;
            factionRelation.CheckKindThresholds(faction, canSendHostilityLetter, reason, (!lookTarget.HasValue) ? GlobalTargetInfo.Invalid : lookTarget.Value, out flag);
            FactionRelation factionRelation2 = other.RelationWith(faction, false);
            FactionRelationKind kind = factionRelation2.kind;
            factionRelation2.goodwill = factionRelation.goodwill;
            factionRelation2.kind = factionRelation.kind;
            bool flag2;
            if (kind != factionRelation2.kind)
            {
                other.Notify_RelationKindChanged(faction, kind, canSendHostilityLetter, reason, (!lookTarget.HasValue) ? GlobalTargetInfo.Invalid : lookTarget.Value, out flag2);
            }

            if ((faction == Util_Faction.MiningCoFaction)
                && (other == Faction.OfPlayer)
                && (factionRelation.kind == FactionRelationKind.Hostile))
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
                    LordManager lordManager = map.lordManager;
                    for (int j = 0; j < lordManager.lords.Count; j++)
                    {
                        Lord lord = lordManager.lords[j];
                        if (lord.faction == Util_Faction.MiningCoFaction)
                        {
                            lord.ReceiveMemo("BecameHostileToColony");
                        }
                    }
                }
            }

            /*else
            {
                flag2 = false;
            }
            if (canSendMessage && !flag && !flag2 && Current.ProgramState == ProgramState.Playing && (faction.IsPlayer || other.IsPlayer))
            {
                Faction faction = (!this.IsPlayer) ? this : other;
                string text;
                if (!reason.NullOrEmpty())
                {
                    text = "MessageGoodwillChangedWithReason".Translate(faction.name, num.ToString("F0"), factionRelation.goodwill.ToString("F0"), reason);
                }
                else
                {
                    text = "MessageGoodwillChanged".Translate(faction.name, num.ToString("F0"), factionRelation.goodwill.ToString("F0"));
                }
                Messages.Message(text, (!lookTarget.HasValue) ? GlobalTargetInfo.Invalid : lookTarget.Value, ((float)goodwillChange <= 0f) ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.PositiveEvent, true);
            }*/
            return true;
        }



        /*public static bool AffectFactionGoodwillWithOther(Faction faction, Faction other, float goodwillChange)
        {
            float num = faction.GoodwillWith(other);
            float value = num + goodwillChange;
            if (num == value)
            {
                return true;
            }


            FactionRelation factionRelation = faction.RelationWith(other, false);
            factionRelation.goodwill = Mathf.RoundToInt(Mathf.Clamp(value, -100f, 100f));
            factionRelation.CheckKindThresholds(other)
            if (!faction.HostileTo(other) && faction.GoodwillWith(other) < -75f)
            {
                SetFactionHostileToOther(faction, other, true);
                if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenDown".Translate(faction.Name), LetterDefOf.NegativeEvent, null);
                }
            }
            if (faction.HostileTo(other) && faction.GoodwillWith(other) >= 0f)
            {
                SetFactionHostileToOther(faction, other, false);
                if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeGood".Translate(), "RelationsWarmed".Translate(faction.Name), LetterDefOf.PositiveEvent, null);
                }
            }
            return true;
        }*/

        /*public static void SetFactionHostileToOther(Faction faction, Faction other, bool hostile)
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
                if (!factionRelation.kind )
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
        }*/
    }
}
