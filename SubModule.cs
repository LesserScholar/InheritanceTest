using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;


namespace InheritanceTest
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

        }

        static Dictionary<string, MobileParty> partyMarkers;

        public static void PartyMarker(Vec2 pos, string text)
        {
            if (partyMarkers == null)
            {
                partyMarkers = new Dictionary<string, MobileParty>();
            }

            if (partyMarkers.ContainsKey(text))
            {
                partyMarkers[text].Position2D = pos;
                return;
            }
            PartyTemplateObject partyTemplateObject = Campaign.Current.ObjectManager.GetObject<PartyTemplateObject>("main_hero_party_template");

            MobileParty party = CustomPartyComponent.CreateQuestParty(pos, 0, Settlement.GetFirst, new TaleWorlds.Localization.TextObject(text), Clan.PlayerClan, partyTemplateObject, Hero.MainHero);

            party.DisableAi();
            party.IgnoreForHours(10000);

            partyMarkers.Add(text, party);
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("clear_party_markers", "inheritance_test")]
        public static string ClearPartyMarkers(List<string> strings)
        {
            if (partyMarkers != null)
            {
                foreach (var p in partyMarkers)
                {
                    DestroyPartyAction.Apply(PartyBase.MainParty, p.Value);
                }
                partyMarkers.Clear();
            }
            return "";
        }

        public static bool MatchClanName(Clan c, List<string> strings)
        {
            string input = String.Join("", strings).ToLower();
            string clan_name = c.Name.ToString();
            return String.Join("", clan_name.Split(' ')).ToLower() == String.Join("", input.Split(' ')).ToLower();
        }

        public static IFaction GetFaction(string name)
        {
            Clan match = Clan.FindFirst((Clan c) =>
                name.Trim().ToLower() == c.Name.ToString().ToLower()
            );
            if (match != null) return (IFaction)match;
            Kingdom m = Kingdom.All.FirstOrDefault((Kingdom k) =>
                name.Trim().ToLower() == k.Name.ToString().ToLower()
            );
            return m;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("faction_center", "inheritance_test")]
        public static string FactionCenter(List<string> strings)
        {
            IFaction f = GetFaction(String.Join(" ", strings));
            if (f == null)
            {
                return "can't find faction with given name";
            }
            PartyMarker(f.FactionMidPoint, f.Name.ToString());
            return f.FactionMidPoint.ToString();
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("faction_initial_position", "inheritance_test")]
        public static string ClanInitialPosition(List<string> strings)
        {
            IFaction f = GetFaction(String.Join(" ", strings));
            if (f == null)
            {
                return "can't find faction with given name";
            }

            else
            {
                PartyMarker(f.InitialPosition, f.Name.ToString());
                return f.InitialPosition.ToString();
            }
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("line_between_centers", "inheritance_test")]
        public static string LineBetweenCenters(List<string> strings)
        {
            string[] array = CampaignCheats.ConcatenateString(strings).Split(new char[]
            {
                '|'
            });
            if (array.Length != 2)
            {
                return "Format is: [faction] | [faction]";
            }

            IFaction first = GetFaction(array[0]);
            IFaction second = GetFaction(array[1]);

            Vec3 v1 = first.FactionMidPoint.ToVec3(), v2 = second.FactionMidPoint.ToVec3();
            Campaign.Current.MapSceneWrapper.GetHeightAtPoint(first.FactionMidPoint, ref v1.z);
            Campaign.Current.MapSceneWrapper.GetHeightAtPoint(second.FactionMidPoint, ref v2.z);

            MBDebug.RenderDebugLine(v1, v2 - v1, 0xFFFF0000, false, 20f);
            return $"Drew line between {v1.ToString()} and {v2.ToString()}";
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("every_clan_initial_positions", "inheritance_test")]
        public static string EveryClanInitialPositions(List<string> strings)
        {
            foreach (Clan c in Clan.All.Where((Clan c) => !c.IsMinorFaction && !c.IsBanditFaction && !c.Lords.IsEmpty<Hero>()))
            {
                PartyMarker(c.InitialPosition, c.Name.ToString());
            }
            return "done";
        }

        

        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_clan", "inheritance_test")]
        public static string DestroyClan(List<string> strings)
        {
            if (strings.Count < 1)
            {
                return "Format: clan";
            }

            Clan match = Clan.FindFirst((Clan c) =>
            {
                return MatchClanName(c, strings);
            });

            if (match == null)
            {
                return "didn't find clan";
            }
            else
            {
                TaleWorlds.CampaignSystem.Actions.DestroyClanAction.Apply(match);
                return "clan destroyed";
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("imprison_clan", "inheritance_test")]
        public static string ImprisonClan(List<string> strings)
        {
            if (strings.Count < 1)
            {
                return "Format: clan";
            }

            Clan match = Clan.FindFirst((Clan c) =>
            {
                return MatchClanName(c, strings);
            });

            if (match == null)
            {
                return "didn't find clan";
            }


            foreach (Hero h in match.Heroes)
            {
                if (h.IsAlive)
                {
                    TakePrisonerAction.Apply(PartyBase.MainParty, h);
                }
            }
            return "imprisoned";
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_kingdom", "inheritance_test")]
        public static string DestroyKingdom(List<string> strings)
        {
            string kingdom_name = String.Join(" ", strings);
            Kingdom k = CampaignCheats.GetKingdom(kingdom_name);

            if (k == null)
            {
                return "couldn't find kingdom";
            }
            else
            {
                TaleWorlds.CampaignSystem.Actions.DestroyKingdomAction.Apply(k);
                return "kingdom destroyed";
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_non_leading_clans", "inheritance_test")]
        public static string DestroyNonLeadingClans(List<string> strings)
        {
            string kingdom_name = String.Join(" ", strings);
            Kingdom k = CampaignCheats.GetKingdom(kingdom_name);

            if (k == null)
            {
                return "couldn't find kingdom";
            }
            else
            {
                List<Clan> clans = Clan.All.Where((Clan c) => c.Kingdom == k && c.Leader != k.Leader).ToList();
                foreach (Clan c in clans)
                {
                    TaleWorlds.CampaignSystem.Actions.DestroyClanAction.Apply(c);
                }

                return "clans destroyed";
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("give_settlement_to_clan", "inheritance_test")]
        public static string GiveSettlementToClan(List<string> strings)
        {
            if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckParameters(strings, 1))
            {
                return "Format is \"campaign.give_settlement_to_kingdom [SettlementName] | [ClanName]";
            }
            string[] array = CampaignCheats.ConcatenateString(strings).Split(new char[]
            {
                '|'
            });
            if (array.Length != 2)
            {
                return "Format is \"campaign.give_settlement_to_kingdom [SettlementName] | [KingdomName]";
            }
            Settlement settlement = CampaignCheats.GetSettlement(array[0].Trim());
            if (settlement == null)
            {
                return "Given settlement name could not be found.";
            }
            if (settlement.IsVillage)
            {
                settlement = settlement.Village.Bound;
            }
            Clan match = Clan.FindFirst((Clan c) =>
            {
                return MatchClanName(c, new List<string> { array[1].Trim() });
            });

            if (match == null)
            {
                return "Given clan could not be found.";
            }

            ChangeOwnerOfSettlementAction.ApplyByDefault(match.Leader, settlement);
            return settlement.Name + string.Format(" has been given to {0}.", match.Leader.Name);
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("succession_clans", "inheritance_test")]
        public static String SuccessionClans(List<string> strings)
        {
            Clan oldClan = Clan.FindFirst((Clan c) =>
            {
                return MatchClanName(c, strings);
            });

            if (oldClan == null)
            {
                string kingdom_name = String.Join(" ", strings);
                Kingdom k = CampaignCheats.GetKingdom(kingdom_name);
                if (k == null)
                {
                    return "couldn't find clan";
                }
                else
                {
                    oldClan = k.Leader.Clan;
                }
            }

            float num = float.MaxValue;
            IEnumerable<Clan> all = Clan.All;

            List<(float, Clan)> clans = new List<(float, Clan)>();

            Func<Clan, bool> func = (delegate (Clan t)
            {
                if (t != oldClan && !t.IsEliminated && !t.IsMinorFaction && !t.Lords.IsEmpty<Hero>())
                {
                    return Enumerable.Any<Hero>(t.Lords, (Hero k) => !k.IsChild);
                }
                return false;
            });

            foreach (Clan clan2 in Enumerable.Where<Clan>(all, func))
            {
                float lengthSquared = (clan2.FactionMidPoint - oldClan.FactionMidPoint).LengthSquared;
                clans.Add((lengthSquared, clan2));
            }
            clans = clans.OrderBy(((float, Clan) t) => t.Item1).ToList();

            PartyMarker(oldClan.FactionMidPoint, "Center point");

            List<(int, string)> order_strings = new List<(int, string)> { (0, "1st:"), (1, "2nd:"), (2, "3rd:"), (3, "4th:"), (4, "5th: ") };

            foreach (var t in order_strings)
            {
                if (clans.Count > t.Item1)
                {
                    Clan c = clans[t.Item1].Item2;
                    if (c.Kingdom == null)
                    {
                        PartyMarker(c.FactionMidPoint, $"{t.Item2} {c.Name.ToString()}, d:{clans[t.Item1].Item1}");
                    }
                    else
                    {
                        PartyMarker(c.FactionMidPoint, $"{t.Item2} {c.Name.ToString()}, {c.Kingdom.Name.ToString()}, d:{TaleWorlds.Library.MathF.Sqrt(clans[t.Item1].Item1)}");
                    }
                }
            }

            return "done";
        }
    }

    class AdditionalTests {
        [CommandLineFunctionality.CommandLineArgumentFunction("draw_entity_at_map_pos", "scholar_test")]
        public static string DrawEntityAtMapPos(List<string> strings)
        {
            if (strings.Count < 2)
            {
                return "Format: x y [entity]";
            }
            float x, y;
            float.TryParse(strings[0], out x);
            float.TryParse(strings[1], out y);

            string entity;
            if (strings.Count < 3)
            {
                entity = "quest_icon_2";
            }
            else
            {
                entity = strings[2];
            }
            Campaign.Current.MapSceneWrapper.AddNewEntityToMapScene(entity, new Vec2(x, y));

            return "";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("debug_sphere", "scholar_test")]
        public static string DebugSphere(List<string> strings)
        {
            if (strings.Count < 2)
            {
                return "Format: x y [time]";
            }
            float x, y, z = 0;
            float.TryParse(strings[0], out x);
            float.TryParse(strings[1], out y);

            Campaign.Current.MapSceneWrapper.GetHeightAtPoint(new Vec2(x, y), ref z);

            float time = 30f;
            if (strings.Count > 2)
            {
                float.TryParse(strings[2], out time);
            }

            MBDebug.RenderDebugSphere(new Vec3(x, y, z), 3f, 0xFFFFFFFF, false, time);

            return "";
        }

        public static void DrawDebugMarker(Vec2 pos, string text)
        {

            Campaign.Current.MapSceneWrapper.AddNewEntityToMapScene("quest_icon_2", pos);

            float z = 0;
            Campaign.Current.MapSceneWrapper.GetHeightAtPoint(pos, ref z);
            MBDebug.RenderDebugText3D(new Vec3(pos.x, pos.y, z), text, 0xFFFFFF00, 20, 0, 10f);
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("to_aserai", "culture_change")]
        public static string CultureToAserai(List<string> strings)
        {
            Hero.MainHero.Culture = Campaign.Current.ObjectManager.GetObject<CultureObject>("aserai");
            return "done";
        }
        [CommandLineFunctionality.CommandLineArgumentFunction("to_battania", "culture_change")]
        public static string CultureToBattania(List<string> strings)
        {
            Hero.MainHero.Culture = Campaign.Current.ObjectManager.GetObject<CultureObject>("battania");
            return "done";
        }

    }
}