using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_RoadWest : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> RoadWest_Options = new Dictionary<int, string>();
        private Dictionary<int, int> RoadWest_Results = new Dictionary<int, int>();

        private enum RoadWest_Enum
        {
            Method_MoveCart,

            GoTo_GoblinAmbush_Woods,
            GoTo_GoblinAmbush_DownedHorses,
            GoTo_GoblinAmbush_RoadDitch
        };

        public GoblinAmbush_RoadWest(double id)
        {
            LocationID = id;
            LocationInventory = new List<GameItems.Item>();

            LocationInventory.Add(new GameItems.QuestItem(
                GameItems.QItems_ToolCart.Name,
                GameItems.QItems_ToolCart.Description));
        }

        public override void OpeningText()
        {
            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // if never visited location
                if (Player.CurrentLocation.LocationVisitCount.Equals(0))
                {
                    Methods.Typewriter("There are dark shapes on the road ahead. It is difficult to make out, " +
                        "but they seem motionless as if piled in the middle of the road. Ben squints, but can't " +
                        "make out if it is a living thing or not...");
                }
                // if returning to location but not yet investigated the dark shapes
                else if (Player.CurrentLocation.LocationVisitCount > 0 && !Quests.GoblinAmbush_FoundHorses)
                {
                    Methods.Typewriter("The mysterious dark shapes have not moved.");
                }
                // if returning to location and already investigated the dark shapes
                else if (Quests.GoblinAmbush_FoundHorses)
                {
                    Methods.Typewriter("Down the road lie the motionless shapes of the dead horses.");
                }
            }

            // if tool cart remains at location after the first visit to location
            if (LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_ToolCart.Name)) && LocationVisitCount > 0)
            {
                Methods.Typewriter("The mule and tool cart rest idle in the middle of the road.");
            }
        }

        public override int LocationOptions()
        {
            RoadWest_Options[1] = "Sneak quietly into the woods";
            RoadWest_Options[2] = "Walk closer to the dark shapes";
            RoadWest_Options[3] = "Stealthily investigate the road up ahead";

            RoadWest_Results[1] = (int)RoadWest_Enum.GoTo_GoblinAmbush_Woods;
            RoadWest_Results[2] = (int)RoadWest_Enum.GoTo_GoblinAmbush_DownedHorses;
            RoadWest_Results[3] = (int)RoadWest_Enum.GoTo_GoblinAmbush_RoadDitch;

            if (Quests.GoblinAmbush_FoundHorses)
            {
                RoadWest_Options[2] = "Walk to the dead horses";
            }

            if (LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_ToolCart.Name)))
            {
                RoadWest_Options[2] = "Drive cart forward";

                RoadWest_Results[2] = (int)RoadWest_Enum.Method_MoveCart;
            }

            Methods.PrintOptions(RoadWest_Options);
            return RoadWest_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = RoadWest_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)RoadWest_Enum.Method_MoveCart:
                    MoveCart();
                    break;

                case (int)RoadWest_Enum.GoTo_GoblinAmbush_Woods:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_Woods_ID);
                    break;

                case (int)RoadWest_Enum.GoTo_GoblinAmbush_DownedHorses:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_DownedHorses_ID);
                    break;

                case (int)RoadWest_Enum.GoTo_GoblinAmbush_RoadDitch:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadDitch_ID);
                    break;
            }
        }

        private void MoveCart()
        {
            GameItems.Item _toolcart = LocationInventory.Find(item => item.Name.Equals(GameItems.QItems_ToolCart.Name));
            LocationInventory.Remove(_toolcart);
            World.FindLocation(World.GoblinAmbush_DownedHorses_ID).LocationInventory.Add(_toolcart);

            Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_DownedHorses_ID);
        }
    }
}
