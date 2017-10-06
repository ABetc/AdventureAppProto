using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_RoadDitch : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> RoadDitch_Options = new Dictionary<int, string>();
        private Dictionary<int, int> RoadDitch_Results = new Dictionary<int, int>();

        enum RoadDitch_Enum
        {
            Method_GoblinFight_Camp,

            GoTo_GoblinAmbush_DownedHorses,
            GoTo_GoblinAmbush_RoadEast,
            GoTo_GoblinAmbush_RoadWest
        }

        public GoblinAmbush_RoadDitch(double id)
        {
            LocationID = id;
            LocationInventory = new List<GameItems.Item>();
        }

        public override void OpeningText()
        {
            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // if never visited location
                if (LocationVisitCount.Equals(0))
                {
                    Methods.Typewriter("A low ditch full of overgrown shrubs at the edge of the road provides decent cover " +
                        "from possible wandering eyes from the forest or from the road up ahead.");
                }
                // if returning to location
                else
                {
                    Methods.Typewriter("The low ditch still has some flattened areas from the previous stealth appraoch");
                }
            }

            // check if stealth is low enough to trigger a goblin fight
            GoblinAmbush.Skill_Stealth = Methods.RollStat(Player.DEX, "Stealth");

            if (!GoblinAmbush.Note_FoughtGoblins)
            {
                // if found the goblins, lower the difficulty of the stealth check
                if (GoblinAmbush.Note_FoundGoblins && GoblinAmbush.Skill_Stealth < 5)
                {
                    Methods.Typewriter(string.Format("While creeping along through the underbrush and trying to keep an eye on the " +
                        "spot between the trees where the goblins may be spying, an upturned root escapes {0}'s notice. " +
                        "With an accidental startled cry, {1} catches {2} boot and tumbles forward onto hands and knees, " +
                        "bushes crackling and gear making just enough of a rattle that the sounds of the goblin's cackles " +
                        "can be heard as they jump out of hiding to inspect the noise.", Player.Name, Player.Pronoun, Player.Possessive));
                    Methods.Typewriter("\"AH! Human, ahaha!\"", "red");
                    Methods.Typewriter(string.Format("The first goblin shrieks as it spies {0}. Behind it the four others leap " +
                        "to attack!", Player.Name));

                    GoblinFight_Road();
                    return;
                }
                else if (GoblinAmbush.Skill_Stealth < 10)
                {
                    Methods.Typewriter(string.Format("Creeping along at the bottom of the ditch is tedious and difficult to do while staying " +
                        "quiet and visibly hidden. A loose bit of moss suddenly slips out from underneath {0}'s hand just as {1} " +
                        "was leaning too far forward. With an audible tumble, cackles from the edge of the woods show the fall " +
                        "did not go unnoticed. From out of some hidden spot in the trees leap five beady eyed goblins.", Player.Name,
                        Player.Possessive));
                    Methods.Typewriter("\"AH! Human, ahaha!\"", "red");
                    Methods.Typewriter(string.Format("The first goblin shrieks as it spies {0}. Behind it the four others leap " +
                        "to attack!", Player.Name));

                    GoblinFight_Road();
                    return;
                }
            }
            else
            {
                Methods.Typewriter(string.Format("The view of the road ahead disappears as the shurbbery closes in on both sides of the " +
                        "ditch. It seems painfully slow, but soon {0} makes the crawl further up the road without seeming to attract " +
                        "any attention", Player.Name));
            }

            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // if not discovered the downed horses
                if (Quests.GoblinAmbush_FoundHorses)
                {
                    Methods.Typewriter("Through a gap in the bushes, the dark shapes on the road are more recognizable as a pair of dead " +
                        "horses. Many crude arrow shafts stick out of their sides and the remanis of baggage are littered across the road.");
                }
            }
        }

        public override int LocationOptions()
        {
            RoadDitch_Options[1] = "Leave the ditch to examine the horses";
            RoadDitch_Options[2] = "Continue stealthily past the horses";
            RoadDitch_Options[3] = "Double back to the earlier area of road";

            RoadDitch_Results[1] = (int)RoadDitch_Enum.GoTo_GoblinAmbush_DownedHorses;
            RoadDitch_Results[2] = (int)RoadDitch_Enum.GoTo_GoblinAmbush_RoadEast;
            RoadDitch_Results[3] = (int)RoadDitch_Enum.GoTo_GoblinAmbush_RoadWest;

            if (GoblinAmbush.Note_FoundGoblins && !GoblinAmbush.Note_FoughtGoblins)
            {
                RoadDitch_Options[1] = "Attack the goblins by surprise";

                RoadDitch_Results[1] = (int)RoadDitch_Enum.Method_GoblinFight_Camp;
            }

            Methods.PrintOptions(RoadDitch_Options);
            return RoadDitch_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = RoadDitch_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)RoadDitch_Enum.Method_GoblinFight_Camp:
                    GoblinFight_Camp();
                    break;

                case (int)RoadDitch_Enum.GoTo_GoblinAmbush_DownedHorses:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_DownedHorses_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)RoadDitch_Enum.GoTo_GoblinAmbush_RoadEast:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadEast_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)RoadDitch_Enum.GoTo_GoblinAmbush_RoadWest:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadWest_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void GoblinFight_Road()
        {
            Methods.Typewriter("(goblin fight)");
            GoblinAmbush.Note_FoughtGoblins = true;
            GoblinAmbush.Note_RoadFight = true;
            Methods.Enter();

            // raise travel counts since the location is changing abruptly
            Player.TravelCount++;
            Player.CurrentLocation.LocationVisitCount++;
            Player.CurrentLocation.LastVisitedLocation = Player.TravelCount;
            Player.PreviousLocation = Player.CurrentLocation;

            // since this location change occurs during the opening text, display the new location's opening text
            Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_DownedHorses_ID);
            Player.CurrentLocation.OpeningText();
        }

        private void GoblinFight_Camp()
        {
            Methods.Typewriter("(goblin fight)");
            GoblinAmbush.Note_FoughtGoblins = true;
            GoblinAmbush.Note_CampFight = true;
            Methods.Enter();

            Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_PathRoad_ID);
        }
    }
}