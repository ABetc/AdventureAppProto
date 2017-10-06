using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_PathCave : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> PathCave_Options = new Dictionary<int, string>();
        private Dictionary<int, int> PathCave_Results = new Dictionary<int, int>();

        enum PathCave_Enum
        {
            Method_SpringTrap,

            GoTo_GoblinCave_CaveEntrance,
            GoTo_GoblinAmbush_Woods
        }

        public GoblinAmbush_PathCave(double id)
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
                    Methods.Typewriter("As the path threads through the forest, the underbrush grows thicker.");
                }
                else
                {
                    Methods.Typewriter("The small path winds naturally around the forest trees and thick underbrush.");
                }

                // if snare trap was triggered
                if (GoblinAmbush.Note_SnareSprung)
                {
                    Methods.Typewriter("The sprung snare trap hangs limply from a tree to the side of the path.");
                }
                else
                {
                    SnareTrap();
                }
            }
        }

        public override int LocationOptions()
        {
            PathCave_Options[1] = "Continue deeper into the woods";
            PathCave_Options[2] = "Turn back towards the forest road";

            PathCave_Results[1] = (int)PathCave_Enum.GoTo_GoblinCave_CaveEntrance;
            PathCave_Results[2] = (int)PathCave_Enum.GoTo_GoblinAmbush_Woods;

            if (Player.PreviousLocation.LocationID.Equals(World.GoblinCave_CaveEntrance_ID))
            {
                PathCave_Options[1] = "Continue along the forest path";
                PathCave_Options[2] = "Turn back towards the cave entrance";

                PathCave_Results[1] = (int)PathCave_Enum.GoTo_GoblinAmbush_Woods;
                PathCave_Results[2] = (int)PathCave_Enum.GoTo_GoblinCave_CaveEntrance;
            }

            Methods.PrintOptions(PathCave_Options);
            return PathCave_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = PathCave_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)PathCave_Enum.GoTo_GoblinCave_CaveEntrance:
                    Player.CurrentLocation = World.FindLocation(World.GoblinCave_CaveEntrance_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)PathCave_Enum.GoTo_GoblinAmbush_Woods:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_Woods_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void SnareTrap()
        {
            GoblinAmbush.Skill_Survival = Methods.RollStat(Player.WIS);

            if (GoblinAmbush.Note_FoundSnare)
            {
                if (GoblinAmbush.Skill_Survival >= 5)
                {
                    Methods.Typewriter("The goblin snare lies partially concealed across the path.");

                    Dictionary<int, string> _choices = new Dictionary<int, string>();
                    Dictionary<int, int> _results = new Dictionary<int, int>();

                    _choices[1] = "Try to spring the trap";
                    _choices[2] = "Leave the trap alone";

                    _results[1] = (int)PathCave_Enum.Method_SpringTrap;
                    _results[2] = 1;

                    Methods.PrintOptions(_choices);
                    int _playerChoice = Methods.GetPlayerChoice(_choices.Count);
                    int _enumNumber = _results[_playerChoice];

                    switch (_enumNumber)
                    {
                        case (int)PathCave_Enum.Method_SpringTrap:
                            SpringTrap();
                            break;

                        case 1:
                            break;
                    }
                }
                else
                {
                    Methods.Typewriter(string.Format("A fraction too late, {0} recalls the snare trap as the tripcord breaks around " +
                        "{1} feet!", Player.Name, Player.Possessive));
                    GoblinAmbush.Note_SnareSprung = true;
                    CaughtInTrap();
                }
            }
            else
            {
                if (GoblinAmbush.Skill_Survival >= 12)
                {
                    Methods.Typewriter("While carefully treading the path, a curious line on the ground caches the eye. Partially " +
                        "concealed with branches and leaves is a loaded snare trap.");
                    GoblinAmbush.Note_FoundSnare = true;

                    Dictionary<int, string> _choices = new Dictionary<int, string>();
                    Dictionary<int, int> _results = new Dictionary<int, int>();

                    _choices[1] = "Try to spring the trap";
                    _choices[2] = "Leave the trap alone";

                    _results[1] = (int)PathCave_Enum.Method_SpringTrap;
                    _results[2] = 1;

                    Methods.PrintOptions(_choices);
                    int _playerChoice = Methods.GetPlayerChoice(_choices.Count);
                    int _enumNumber = _results[_playerChoice];

                    switch (_enumNumber)
                    {
                        case (int)PathCave_Enum.Method_SpringTrap:
                            SpringTrap();
                            break;

                        case 1:
                            break;
                    }
                }
                else
                {
                    Methods.Typewriter(string.Format("With a *snap* a tripcord breaks around {0}'s feet!", Player.Name));
                    GoblinAmbush.Note_SnareSprung = true;
                    CaughtInTrap();
                }
            }
        }

        private void SpringTrap()
        {
            GoblinAmbush.Skill_SlightOfHand = Methods.RollStat(Player.DEX, "Slight of Hand");

            if (GoblinAmbush.Skill_SlightOfHand >= 5)
            {
                Methods.Typewriter(string.Format("Gingerly, {0} springs the trap and the snare launches harmlessly into the air in " +
                    "a burst of leaves and twigs.", Player.Name));
                GoblinAmbush.Note_SnareSprung = true;
            }
            else
            {
                Methods.Typewriter(string.Format("Gingerly, {0} springs the trap and the snare launches into the air, but, " +
                    "as it whips around, its tail end snaps across {0}'s arm painfully.", Player.Name));

                Methods.Typewriter("\"Yarr! Blast you fiendish cord you!\"", "cyan");

                int _damage = Methods.RollD(4);
                Methods.Print("Damage", _damage);
                Player.Damage += _damage;
                Player.CheckHealth();
            }
        }

        private void CaughtInTrap()
        {
            int _DexSave = Methods.RollStat(Player.DEX, "Dexterity Save");

            if (_DexSave > 10)
            {
                Methods.Typewriter(string.Format("{0} springs out of the way as the snare launches into the air in a burst of leaves " +
                    "and twigs.", Player.Name));
                return;
            }
            else
            {
                Methods.Typewriter(string.Format("{0} springs to the side but the trap tightends around his legs too quickly and " +
                    "{1} is hoisted upside-down into the air!", Player.Name, Player.Pronoun));
            }

            Dictionary<int, string> _choices = new Dictionary<int, string>();
            Dictionary<int, int> _results = new Dictionary<int, int>();

            _choices[1] = "Slice at the rope";
            _choices[2] = "Reach up and undo the knot";

            Methods.PrintOptions(_choices);
            int _PlayerChoice = Methods.GetPlayerChoice(_choices.Count);

            switch (_PlayerChoice)
            {
                case 1:
                    SliceRope();
                    break;

                case 2:
                    UntieKnot();
                    break;
            }
        }

        private void SliceRope()
        {
            Methods.Typewriter(string.Format("With a muttered curse and minor awkwardness, {0} reaches for a blade on {1} " +
                "belt, slices at the rope, and tumbles heavily to the forest ground with a crumpled thud.", Player.Name, Player.Possessive));

            Methods.Typewriter("\"Ouff! Blisterin' blue barnicles, me back...\"");

            int _damage = Methods.RollD(4);
            Methods.Print("Damage", _damage);
            Player.Damage += _damage;
            Player.CheckHealth();
        }

        private void UntieKnot()
        {
            int _StrSave = Methods.RollStat(Player.STR, "Strength Check");

            if (_StrSave > 10)
            {
                Methods.Typewriter(string.Format("With a muttered curse and a grunt, {0} reaches up to snatch at the rope " +
                    "and undoes the knot without too much difficulty. Keeping a firm grasp on the rope, {1} swings " +
                    "right-side up and lands back onto the forest floor.", Player.Name, Player.Pronoun));
            }
            else
            {
                Methods.Typewriter(string.Format("With a muttered curse and a few labourous grunts, {0} reaches up to " +
                    "claw at the knotted rope. The snare finally comes apart and sends {1} flailing back down to " +
                    "the forest ground with a heavy thud", Player.Name, Player.Pronoun));

                Methods.Typewriter("\"Ouff! Blisterin' blue barnicles, me back...\"");

                int _damage = Methods.RollD(4);
                Methods.Print("Damage", _damage);
                Player.Damage += _damage;
                Player.CheckHealth();
            }
        }
    }
}