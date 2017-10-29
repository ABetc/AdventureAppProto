using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
{
    class GoblinAmbush
    {
        public static bool Note_FoundPath;
        public static bool Note_FoundGoblins;
        public static bool Note_FoughtGoblins;
        public static bool Note_CampFight;
        public static bool Note_RoadFight;
        public static bool Note_WoodsFight;
        public static bool Note_FoundSnare;
        public static bool Note_SnareSprung;
        public static bool Note_HorsesMoved;

        public static int Skill_Investigation;
        public static int Skill_Survival;
        public static int Skill_Stealth;
        public static int Skill_SlightOfHand;
        public static int Skill_Athletics;

        public static void ResetSkills()
        {
            Skill_Investigation = 0;
            Skill_Survival = 0;
            Skill_Stealth = 0;
            Skill_SlightOfHand = 0;
        }
    }
}
