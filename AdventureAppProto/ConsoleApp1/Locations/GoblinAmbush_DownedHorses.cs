using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_DownedHorses : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> DownedHorses_Options = new Dictionary<int, string>();
        private Dictionary<int, int> DownedHorses_Results = new Dictionary<int, int>();

        enum DownedHorses_Enum
        {
            Method_MoveHorses,
            Method_SearchArea,
            Method_ContinueOn,

            GoTo_GoblinAmbush_Woods,
            GoTo_GoblinAmbush_RoadEast
        }

        public GoblinAmbush_DownedHorses(double id)
        {
            LocationID = id;
            LocationInventory = new List<GameItems.Item>();

            LocationInventory.Add(new GameItems.QuestItem(
                GameItems.QItems_TatteredNote.Name,
                GameItems.QItems_TatteredNote.Description));
        }

        public override void OpeningText()
        {
            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // if never visited location
                if (LocationVisitCount.Equals(0))
                {
                    Methods.Typewriter("After taking some steps down the road and peering intently at the dark shapes ahead, it's " +
                    "clear they are a pair of dead horses lying half on top of one another. The scene is somewhat gruesome; " +
                    "crude sticks seem to be poking up out of the creature's bodies, which on further inspection are the broken " +
                    "and battered shafts of tens of arrows, and the remains of emptied baggage lies scattered all around. There " +
                    "is no sign of the riders, although each animal still carries an empty saddle.");

                    Quests.GoblinAmbush_FoundHorses = true;
                }
                else
                {
                    Methods.Typewriter("The horses' stink is pungent and a cloud of flies has formed.");
                }

                // check if already fought goblins
                if (!GoblinAmbush.Note_FoughtGoblins)
                {
                    Methods.Typewriter(string.Format("Suddenly there is a shriek from the woods to the north, and from some hidden spot " +
                        "behind the trees leap a group of five goblins! A few rush forward in different directions and begin to " +
                        "surroud {0} while a couple others remain behind and nock arrows to small goblinoid bows.", Player.Name));
                    Methods.Typewriter("OHhh... Human!", "red");
                    Methods.Typewriter(string.Format("The lead goblin eyes {0} gleefully while licking its lips", Player.Name));

                    GoblinFight();
                }
                else if (GoblinAmbush.Note_RoadFight)
                {
                    Methods.Typewriter("Spread around the fallen horses are the corpses of the goblin ambushers.");
                }
            }
        }

        public override int LocationOptions()
        {
            DownedHorses_Options[1] = "Search the area";
            DownedHorses_Options[2] = "Try to move the horses from the road";
            DownedHorses_Options[3] = "Continue down the road";
            DownedHorses_Options[4] = "Leave the road and sneak into the woods";

            DownedHorses_Results[1] = (int)DownedHorses_Enum.Method_SearchArea;
            DownedHorses_Results[2] = (int)DownedHorses_Enum.Method_MoveHorses;
            DownedHorses_Results[3] = (int)DownedHorses_Enum.GoTo_GoblinAmbush_RoadEast;
            DownedHorses_Results[4] = (int)DownedHorses_Enum.GoTo_GoblinAmbush_Woods;

            if (LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_ToolCart.Name)))
            {
                DownedHorses_Results[3] = (int)DownedHorses_Enum.Method_ContinueOn;
            }

            Methods.PrintOptions(DownedHorses_Options);
            return DownedHorses_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = DownedHorses_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)DownedHorses_Enum.Method_SearchArea:
                    SearchArea();
                    break;

                case (int)DownedHorses_Enum.Method_MoveHorses:
                    MoveHorses();
                    break;

                case (int)DownedHorses_Enum.Method_ContinueOn:
                    ContinueOn();
                    break;

                case (int)DownedHorses_Enum.GoTo_GoblinAmbush_RoadEast:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadEast_ID);
                    GoblinAmbush.ResetSkills();
                    break;

                case (int)DownedHorses_Enum.GoTo_GoblinAmbush_Woods:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_Woods_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }

        private void GoblinFight()
        {
            // grid design
            var GridSize = Tuple.Create(4, 3);

            Dictionary<int, string> TileDescriptions = new Dictionary<int, string>()
            {
                { 1, "the forest" },
                { 2, "the forest" },
                { 3, "the forest" },
                { 4, "the forest" },
                { 5, "the west end of the road" },
                { 6, "the middle of the road" },
                { 7, "the downed horses" },
                { 8, "the estern end of the road" },
                { 9, "the forest" },
                { 10, "the forest" },
                { 11, "the forest" },
                { 12, "the forest" }
            };

            Dictionary<int, List<GameItems.Item>> TileCover = new Dictionary<int, List<GameItems.Item>>()
            {
                { 1, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_ClusterLargeTrees) } },
                { 2, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_LargeBoulder),
                                                Methods.MakeItem(GameItems.Cover_SmallTree) } },
                { 3, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_LargeTree),
                                                Methods.MakeItem(GameItems.Cover_SmallTree),
                                                Methods.MakeItem(GameItems.Cover_SmallBoulder) } },
                { 4, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_LargeTree),
                                                Methods.MakeItem(GameItems.Cover_ClusterSmallTrees) } },
                { 5, new List<GameItems.Item>() },
                { 6, new List<GameItems.Item>() },
                { 7, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_DownedHorses) } },
                { 8, new List<GameItems.Item>() },
                { 9, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_ClusterSmallTrees),
                                                Methods.MakeItem(GameItems.Cover_MediumBoulder) } },
                { 10, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_FallenTree),
                                                 Methods.MakeItem(GameItems.Cover_SmallTree) } },
                { 11, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_ClusterLargeTrees) } },
                { 12, new List<GameItems.Item> { Methods.MakeItem(GameItems.Cover_SmallBoulder),
                                                 Methods.MakeItem(GameItems.Cover_MediumBoulder),
                                                 Methods.MakeItem(GameItems.Cover_SmallTree) } },
            };

            // check for tool cart
            if (LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_ToolCart.Name)))
            {
                TileDescriptions[5] = "the tool cart";
                TileCover[5].Add(Methods.MakeItem(GameItems.Cover_ToolCart));
            }

            List<List<Methods.Tile>> BattleGrid = Methods.MakeGrid(GridSize, TileDescriptions, TileCover);

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

            Fight GoblinFight = new Fight(BattleGrid, Enemies, "4x3");

            GoblinAmbush.Note_FoughtGoblins = true;
            GoblinAmbush.Note_RoadFight = true;
        }

        private void SearchArea()
        {
            if (GoblinAmbush.Note_RoadFight)
            {
                Dictionary<int, string> _choices = new Dictionary<int, string>();

                _choices[1] = "Search for loot or other items";
                _choices[2] = "Examine the horses";

                Methods.PrintOptions(_choices);
                int _playerChoice = Methods.GetPlayerChoice(_choices.Count);

                switch (_playerChoice)
                {
                    case 1:
                        Methods.SearchForItems(Player.Inventory);
                        return;

                    case 2:
                        break;
                }
            }

            GoblinAmbush.Skill_Investigation = Methods.RollStat(Player.WIS, "Investigation");

            if (GoblinAmbush.Skill_Investigation >= 15 && LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_TatteredNote.Name)))
            {
                Methods.Typewriter("Amongst the debris are the scatterings of a note written by Gundren and " +
                    "addressed to the mayor of Phandelin; it seems to relate to a business deal between the " +
                    "two parties. The ripped paper is not complete, but the signature is clear - Gundren was " +
                    "here and was ambushed!");
                Quests.GundrenKidnapped_TwoKidnapped = true;

                Dictionary<int, string> _choices = new Dictionary<int, string>();

                _choices[1] = "Take the tattered note";
                _choices[2] = "Leave the item";

                Methods.PrintOptions(_choices);
                int _playerChoice = Methods.GetPlayerChoice(_choices.Count);

                switch (_playerChoice)
                {
                    case 1:
                        GameItems.Item _tatteredNote = LocationInventory.Find(item => item.Name.Equals(GameItems.QItems_TatteredNote.Name));
                        LocationInventory.Remove(_tatteredNote);
                        Player.TakeItem(_tatteredNote);
                        break;

                    case 2:
                        break;
                }
            }
            else if (GoblinAmbush.Skill_Investigation > 10 && !Quests.GundrenKidnapped_TwoKidnapped)
            {
                Methods.Typewriter("A mercenary company sigil is on the one saddle that likely carryied a human. There " +
                    "are also some conspicuous drag marks leading away from the scene and into the woods. As the size " +
                    "of the saddles also suggest, the victims seem to have been an adult human--or similar in size--" +
                    "and a dwarf.");
                Methods.Typewriter(string.Format("{0} takes a deep breath and shakes his head. If these are not the " +
                    "travelling horses of Gundren and his hired bodyguard, the coincidence would be very surprising.",
                    Player.Name));
                Methods.Typewriter("Sure as I can tell, this 'ere be a case of luck most terrible.", "cyan");
                Quests.GundrenKidnapped_TwoKidnapped = true;
            }
            else if (GoblinAmbush.Skill_Investigation > 5)
            {
                Methods.Typewriter("The arrows sticking from the horse's flanks have the rough make and smaller size " +
                    "of goblin creations. Curiously the horses do not seem to have been bothered with at all. They " +
                    "appear to have fallen where they stood after being peppered with arrows.");
            }
            else
            {
                Methods.Typewriter("The site is filled with sights and smells of death and rot. Even standing nearby " +
                    "is beginning to become nigh unbearable. Soon only the flies will be able to examine the scene.");
            }            
        }

        private void MoveHorses()
        {
            if (GoblinAmbush.Skill_Athletics.Equals(0))
            {
                GoblinAmbush.Skill_Athletics = Methods.RollStat(Player.STR, "Athletics");
            }

            if (GoblinAmbush.Skill_Athletics >= 16)
            {
                Methods.Typewriter(string.Format("The animal's bodies are slick and immensely heavy, but with more then a few " +
                    "pauses to catch {0} breath, {1} maneages to push them off to the side slightly. There is just " +
                    "enough space for the cart to pass through.", Player.Possessive, Player.Name));
                GoblinAmbush.Note_HorsesMoved = true;
            }
            else if (GoblinAmbush.Skill_Athletics >= 12)
            {
                Methods.Typewriter(string.Format("Rolling each animal over gets them shifted slightly but with all {0}'s effort " +
                    "the creatures remain spread across the center of the road.", Player.Name));
            }
            else
            {
                Methods.Typewriter(string.Format("With a mighty shove, {0} puts his full weight against the animals but even after " +
                    "multiple attempts they will not budge.", Player.Name));
            }
        }

        private void ContinueOn()
        {
            Dictionary<int, string> _choices = new Dictionary<int, string>();

            _choices[1] = "Drive the cart forward";
            _choices[2] = "Leave the cart and go by foot";

            Methods.PrintOptions(_choices);
            int _playerChoice = Methods.GetPlayerChoice(_choices.Count);

            switch (_playerChoice)
            {
                case 1:
                    if (!GoblinAmbush.Note_HorsesMoved)
                    {
                        Methods.Typewriter("As the cart rolls forward it becomes clear that the dead horses block " +
                            "the road too completely for the cart to be able to pass.");
                    }
                    else
                    {
                        Methods.Typewriter("Rolling past the dead creatures causes the donkey to skitter slightly " +
                            "and let out a nervous whinney.");
                        Methods.Typewriter("There, there now beastie, we'll be fine again soon enough", "cyan");
                        Methods.Typewriter(string.Format("{0} tries to comfort the animal and they make it past " +
                            "the aftermath of the amubsh and back onto open road.", Player.Name));

                        //move tool cart to next location
                        GameItems.Item _toolcart = LocationInventory.Find(item => item.Name.Equals(GameItems.QItems_ToolCart.Name));
                        LocationInventory.Remove(_toolcart);
                        World.FindLocation(World.GoblinAmbush_RoadEast_ID).LocationInventory.Add(_toolcart);

                        Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadEast_ID);
                        GoblinAmbush.ResetSkills();
                    }
                    break;

                case 2:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadEast_ID);
                    GoblinAmbush.ResetSkills();
                    break;
            }
        }
    }
}