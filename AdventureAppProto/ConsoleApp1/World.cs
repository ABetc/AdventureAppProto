using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    class World
    {
        public static readonly List<Location> GameLocations = new List<Location>();

        // LOCATION IDs: GOBLIN AMBUSH
        public const double GoblinAmbush_RoadWest_ID = 0.01;
        public const double GoblinAmbush_Woods_ID = 0.02;
        public const double GoblinAmbush_PathRoad_ID = 0.03;
        public const double GoblinAmbush_PathCave_ID = 0.04;
        public const double GoblinAmbush_RoadDitch_ID = 0.05;
        public const double GoblinAmbush_DownedHorses_ID = 0.06;
        public const double GoblinAmbush_RoadEast_ID = 0.07;

        // LOCATION IDs: GOBLIN CAVE
        public const double GoblinCave_CaveEntrance_ID = 1.01;

        // LOCATION IDs: PHANDELIN TOWN
        public const double PhandelinTown_Entrance_ID = 2.01;

        static World()
        {
            PopulateLocations();
        }

        private static void PopulateLocations()
        {
            GameLocations.Add(new Locations.GoblinAmbush_RoadWest(GoblinAmbush_RoadWest_ID));
            GameLocations.Add(new Locations.GoblinAmbush_Woods(GoblinAmbush_Woods_ID));
            GameLocations.Add(new Locations.GoblinAmbush_PathRoad(GoblinAmbush_PathRoad_ID));
            GameLocations.Add(new Locations.GoblinAmbush_PathCave(GoblinAmbush_PathCave_ID));
            GameLocations.Add(new Locations.GoblinAmbush_RoadDitch(GoblinAmbush_RoadDitch_ID));
            GameLocations.Add(new Locations.GoblinAmbush_DownedHorses(GoblinAmbush_DownedHorses_ID));
            GameLocations.Add(new Locations.GoblinAmbush_RoadEast(GoblinAmbush_RoadEast_ID));
        }

        public static Location FindLocation(double id)
        {
            foreach (Location location in GameLocations)
            {
                if (location.LocationID == id)
                {
                    return location;
                }
            }
            return null;
        }
    }
}
