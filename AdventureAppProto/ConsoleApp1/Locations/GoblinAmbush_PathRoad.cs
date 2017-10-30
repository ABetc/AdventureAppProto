using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_PathRoad : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> RoadPath_Options = new Dictionary<int, string>();
        private Dictionary<int, int> RoadPath_Results = new Dictionary<int, int>();

        enum RoadPath_Enum
        {
            Method_GoblinFight,
            Method_GoblinChat,
            Method_SearchCamp,

            GoTo_GoblinAmbush_Woods,
            GoTo_GoblinAmbush_DownedHorses
        }

        public GoblinAmbush_PathRoad(double id)
        {
            LocationID = id;
            LocationInventory = new List<GameItems.Item>();

            LocationInventory.Add(new GameItems.Trinket(
                GameItems.Trinket_GoblinDice.Name,
                GameItems.Trinket_GoblinDice.Description,
                GameItems.Trinket_GoblinDice.Value));
        }

        public override void OpeningText()
        {
            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // if never visited location and not found goblins
                if (LocationVisitCount.Equals(0) && !GoblinAmbush.Note_FoundGoblins && !GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("Following the path towards the road reveals five unsuspecting goblins! " +
                        "The creatures seem completely unaware of being watched. Periodically one or another " +
                        "goblin glances towards the road from their hidden spot in the trees but most of the " +
                        "group seem to be focused on other tasks. Bits of food scraps and small items litter " +
                        "the makeshift campsite.");
                    GoblinAmbush.Note_FoundGoblins = true;
                }
                // if never visited and fought the goblins
                else if (LocationVisitCount.Equals(0) && GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("The trees between the path and the road are large and the shrubbery forms " +
                        "a solid wall from which to stay hidden behind while watching the road. A small space has " +
                        "been cleared of twigs and brush. Various food scraps and meager goblin belongings lie " +
                        "scattered about what looks like a makeshift camp.");
                }
                // if never visited but found the goblins and not fought the goblins
                else if (LocationVisitCount.Equals(0) && GoblinAmbush.Note_FoundGoblins && !GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("The unsuspecting goblins continue to chatter aimlessly amoungst themselves. " +
                        "Around the goblins is a small space cleared of twigs and debris where the creatures have " +
                        "an easy view of the road from between the trees. Bits of food scraps, cooking items, " +
                        "and a few megear belongings are scattered around.");
                }
                // if returning to location but not fought the goblins
                else if (!LocationVisitCount.Equals(0) && !GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter("The unsuspecting goblins continue to chatter aimlessly amoungst themselves.");
                }
                // if returning to location and fought the goblins
                else if (!LocationVisitCount.Equals(0) && GoblinAmbush.Note_FoughtGoblins)
                {
                    if (GoblinAmbush.Note_CampFight)
                    {
                        Methods.Typewriter("The bodies of the goblins lie amoungst the remains of the camp.");
                    }
                    else
                    {
                        Methods.Typewriter("The bits of food scraps and small items are all that is left of the goblin camp.");
                    }
                }
            }
        }

        public override int LocationOptions()
        {
            RoadPath_Options[1] = "Attack the goblins by surprise";
            RoadPath_Options[2] = "Initiate conversation";
            RoadPath_Options[3] = "Return to the woods";

            RoadPath_Results[1] = (int)RoadPath_Enum.Method_GoblinFight;
            RoadPath_Results[2] = (int)RoadPath_Enum.Method_GoblinChat;
            RoadPath_Results[3] = (int)RoadPath_Enum.GoTo_GoblinAmbush_Woods;

            if (GoblinAmbush.Note_FoughtGoblins)
            {
                RoadPath_Options[1] = "Search the area";
                RoadPath_Options[2] = "Investigate the dark shapes on the road";

                RoadPath_Results[1] = (int)RoadPath_Enum.Method_SearchCamp;
                RoadPath_Results[2] = (int)RoadPath_Enum.GoTo_GoblinAmbush_DownedHorses;

                if (Quests.GoblinAmbush_FoundHorses)
                {
                    RoadPath_Options[2] = "Walk out to the downed horses in the road";
                }
            }

            Methods.PrintOptions(RoadPath_Options);
            return RoadPath_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = RoadPath_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)RoadPath_Enum.Method_GoblinFight:
                    GoblinFight();
                    break;

                case (int)RoadPath_Enum.Method_GoblinChat:
                    GoblinChat();
                    break;

                case (int)RoadPath_Enum.Method_SearchCamp:
                    SearchCamp();
                    break;

                case (int)RoadPath_Enum.GoTo_GoblinAmbush_Woods:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_Woods_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)RoadPath_Enum.GoTo_GoblinAmbush_DownedHorses:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_DownedHorses_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void GoblinFight()
        {
            Methods.Typewriter("(creeping up on the goblins text)");

            // abrupt location change
            Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_PathRoad_ID);

            // grid design
            var GridSize = Tuple.Create(2, 2);

            Dictionary<int, string> TileDescriptions = new Dictionary<int, string>()
            {
                { 1, "a fallen tree" },
                { 2, "a large boulder" },
                { 3, "a large tree" },
                { 4, "a small tree" }
            };

            Dictionary<int, List<GameItems.Item>> TileCover = new Dictionary<int, List<GameItems.Item>>()
            {
                { 1, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_ClusterLargeTrees) } },
                { 2, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_LargeTree),
                                                Methods.MakeItem(GameItems.Cover_SmallBoulder) } },
                { 3, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_SmallTree),
                                                Methods.MakeItem(GameItems.Cover_LargeBoulder) } },
                { 4, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_FallenTree),
                                                Methods.MakeItem(GameItems.Cover_ClusterSmallTrees) } }
            };

            var BattleGrid = Methods.MakeGrid(GridSize, TileDescriptions, TileCover);

            // combatants and positions
            List<Creatures.Creature> Enemies = new List<Creatures.Creature>
            {
                new Creatures.Goblin("Elf Ears"),
                new Creatures.Goblin("Pot Belly"),
                new Creatures.Goblin("Buck Tooth"),
                new Creatures.Goblin("Rat Breath"),
                new Creatures.Goblin("Hang Nail")
            };

            Player.Coordinates = new List<int> { 1, 2 };

            Fight GoblinFight = new Fight(BattleGrid, Enemies, "2x2");

            GoblinAmbush.Note_FoughtGoblins = true;
            GoblinAmbush.Note_CampFight = true;
        }

        private void GoblinChat()
        {
            Methods.Typewriter("(goblin chat)");
            Methods.Enter();
        }

        private void SearchCamp()
        {
            if (GoblinAmbush.Note_CampFight)
            {
                Dictionary<int, string> _choices = new Dictionary<int, string>();

                _choices[1] = "Search the goblins for any items of interest";
                _choices[2] = "Search the remains of the goblin's camp";

                Methods.PrintOptions(_choices);
                int _playerChoice = Methods.GetPlayerChoice(_choices.Count);

                switch (_playerChoice)
                {
                    case 1:
                        SearchGoblins();
                        return;

                    case 2:
                        break;
                }
            }

            GoblinAmbush.Skill_Investigation = Methods.RollStat(Player.WIS, "Investigation");

            if (GoblinAmbush.Skill_Investigation >= 10 && LocationInventory.Exists(item => item.Name.Equals(GameItems.Trinket_GoblinDice.Name)))
            {
                Methods.Typewriter("Amid the old and ragged cooking items and discarded clothing is a pouch with a set of carved bone " +
                    "pieces. Each one is a different shape but on whatever flat surfaces exist there are strange markings. They " +
                    "may be a part of some goblin ritual or pastime.");

                Dictionary<int, string> _choices = new Dictionary<int, string>();

                _choices[1] = "Take the bone dice";
                _choices[2] = "Leave the item";

                Methods.PrintOptions(_choices);
                int _playerChoice = Methods.GetPlayerChoice(_choices.Count);

                switch (_playerChoice)
                {
                    case 1:
                        GameItems.Item _goblinDice = LocationInventory.Find(item => item.Name.Equals(GameItems.Trinket_GoblinDice.Name));
                        Player.TakeItem(_goblinDice);
                        LocationInventory.Remove(_goblinDice);
                        break;

                    case 2:
                        break;
                }
            }

            if (GoblinAmbush.Skill_Investigation >= 5)
            {
                Methods.Typewriter("The wear and use of small cooking items and food scraps show that the " +
                    "goblins had been staying here for at least a few days.");
            }
            else
            {
                Methods.Typewriter("Nothing about the scraps seems particularly interesting.");
            }
        }

        private void SearchGoblins()
        {
            Console.WriteLine("(Search goblins)");

            Methods.SearchForItems(LocationInventory);
        }
    }
}
