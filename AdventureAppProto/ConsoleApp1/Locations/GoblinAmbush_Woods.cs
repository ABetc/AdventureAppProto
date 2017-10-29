using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_Woods : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> Woods_Options = new Dictionary<int, string>();
        private Dictionary<int, int> Woods_Results = new Dictionary<int, int>();

        private enum Woods_Enum
        {
            Method_SearchWoods,
            Method_CallOut,
            Method_Move,
            Method_FollowPath,
            Method_SearchGoblins,

            GoTo_GoblinAmbush_PathRoad,
            GoTo_GoblinAmbush_PathCave,
            GoTo_GoblinAmbush_RoadWest,
            GoTo_GoblinAmbush_RoadEast
        };

        public GoblinAmbush_Woods(double id)
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
                    Methods.Typewriter("The woods grow dim as a thick, leafy canopy blocks out the sun.");
                }
                // if returning to location
                else
                {
                    Methods.Typewriter("The shadowy din of the forest canopy looms overhead.");
                }
            }

            GoblinAmbush.Skill_Stealth = 2;     // temp

            // check if stealth is low enough to trigger a goblin fight
            if (GoblinAmbush.Skill_Stealth.Equals(0))
            {
                GoblinAmbush.Skill_Stealth = Methods.RollStat(Player.DEX, "Stealth");
            }

            if (GoblinAmbush.Skill_Stealth < 10 && !GoblinAmbush.Note_FoughtGoblins)
            {
                Methods.Typewriter("A group of goblins suddenly jump from the underbrush and attack!");

                GoblinFight();
            }

            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // automatic survival check for details in the woods
                GoblinAmbush.Skill_Survival = Methods.RollStat(Player.WIS);

                if (GoblinAmbush.Skill_Survival >= 15 && !GoblinAmbush.Note_FoughtGoblins || GoblinAmbush.Note_FoundGoblins && !GoblinAmbush.Note_FoughtGoblins)
                {
                    // finding goblins and path for the first time
                    if (!GoblinAmbush.Note_FoundGoblins)
                    {
                        Methods.Typewriter("There is a subtle path leading through the woods towards the road. Following it slightly " +
                            "reveals a group of five goblins huddled amoungst the underbrush absently watching the road.");
                        GoblinAmbush.Note_FoundPath = true;
                        GoblinAmbush.Note_FoundGoblins = true;
                    }
                    // reminder of having already found goblins
                    else
                    {
                        Methods.Typewriter("In the background, the goblins still bicker quitely amoungst themselves.");
                    }
                }
                else if (GoblinAmbush.Skill_Survival >= 10 || GoblinAmbush.Note_FoundPath)
                {
                    // finding path for the first time
                    if (!GoblinAmbush.Note_FoundPath)
                    {
                        Methods.Typewriter("There is a subtle path leading through the woods towards the road.");
                        GoblinAmbush.Note_FoundPath = true;
                    }
                    // reminder of having already found path
                    else
                    {
                        Methods.Typewriter("The small forest path is still noticeable through the underbrush.");
                    }
                }
                // if nothing has been found
                else
                {
                    Methods.Typewriter("The woods seem quiet and still.");
                }

                // check if goblins were fought at this location
                if (GoblinAmbush.Note_WoodsFight)
                {
                    Methods.Typewriter("The bodies of the goblins lie amoungst the underbrush.");
                }
            }
        }

        public override int LocationOptions()
        {
            if (GoblinAmbush.Note_FoughtGoblins)
            {
                Woods_Options[1] = "Search the area";
                Woods_Options[2] = "Call out in a loud voice";
                Woods_Options[3] = "Move through the woods";

                Woods_Results[1] = (int)Woods_Enum.Method_SearchWoods;
                Woods_Results[2] = (int)Woods_Enum.Method_CallOut;
                Woods_Results[3] = (int)Woods_Enum.Method_Move;


                if (GoblinAmbush.Note_FoundPath)
                {
                    Woods_Options[2] = "Follow the path";
                    Woods_Results[2] = (int)Woods_Enum.Method_FollowPath;
                }

                if (GoblinAmbush.Note_WoodsFight)
                {
                    Woods_Options[1] = "Search the goblins for any items of interest";

                    Woods_Results[1] = (int)Woods_Enum.Method_SearchGoblins;
                }
            }
            else if (GoblinAmbush.Note_FoundGoblins)
            {
                Woods_Options[1] = "Sneak towards the goblins";
                Woods_Options[2] = "Follow the path into the woods";
                Woods_Options[3] = "Ignore the goblins and the path";

                Woods_Results[1] = (int)Woods_Enum.GoTo_GoblinAmbush_PathRoad;
                Woods_Results[2] = (int)Woods_Enum.GoTo_GoblinAmbush_PathCave;
                Woods_Results[3] = (int)Woods_Enum.Method_Move;
            }
            else if (GoblinAmbush.Note_FoundPath)
            {
                Woods_Options[1] = "Search the area";
                Woods_Options[2] = "Follow the path";
                Woods_Options[3] = "Ignore the path and move on";

                Woods_Results[1] = (int)Woods_Enum.Method_SearchWoods;
                Woods_Results[2] = (int)Woods_Enum.Method_FollowPath;
                Woods_Results[3] = (int)Woods_Enum.Method_Move;
            }
            else
            {
                Woods_Options[1] = "Search the area";
                Woods_Options[2] = "Call out a loud message";
                Woods_Options[3] = "Continue on through the woods";

                Woods_Results[1] = (int)Woods_Enum.Method_SearchWoods;
                Woods_Results[2] = (int)Woods_Enum.Method_CallOut;
                Woods_Results[3] = (int)Woods_Enum.Method_Move;
            }

            Methods.PrintOptions(Woods_Options);
            return Woods_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = Woods_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)Woods_Enum.Method_SearchWoods:
                    SearchWoods();
                    break;

                case (int)Woods_Enum.Method_CallOut:
                    CallOut();
                    break;

                case (int)Woods_Enum.Method_Move:
                    Move();
                    break;

                case (int)Woods_Enum.Method_FollowPath:
                    FollowPath();
                    break;

                case (int)Woods_Enum.Method_SearchGoblins:
                    SearchGoblins();
                    break;

                case (int)Woods_Enum.GoTo_GoblinAmbush_PathRoad:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_PathRoad_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)Woods_Enum.GoTo_GoblinAmbush_PathCave:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_PathCave_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void SearchWoods()
        {
            GoblinAmbush.Skill_Investigation = Methods.RollStat(Player.INT, "Investigation");

            if (GoblinAmbush.Note_FoundPath)
            {
                if (GoblinAmbush.Skill_Investigation >= 10 && !GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("At first the woods seem devoid of any activity, but then in a pause " +
                        "between the slight fluttering of leaves comes the sounds of a short scuffle. " +
                        "Towards the road along the woodland path, a few more muttered grunts and quiet " +
                        "chattering reveals five unsuspecting goblins.");
                    GoblinAmbush.Note_FoundGoblins = true;
                }
                else if (GoblinAmbush.Skill_Investigation >= 10 && GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("Too small for a regualar human, but too large for most forest " +
                        "animals, the path looks to be a hidden goblin game trail. Small cuts along " +
                        "some nearby trees seem to be from sharp weapons. A discarded remnant " +
                        "of worked leather completes the set of clues.");
                }
                else
                {
                    Methods.Typewriter("Nothing points to what uses the small path.");
                }
            }
            else
            {
                if (GoblinAmbush.Skill_Investigation >= 15 && !GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("There is a subtle path leading through the woods towards the road. Following it slightly " +
                            "reveals a group of five goblins huddled amoungst the underbrush absently watching the road.");
                    GoblinAmbush.Note_FoundGoblins = true;
                    GoblinAmbush.Note_FoundPath = true;
                }
                else if (GoblinAmbush.Skill_Investigation >= 10)
                {
                    Methods.Typewriter("There is a subtle woodland path leading from the woods to the road.");
                    GoblinAmbush.Note_FoundGoblins = true;
                }
                else
                {
                    Methods.Typewriter("A stillness hangs over the woods, nothing seems to stirr.");
                }
            }
        }

        private void CallOut()
        {
            Dictionary<int, string> _choices = new Dictionary<int, string>();
            Dictionary<int, string> _results = new Dictionary<int, string>();

            _choices[1] = "You mean no harm";
            _choices[2] = "Shout a challenge";
            _choices[3] = "(opt for silence)";

            _results[1] = "\"I'll not harm a soul, stand ye and be seen!\"";
            _results[2] = "\"Any who's of a mind to cross blades, here I be!\"";
            _results[3] = "\"...\"";

            Methods.PrintOptions(_choices);
            int _playerChoice = Methods.GetPlayerChoice(_choices.Count);
            Methods.Typewriter(_results[_playerChoice], "cyan");

            if (!GoblinAmbush.Note_FoughtGoblins)
            {
                GoblinFight();
            }
            else
            {
                Methods.Typewriter("The sound carries into the trees, but nothing seems to respond.");
            }
        }

        private void Move()
        {
            Dictionary<int, string> _choices = new Dictionary<int, string>();
            Dictionary<int, int> _results = new Dictionary<int, int>();

            _choices[1] = "Continue through the woods";
            _choices[2] = "Double back to the road";

            _results[1] = (int)Woods_Enum.GoTo_GoblinAmbush_RoadEast;
            _results[2] = (int)Woods_Enum.GoTo_GoblinAmbush_RoadWest;

            Methods.PrintOptions(_choices);
            int _playerChoice = Methods.GetPlayerChoice(_choices.Count);
            int _enumNumber = _results[_playerChoice];

            switch (_enumNumber)
            {
                case (int)Woods_Enum.GoTo_GoblinAmbush_RoadEast:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadEast_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)Woods_Enum.GoTo_GoblinAmbush_RoadWest:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadWest_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void FollowPath()
        {
            Dictionary<int, string> _choices = new Dictionary<int, string>();
            Dictionary<int, int> _results = new Dictionary<int, int>();

            _choices[1] = "Follow the path into the woods";
            _choices[2] = "Follow the path towards the road";

            _results[1] = (int)Woods_Enum.GoTo_GoblinAmbush_PathCave;
            _results[2] = (int)Woods_Enum.GoTo_GoblinAmbush_PathRoad;

            Methods.PrintOptions(_choices);
            int _playerChoice = Methods.GetPlayerChoice(_choices.Count);
            int _enumNumber = _results[_playerChoice];

            switch (_enumNumber)
            {
                case (int)Woods_Enum.GoTo_GoblinAmbush_PathCave:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_PathCave_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)Woods_Enum.GoTo_GoblinAmbush_PathRoad:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_PathRoad_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void GoblinFight()
        {
            Methods.Typewriter("(goblin fight)");

            GoblinAmbush.Note_FoughtGoblins = true;
            GoblinAmbush.Note_WoodsFight = true;
        }

        private void SearchGoblins()
        {
            Methods.Typewriter("(search goblins)");
            Methods.Enter();
        }
    }
}