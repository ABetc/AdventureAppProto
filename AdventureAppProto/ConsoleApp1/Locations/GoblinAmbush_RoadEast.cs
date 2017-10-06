using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush_RoadEast : Location
    {
        public override double LocationID { get; set; }
        public override int LocationVisitCount { get; set; }
        public override int LastVisitedLocation { get; set; }
        public override List<GameItems.Item> LocationInventory { get; set; }

        private Dictionary<int, string> RoadEast_Options = new Dictionary<int, string>();
        private Dictionary<int, int> RoadEast_Results = new Dictionary<int, int>();

        enum RoadEast_Enum
        {
            Method_MoveCart,

            GoTo_GoblinAmbush_DownedHorses,
            GoTo_GoblinAmbush_RoadDitch,
            GoTo_GoblinAmbush_Woods,

            GoTo_PhandelinTown_Entrance
        }

        public GoblinAmbush_RoadEast(double id)
        {
            LocationID = id;
            LocationInventory = new List<GameItems.Item>();
        }

        public override void OpeningText()
        {
            if (Player.PreviousLocation.LocationID != LocationID)
            {
                // if never visited location
                if (LocationVisitCount.Equals(0) && !Quests.GoblinAmbush_FoundHorses)
                {
                    Methods.Typewriter("The road stretches ahead to Phandelin, a journey of at least a few days by foot " +
                        "or cart. Behind the dark shapes still lie motionless in the road.");
                }
                else if (LocationVisitCount.Equals(0) && Quests.GoblinAmbush_FoundHorses)
                {
                    Methods.Typewriter("The road stretches ahead to Phandelin, a journey of at least a few days by foot " +
                        "or cart. Behind the dead horses still lie motionless in the road.");
                }
                else
                {
                    Methods.Typewriter("The road to Phandelin stretches ahead, a journey of a few days by foot or cart.");
                }

                // if tool cart remains at location after the first visit to location
                if (LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_ToolCart.Name)) && LocationVisitCount > 0)
                {
                    Methods.Typewriter("The mule and tool cart rest idle in the middle of the road.");
                }
            }
        }

        public override int LocationOptions()
        {
            RoadEast_Options[1] = "Sneak quietly into the nearby woods";
            RoadEast_Options[2] = "Head to Phandelin by foot";
            RoadEast_Options[3] = "Sneak back to investigate the dark shapes";
            RoadEast_Options[4] = "Walk back towards the dark shapes";

            RoadEast_Results[1] = (int)RoadEast_Enum.GoTo_GoblinAmbush_Woods;
            RoadEast_Results[2] = (int)RoadEast_Enum.GoTo_PhandelinTown_Entrance;
            RoadEast_Results[3] = (int)RoadEast_Enum.GoTo_GoblinAmbush_RoadDitch;
            RoadEast_Results[4] = (int)RoadEast_Enum.GoTo_GoblinAmbush_DownedHorses;

            if (Quests.GoblinAmbush_FoundHorses)
            {
                RoadEast_Options[3] = "Stealthily investigate the downed horses";
                RoadEast_Options[4] = "Walk back to the downed horses";
            }

            if (LocationInventory.Exists(item => item.Name.Equals(GameItems.QItems_ToolCart.Name)))
            {
                RoadEast_Options[2] = "Drive cart forward";

                RoadEast_Results[2] = (int)RoadEast_Enum.Method_MoveCart;
            }

            Methods.PrintOptions(RoadEast_Options);
            return RoadEast_Options.Count;
        }

        public override void LocationResults(int playerChoice)
        {
            int EnumNumber = RoadEast_Results[playerChoice];

            switch (EnumNumber)
            {
                case (int)RoadEast_Enum.Method_MoveCart:
                    MoveCart();
                    break;

                case (int)RoadEast_Enum.GoTo_GoblinAmbush_DownedHorses:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_DownedHorses_ID);
                    break;

                case (int)RoadEast_Enum.GoTo_GoblinAmbush_RoadDitch:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadDitch_ID);
                    break;

                case (int)RoadEast_Enum.GoTo_GoblinAmbush_Woods:
                    Player.CurrentLocation = World.FindLocation(World.GoblinAmbush_Woods_ID);
                    break;

                case (int)RoadEast_Enum.GoTo_PhandelinTown_Entrance:
                    Player.CurrentLocation = World.FindLocation(World.PhandelinTown_Entrance_ID);
                    break;
            }
        }

        private void MoveCart()
        {
            GameItems.Item _toolcart = LocationInventory.Find(item => item.Name.Equals(GameItems.QItems_ToolCart.Name));
            LocationInventory.Remove(_toolcart);
            World.FindLocation(World.PhandelinTown_Entrance_ID).LocationInventory.Add(_toolcart);

            Player.CurrentLocation = World.FindLocation(World.PhandelinTown_Entrance_ID);
        }
    }
}