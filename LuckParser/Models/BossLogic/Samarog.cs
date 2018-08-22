﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models
{
    public class Samarog : BossLogic
    {
        public Samarog() : base()
        {
            mode = ParseMode.Raid;
            mechanicList.AddRange(new List<Mechanic>
            {

            new Mechanic(37996, "Shockwave", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'circle',color:'rgb(0,0,255)',", "Shkwv",0), //Shockwave from Spears, Shockwave
            new Mechanic(38168, "Prisoner Sweep", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'hexagon',color:'rgb(0,0,255)',", "Swp",0), //Prisoner Sweep (horizontal), Sweep
            new Mechanic(38305, "Bludgeon", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'triangle-down',color:'rgb(0,0,255)',", "Slm",0), //Bludgeon (vertical Slam), Slam
            new Mechanic(37868, "Fixate: Samarog", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'star',color:'rgb(255,0,255)',", "S.Fix",0), //Fixated by Samarog, Fixate: Samarog
            new Mechanic(38223, "Fixate: Guldhem", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'star-open',color:'rgb(255,200,0)',", "G.Fix",0), //Fixated by Guldhem, Fixate: Guldhem
            new Mechanic(37693, "Fixate: Rigom", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'star-open',color:'rgb(255,0,0)',", "R.Fix",0), //Fixated by Rigom, Fixate: Rigom
            new Mechanic(37966, "Big Hug", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'circle',color:'rgb(0,128,0)',", "BgGrn",0), // Big Green (friends mechanic), Big Green
            new Mechanic(38247, "Small Hug", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'circle-open',color:'rgb(0,128,0)',", "SmGrn",0), //Small Green (friends mechanic), Small Green
            new Mechanic(38180, "Spear Return", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'triangle-left',color:'rgb(255,0,0)',", "SprRtn",0), //Hit by Spear Return, Spear Return
            new Mechanic(38260, "Inevitable Betrayal", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'circle',color:'rgb(255,0,0)',", "G.Fail",0), //Inevitable Betrayal (failed Green), Failed Green
            new Mechanic(37851, "Inevitable Betrayal", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'circle',color:'rgb(255,0,0)',", "G.Fail",0), //Inevitable Betrayal (failed Green), Failed Green
            new Mechanic(37901, "Effigy Pulse", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'triangle-down-open',color:'rgb(255,0,0)',", "S.Pls",0), //Effigy Pulse (Stood in Spear AoE), Spear Aoe
            new Mechanic(37816, "Spear Impact", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'triangle-down',color:'rgb(255,0,0)',", "S.Spwn",0), // Spear Impact (hit by spawning Spear), Spear Spawned
            new Mechanic(38199, "Brutalize", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'diamond',color:'rgb(255,0,255)',","CC",0), //Brutalize (jumped upon by Samarog->Breakbar), Brutalize (Breakbar)
            new Mechanic(37892, "Soul Swarm", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Samarog, "symbol:'x-thin-open',color:'rgb(0,255,255)',","Wall",0), // Soul Swarm (stood in or beyond Spear Wall), Spear Wall
            new Mechanic(38231, "Impaling Stab", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'hourglass',color:'rgb(0,0,255)',","ShWv.Ctr",0), //Impaling Stab (hit by Spears causing Shockwave), Shockwave Center
            
            //  new Mechanic(37816, "Brutalize", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Samarog, "symbol:'star-square',color:'rgb(255,0,0)',", "CC Target", casted without dmg odd
            });
        }

        public override CombatReplayMap getCombatMap()
        {
            return new CombatReplayMap("https://i.imgur.com/o2DHN29.png",
                            Tuple.Create(1221, 1171),
                             Tuple.Create(-6526, 1218, -2423, 5146),
                            Tuple.Create(-27648, -9216, 27648, 12288),
                            Tuple.Create(11774, 4480, 14078, 5376));
        }

        public override List<PhaseData> getPhases(Boss boss, ParsedLog log, List<CastLog> cast_logs)
        {
            long start = 0;
            long end = 0;
            long fight_dur = log.getBossData().getAwareDuration();
            List<PhaseData> phases = getInitialPhase(log);
            // Determined check
            List<CombatItem> invulsSam = getFilteredList(log, 762, boss.getInstid());         
            for (int i = 0; i < invulsSam.Count; i++)
            {
                CombatItem c = invulsSam[i];
                if (c.isBuffremove() == ParseEnum.BuffRemove.None)
                {
                    end = c.getTime() - log.getBossData().getFirstAware();
                    phases.Add(new PhaseData(start, end));
                    if (i == invulsSam.Count - 1)
                    {
                        cast_logs.Add(new CastLog(end, -5, (int)(fight_dur - end), ParseEnum.Activation.None, (int)(fight_dur - end), ParseEnum.Activation.None));
                    }
                }
                else
                {
                    start = c.getTime() - log.getBossData().getFirstAware();
                    phases.Add(new PhaseData(end, start));
                    cast_logs.Add(new CastLog(end, -5, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None));
                }
            }
            if (fight_dur - start > 5000 && start >= phases.Last().getEnd())
            {
                phases.Add(new PhaseData(start, fight_dur));
            }
            string[] namesSam = new string[] { "Phase 1", "Split 1", "Phase 2", "Split 2", "Phase 3" };
            for (int i = 1; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                phase.setName(namesSam[i - 1]);
                if (i == 2 || i == 4)
                {
                    List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                       ParseEnum.ThrashIDS.Rigom,
                       ParseEnum.ThrashIDS.Guldhem
                    };
                    List<AgentItem> slaves = log.getAgentData().getNPCAgentList().Where(x => ids.Contains(ParseEnum.getThrashIDS(x.getID()))).ToList();
                    foreach (AgentItem a in slaves)
                    {
                        long agentStart = a.getFirstAware() - log.getBossData().getFirstAware();
                        long agentEnd = a.getLastAware() - log.getBossData().getFirstAware();
                        if (phase.inInterval(agentStart))
                        {
                            phase.addRedirection(a);
                        }
                    }
                    phase.overrideStart(log.getBossData().getFirstAware());
                }
            }
            return phases;
        }

        public override List<ParseEnum.ThrashIDS> getAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO: facing information (shock wave)
            List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                        ParseEnum.ThrashIDS.Rigom,
                        ParseEnum.ThrashIDS.Guldhem
                    };
            List<CombatItem> brutalize = log.getBoonData().Where(x => x.getSkillID() == 38226 && x.isBuffremove() != ParseEnum.BuffRemove.Manual).ToList();
            int brutStart = 0;
            int brutEnd = 0;
            foreach (CombatItem c in brutalize)
            {
                if (c.isBuffremove() == ParseEnum.BuffRemove.None)
                {
                    brutStart = (int)(c.getTime() - log.getBossData().getFirstAware());
                }
                else
                {
                    brutEnd = (int)(c.getTime() - log.getBossData().getFirstAware());
                    replay.addCircleActor(new CircleActor(true, 0, 120, new Tuple<int, int>(brutStart, brutEnd), "rgba(0, 180, 255, 0.3)"));
                }
            }
            return ids;
        }

        public override void getAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // big bomb
            List<CombatItem> bigbomb = log.getBoonData().Where(x => x.getSkillID() == 37966 && ((x.getDstInstid() == p.getInstid() && x.isBuffremove() == ParseEnum.BuffRemove.None))).ToList();
            foreach (CombatItem c in bigbomb)
            {
                int bigStart = (int)(c.getTime() - log.getBossData().getFirstAware());
                int bigEnd = bigStart + 6000;
                replay.addCircleActor(new CircleActor(true, 0, 300, new Tuple<int, int>(bigStart, bigEnd), "rgba(150, 80, 0, 0.2)"));
                replay.addCircleActor(new CircleActor(true, bigEnd, 300, new Tuple<int, int>(bigStart, bigEnd), "rgba(150, 80, 0, 0.2)"));
            }
            // small bomb
            List<CombatItem> smallbomb = log.getBoonData().Where(x => x.getSkillID() == 38247 && ((x.getDstInstid() == p.getInstid() && x.isBuffremove() == ParseEnum.BuffRemove.None))).ToList();
            foreach (CombatItem c in smallbomb)
            {
                int smallStart = (int)(c.getTime() - log.getBossData().getFirstAware());
                int smallEnd = smallStart + 6000;
                replay.addCircleActor(new CircleActor(true, 0, 80, new Tuple<int, int>(smallStart, smallEnd), "rgba(80, 150, 0, 0.3)"));
            }
            // fixated
            List<CombatItem> fixatedSam = getFilteredList(log, 37868, p.getInstid());
            int fixatedSamStart = 0;
            int fixatedSamEnd = 0;
            foreach (CombatItem c in fixatedSam)
            {
                if (c.isBuffremove() == ParseEnum.BuffRemove.None)
                {
                    fixatedSamStart = (int)(c.getTime() - log.getBossData().getFirstAware());
                }
                else
                {
                    fixatedSamEnd = (int)(c.getTime() - log.getBossData().getFirstAware());
                    replay.addCircleActor(new CircleActor(true, 0, 80, new Tuple<int, int>(fixatedSamStart, fixatedSamEnd), "rgba(255, 80, 255, 0.3)"));
                }
            }
        }

        public override int isCM(List<CombatItem> clist, int health)
        {
            return (health > 30e6) ? 1 : 0;
        }

        public override string getReplayIcon()
        {
            return "https://i.imgur.com/MPQhKfM.png";
        }
    }
}
