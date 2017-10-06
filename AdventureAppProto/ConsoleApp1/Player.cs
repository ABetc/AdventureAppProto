using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Creatures;

namespace Main
{
    public class Player
    {
        public const string Name = "Ben";
        public const string Pronoun = "he";
        public const string Possessive = "his";
        public static int HitPoints = 25;
        public static int Damage;
        public static int ArmorClass = 14;
        public static int Proficiency = 2;
        public static int STR = 8;
        public static int DEX = 14;
        public static int CON = 12;
        public static int INT = 14;
        public static int WIS = 10;
        public static int CHA = 16;
        public static int Initiative;
        public static string CoverName = "";
        public static int CoverBonus = 0;

        public static Location CurrentLocation { get; set; }
        public static Location PreviousLocation { get; set; }
        public static int TravelCount { get; set; }
        public static List<GameItems.Item> Inventory = new List<GameItems.Item>();

        public static List<int> Coordinates = new List<int>();
        public static List<Creature> GrappledBy = new List<Creature>();

        // weather

        static Player()
        {
            CurrentLocation = World.FindLocation(World.GoblinAmbush_RoadWest_ID);
            PreviousLocation = World.FindLocation(World.GoblinAmbush_Woods_ID);

            Inventory.Add(new GameItems.Weapon(
                GameItems.Weapon_ShortSword.Name,
                GameItems.Weapon_ShortSword.Description,
                GameItems.Weapon_ShortSword.StrengthBased,
                GameItems.Weapon_ShortSword.DamageDie,
                GameItems.Weapon_ShortSword.NumberOfDice,
                GameItems.Weapon_ShortSword.Range,
                GameItems.Weapon_ShortSword.Value));
        }

        public static int WeaponStat(GameItems.Item weapon)
        {
            if (weapon.StrengthBased) { return STR; } else { return DEX; }
        }

        public static void CheckHealth()
        {
            if (Damage < HitPoints)
            {
                string text = string.Format("{0}'s HitPoitns", Name);
                Methods.Print(text, HitPoints - Damage);
            }
            else
            {
                string text = string.Format("{0}'s HitPoitns", Name);
                Methods.Print(text, HitPoints - Damage);

                Methods.Typewriter(string.Format("The darkness closes in as {0} breathes his last... " +
                    "So ends the tale of this gallant rogue.", Name));
                Environment.Exit(0);
            }
        }

        public static bool IsAlive() { return Damage < HitPoints; }

        public static void TakeItem(GameItems.Item item)
        {
            Inventory.Add(item);
            Methods.Pause();
            Methods.Pause();
            Methods.Pause();
            Methods.Typewriter("(Picked up " + item.Name + ")");
            Methods.Pause();
            Methods.Pause();
            Methods.Pause();
        }
    }
}
