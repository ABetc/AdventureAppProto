using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main
{
    public class SuperAdventure
    {
        public SuperAdventure()
        {
            ///*
            Methods.Pause();
            Methods.Typewriter("Adventure App Prototype");
            Methods.Enter();
            Methods.Typewriter("Ben is a stalwart rogue grizzled with a life of sails and swashbuckling on " +
                "the high seas, but, somehow, he now finds himself delivering a donkey cart full of mining " +
                "tools to the Dwarf Gundren Rockseeker in the small forest town of Phandelin. Gundren hired " +
                "Ben only a few days ago and left early with a bodyguard to travel to Phandelin ahead of " +
                "the cart to get his business affairs in order. Ben began traveling later with the supplies.");
            Methods.Typewriter("All was well on the forest road until Ben spies something strange in the distance...");

            GameLoop();
            //*/
        }

        private void GameLoop()
        {
            while (Player.IsAlive())
            {
                // display location text and check Player health
                Player.CurrentLocation.OpeningText();

                // display options, gather choice limit, get player choice
                int NumberOfOptions = Player.CurrentLocation.LocationOptions();
                int PlayerChoice = Methods.GetPlayerChoice(NumberOfOptions);

                // update travel count, visit count, last visited count and location
                Player.TravelCount++;
                Player.CurrentLocation.LocationVisitCount++;
                Player.CurrentLocation.LastVisitedLocation = Player.TravelCount;
                Player.PreviousLocation = Player.CurrentLocation;

                // act on choice and check Player health
                Player.CurrentLocation.LocationResults(PlayerChoice);
            }
        }
    }
}
