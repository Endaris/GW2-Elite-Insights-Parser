﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EncounterLogic.EncounterCategory;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class WvWFight : FightLogic
    {
        private readonly string _defaultName;
        private readonly bool _detailed;
        public WvWFight(int triggerID, bool detailed) : base(triggerID)
        {
            Mode = ParseMode.WvW;
            Icon = "https://wiki.guildwars2.com/images/3/35/WvW_Rank_up.png";
            _detailed = detailed;
            Extension = _detailed ? "detailed_wvw" : "wvw";
            _defaultName = _detailed ? "Detailed WvW" : "World vs World";
            EncounterCategoryInformation.Category = FightCategory.WvW;
        }

        protected override HashSet<int> GetUniqueTargetIDs()
        {
            return new HashSet<int>();
        }

        internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            NPC mainTarget = Targets.FirstOrDefault(x => x.ID == (int)ArcDPSEnums.TargetID.WorldVersusWorld);
            if (mainTarget == null)
            {
                throw new MissingKeyActorsException("Main target of the fight not found");
            }
            phases[0].AddTarget(mainTarget);
            if (!requirePhases)
            {
                return phases;
            }
            if (_detailed)
            {
                phases.Add(new PhaseData(phases[0].Start, phases[0].End)
                {
                    Name = "Detailed Full Fight",
                    CanBeSubPhase = false
                });
                phases[1].AddTargets(Targets);
                phases[1].RemoveTarget(mainTarget);
            }
            return phases;
        }

        protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
        {
            MapIDEvent mapID = log.CombatData.GetMapIDEvents().LastOrDefault();
            if (mapID == null)
            {
                return base.GetCombatMapInternal(log);
            }
            switch (mapID.MapID)
            {
                // EB
                case 38:
                    return new CombatReplayMap("https://i.imgur.com/t0khtQd.png", (954, 1000), (-36864, -36864, 36864, 36864)/*, (-36864, -36864, 36864, 36864), (8958, 12798, 12030, 15870)*/);
                // Green Alpine
                case 95:
                    return new CombatReplayMap("https://i.imgur.com/nVu2ivF.png", (697, 1000), (-30720, -43008, 30720, 43008)/*, (-30720, -43008, 30720, 43008), (5630, 11518, 8190, 15102)*/);
                // Blue Alpine
                case 96:
                    return new CombatReplayMap("https://i.imgur.com/nVu2ivF.png", (697, 1000), (-30720, -43008, 30720, 43008)/*, (-30720, -43008, 30720, 43008), (12798, 10878, 15358, 14462)*/);
                // Red Desert
                case 1099:
                    return new CombatReplayMap("https://i.imgur.com/R5p9fqw.png", (1000, 1000), (-36864, -36864, 36864, 36864)/*, (-36864, -36864, 36864, 36864), (9214, 8958, 12286, 12030)*/);
            }
            return base.GetCombatMapInternal(log);
        }
        internal override string GetLogicName(ParsedEvtcLog log)
        {
            MapIDEvent mapID = log.CombatData.GetMapIDEvents().LastOrDefault();
            if (mapID == null)
            {
                return _defaultName;
            }
            switch (mapID.MapID)
            {
                case 38:
                    EncounterCategoryInformation.SubCategory = SubFightCategory.EternalBattlegrounds;
                    return _defaultName + " - Eternal Battlegrounds";
                case 95:
                    EncounterCategoryInformation.SubCategory = SubFightCategory.GreenAlpineBorderlands;
                    return _defaultName + " - Green Alpine Borderlands";
                case 96:
                    EncounterCategoryInformation.SubCategory = SubFightCategory.BlueAlpineBorderlands;
                    return _defaultName + " - Blue Alpine Borderlands";
                case 1099:
                    EncounterCategoryInformation.SubCategory = SubFightCategory.RedDesertBorderlands;
                    return _defaultName + " - Red Desert Borderlands";
                case 899:
                    EncounterCategoryInformation.SubCategory = SubFightCategory.ObsidianSanctum;
                    return _defaultName + " - Obsidian Sanctum";
                case 968:
                    EncounterCategoryInformation.SubCategory = SubFightCategory.EdgeOfTheMists;
                    return _defaultName + " - Edge of the Mists";
            }
            return _defaultName;
        }

        internal override void CheckSuccess(CombatData combatData, AgentData agentData, FightData fightData, HashSet<AgentItem> playerAgents)
        {
            fightData.SetSuccess(true, fightData.FightEnd);
        }

        internal override void EIEvtcParse(FightData fightData, AgentData agentData, List<CombatItem> combatData, List<Player> playerList)
        {
            AgentItem dummyAgent = agentData.AddCustomAgent(fightData.FightStart, fightData.FightEnd, AgentItem.AgentType.NPC, _detailed ? "Dummy WvW Agent" : "Enemy Players", "", (int)ArcDPSEnums.TargetID.WorldVersusWorld);
            ComputeFightTargets(agentData, combatData);

            var aList = agentData.GetAgentByType(AgentItem.AgentType.EnemyPlayer).ToList();
            if (_detailed)
            {
                foreach (AgentItem a in aList)
                {
                    _targets.Add(new NPC(a));
                }
            }
            else
            {
                var enemyPlayerDicts = aList.GroupBy(x => x.Agent).ToDictionary(x => x.Key, x => x.ToList().First());
                foreach (CombatItem c in combatData)
                {
                    if (c.IsStateChange == ArcDPSEnums.StateChange.None &&
                        c.IsActivation == ArcDPSEnums.Activation.None &&
                        c.IsBuffRemove == ArcDPSEnums.BuffRemove.None &&
                        ((c.IsBuff != 0 && c.Value == 0) || (c.IsBuff == 0)))
                    {
                        if (enemyPlayerDicts.TryGetValue(c.SrcAgent, out AgentItem src))
                        {
                            c.OverrideSrcAgent(dummyAgent.Agent);
                        }
                        if (enemyPlayerDicts.TryGetValue(c.DstAgent, out AgentItem dst))
                        {
                            c.OverrideDstAgent(dummyAgent.Agent);
                        }
                    }
                }
            }

        }
    }
}
