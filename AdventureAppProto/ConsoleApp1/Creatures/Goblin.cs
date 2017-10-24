using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Creatures
{
    public class Goblin : Creature
    {
        public override string Name { get; set; }
        public override int HitPoints { get; set; }
        public override int Damage { get; set; }
        public override int Size { get; set; }
        public override int ArmorClass { get; set; }
        public override int STR { get; set; }
        public override int DEX { get; set; }
        public override int CON { get; set; }
        public override int WIS { get; set; }
        public override int INT { get; set; }
        public override int CHA { get; set; }
        public override int Initiative { get; set; }
        public override string CoverName { get; set; }
        public override int CoverBonus { get; set; }

        private static int PrivateInititative;
        //public ??? Personality;

        public override List<GameItems.Item> Inventory { get; set; }
        public override List<int> Coordinates { get; set; }
        public override List<string> GrappledBy { get; set; }

        public Goblin(string name, int coordX = 0, int coordY = 0)
        {
            Coordinates = new List<int>();
            GrappledBy = new List<string>();

            Name = name;
            Coordinates.Add(coordX);
            Coordinates.Add(coordY);

            HitPoints = Methods.RollD(6, 2);
            Size = (int)Sizes_Enum.Small;
            ArmorClass = 12;
            STR = 8;
            DEX = 14;
            CON = 10;
            WIS = 10;
            INT = 8;
            CHA = 8;
            CoverName = "";
            CoverBonus = 0;

            Inventory = new List<GameItems.Item>();
            Inventory.Add(new GameItems.Weapon(
                GameItems.Weapon_GoblinBlade.Name,
                GameItems.Weapon_GoblinBlade.Description,
                GameItems.Weapon_GoblinBlade.StrengthBased,
                GameItems.Weapon_GoblinBlade.DamageDie,
                GameItems.Weapon_GoblinBlade.NumberOfDice,
                GameItems.Weapon_GoblinBlade.Range,
                GameItems.Weapon_GoblinBlade.Value));
            Inventory.Add(new GameItems.Weapon(
                GameItems.Weapon_Javelin.Name,
                GameItems.Weapon_Javelin.Description,
                GameItems.Weapon_Javelin.StrengthBased,
                GameItems.Weapon_Javelin.DamageDie,
                GameItems.Weapon_Javelin.NumberOfDice,
                GameItems.Weapon_Javelin.Range,
                GameItems.Weapon_Javelin.Value,
                GameItems.Weapon_Javelin.Ammo));

            RollInitiative();
            Initiative = PrivateInititative;
        }

        private void RollInitiative() { PrivateInititative = Methods.RollStat(DEX); }

        public override bool IsAlive() { return Damage < HitPoints; }

        private int WeaponStat(GameItems.Item weapon)
        {
            if (weapon.StrengthBased) { return STR; } else { return DEX; }
        }

        public override void Fight(List<Creature> listOfEnemies, List<Creature> enemiesEscaped, List<List<Methods.Tile>> battleGrid)
        {
            // fight behavior: 
            // if 3+ enemies are close: grapple, else: melee and run away
            // if HP below 1/2: retreat
            if (Damage < HitPoints * 0.5 && listOfEnemies.Count > 2)
            {
                if (Coordinates.SequenceEqual(Player.Coordinates))
                {
                    if (listOfEnemies.Where(enemy => enemy.Coordinates.SequenceEqual(Player.Coordinates)).Count() > 2 &&
                        Player.GrappledBy.Count < 3)
                    { Grapple(battleGrid); }

                    else { Melee(battleGrid); Move(battleGrid); }
                }
                else { Ranged(); Move(battleGrid); }
            }
            else { Flee(listOfEnemies, enemiesEscaped, battleGrid); }
        }

        private void Melee(List<List<Methods.Tile>> battleGrid)
        {
            // check if grappled
            if (GrappledBy.Count > 1)
            {
                Methods.Typewriter(string.Format("{0} is completely restrained!", Name));
                BreakGrapple();
                return;
            }
            else if (!GrappledBy.Count.Equals(0))
            {
                Methods.Typewriter(string.Format("{0} is being grappled by {1}!", Name, GrappledBy.First()));
                BreakGrapple();
                return;
            }

            // check if behind cover
            if (!CoverName.Count().Equals(0))
            {
                Methods.Tile CurrentTile = Methods.FindTile(Coordinates[0], Coordinates[1], battleGrid);
                GameItems.Cover CurrentCover = CurrentTile.Cover.Where(cover => cover.Name.Equals(CoverName)).First();

                CurrentCover.RoomUsed -= Size;
                CoverName = "";
                CoverBonus = 0;
            }

            // roll to hit
            GameItems.Item BestMelee = Inventory.FindAll(item => item.GetType().Equals(typeof(GameItems.Weapon))).Where(
                item => item.Range.Equals(0)).OrderByDescending(item => item.DamageDie).First();

            Methods.Typewriter(string.Format("{0} attacks with a {1}!", Name, BestMelee.Name));

            int RollToHit = Methods.RollD(20);

            if (RollToHit.Equals(20))
            {
                Console.WriteLine("Attack Roll: Natural 20");
                Console.WriteLine();
                Methods.Pause();
                Methods.Pause();
                Methods.Pause();

                int damage = BestMelee.DamageCrit() + Methods.StatMod(WeaponStat(BestMelee));
                Player.Damage += damage;

                Methods.Typewriter("Critical Hit!");
                Methods.Print("Damage", damage);
            }
            else if (!RollToHit.Equals(20))
            {
                RollToHit += Methods.StatMod(WeaponStat(BestMelee));
                Methods.Print("Attack Roll", RollToHit);

                if (RollToHit >= Player.ArmorClass)
                {
                    int damage = BestMelee.DamageRoll() + Methods.StatMod(WeaponStat(BestMelee));
                    Player.Damage += damage;

                    Methods.Typewriter("Hit!");
                    Methods.Print("Damage", damage);
                }
                else
                {
                    Methods.Typewriter("Miss!");
                }
            }
        }

        private void Ranged()
        {
            GameItems.Item BestRanged = Inventory.FindAll(item => item.GetType().Equals(typeof(GameItems.Weapon))).Where(
                item => !item.Range.Equals(0)).OrderByDescending(item => item.DamageDie).First();

            // check if in range
            if (BestRanged.Range >= Math.Abs(Player.Coordinates[0] - Coordinates[0]) &&
                BestRanged.Range >= Math.Abs(Player.Coordinates[1] - Coordinates[1]))
            {
                // roll to hit
                Methods.Typewriter(string.Format("{0} attacks with a {1}!", Name, BestRanged.Name));

                int RollToHit = Methods.RollD(20);

                if (RollToHit.Equals(20))
                {
                    Console.WriteLine("Attack Roll: Natural 20");
                    Console.WriteLine();
                    Methods.Pause();
                    Methods.Pause();
                    Methods.Pause();

                    int damage = BestRanged.DamageCrit() + Methods.StatMod(WeaponStat(BestRanged));
                    Player.Damage += damage;

                    Methods.Typewriter("Critical Hit!");
                    Methods.Print("Damage", damage);
                }
                else if (!RollToHit.Equals(20))
                {
                    RollToHit += Methods.StatMod(WeaponStat(BestRanged));
                    Methods.Print("Attack Roll", RollToHit);

                    if (RollToHit >= Player.ArmorClass + Player.CoverBonus)
                    {
                        int damage = BestRanged.DamageRoll() + Methods.StatMod(WeaponStat(BestRanged));
                        Player.Damage += damage;

                        Methods.Typewriter("Hit!");
                        Methods.Print("Damage", damage);
                    }
                    else if (!Player.CoverBonus.Equals(0) && RollToHit > Player.ArmorClass)
                    {
                        Methods.Typewriter(string.Format("The {0} smacks into the {1}!", BestRanged.Ammo, Player.CoverName));
                    }
                    else
                    {
                        Methods.Typewriter("Miss!");
                    }
                }
            }
            else
            {
                Methods.Typewriter(string.Format("{0} attacks with a {1}...but the attack falls short!", Name, BestRanged.Name));
            }
        }

        private void Grapple(List<List<Methods.Tile>> battleGrid)
        {
            // check if grappling
            if (Player.GrappledBy.Any(enemy => enemy.Name.Equals(Name)))
            {
                Methods.Typewriter(string.Format("{0} has {1} grappled and won't let go!", Name, Player.Name));
                return;
            }

            // check if behind cover
            if (!CoverName.Count().Equals(0))
            {
                Methods.Tile CurrentTile = Methods.FindTile(Coordinates[0], Coordinates[1], battleGrid);
                GameItems.Cover CurrentCover = CurrentTile.Cover.Where(cover => cover.Name.Equals(CoverName)).First();

                CurrentCover.RoomUsed -= Size;
                CoverName = "";
                CoverBonus = 0;

            }

            // roll to grab
            Methods.Typewriter(string.Format("{0} leaps at {1}!", Name, Player.Name));

            int RollToGrab = Methods.RollStat(STR);
            int StrengthContest = Methods.RollStat(Player.STR);

            if (RollToGrab >= StrengthContest)
            {
                Methods.Typewriter(string.Format("{0} is grappled! ({1} vs {2})", Player.Name, StrengthContest, RollToGrab));
                Methods.Pause();
                Player.GrappledBy.Add(this);
            }
            else
            {
                Methods.Typewriter(string.Format("{0} evades {1}'s grasp! ({2} vs {3})", Player.Name, Name, StrengthContest, RollToGrab));
                Methods.Pause();
            }
        }

        private void BreakGrapple()
        {
            // roll to break grapple
            int RollToBreak = Methods.RollStat(STR);
            int StrengthContest = Methods.RollStat(Player.STR);

            if (RollToBreak >= StrengthContest)
            {
                Methods.Typewriter(string.Format("{0} struggles furiously... And manages to wrench free!", Name));
                GrappledBy.Remove(Player.Name);
            }
            else
            {
                Methods.Typewriter(string.Format("{0} struggles furiously... And can't break away!", Name));
            }
        }

        private void Move(List<List<Methods.Tile>> battleGrid)
        {
            Methods.Tile CurrentTile = Methods.FindTile(Coordinates[0], Coordinates[1], battleGrid);

            // check if grappling
            if (Player.GrappledBy.Any(enemy => enemy.Name.Equals(Name)))
            {
                Methods.Typewriter(string.Format("{0} releases its hold on {1}.", Name, Player.Name));
                Player.GrappledBy.Remove(this);
            }

            // check if grappled
            if (!GrappledBy.Count.Equals(0)) { return; }

            // find available coordinates
            Dictionary<int, int> AvailableDirections = new Dictionary<int, int>();

            if (Methods.TileExists(Coordinates[0], Coordinates[1] - 1, battleGrid) &&
                !Player.Coordinates.SequenceEqual(new List<int> { Coordinates[0], Coordinates[1] - 1 }))
            {
                AvailableDirections[Methods.AddKey(AvailableDirections)] = 1;
            }
            if (Methods.TileExists(Coordinates[0], Coordinates[1] + 1, battleGrid) &&
                !Player.Coordinates.SequenceEqual(new List<int> { Coordinates[0], Coordinates[1] + 1 }))
            {
                AvailableDirections[Methods.AddKey(AvailableDirections)] = 2;
            }
            if (Methods.TileExists(Coordinates[0] + 1, Coordinates[1], battleGrid) &&
                !Player.Coordinates.SequenceEqual(new List<int> { Coordinates[0] + 1, Coordinates[1] }))
            {
                AvailableDirections[Methods.AddKey(AvailableDirections)] = 3;
            }
            if (Methods.TileExists(Coordinates[0] - 1, Coordinates[1], battleGrid) &&
                !Player.Coordinates.SequenceEqual(new List<int> { Coordinates[0] - 1, Coordinates[1] }))
            {
                AvailableDirections[Methods.AddKey(AvailableDirections)] = 4;
            }

            if (!AvailableDirections.Count.Equals(0))
            {
                int RandomChoice = Methods.RollD(AvailableDirections.Count);
                int enumNumber = AvailableDirections[RandomChoice];

                // adjust cover values at location before change
                if (!CoverName.Count().Equals(0))
                {
                    GameItems.Cover CurrentCover = CurrentTile.Cover.Where(cover => cover.Name.Equals(CoverName)).First();

                    CurrentCover.RoomUsed -= Size;
                    CoverName = "";
                    CoverBonus = 0;
                }

                // change coordinate position
                switch (enumNumber)
                {
                    case 1:
                        Coordinates[1] -= 1;
                        break;

                    case 2:
                        Coordinates[1] += 1;
                        break;

                    case 3:
                        Coordinates[0] += 1;
                        break;

                    case 4:
                        Coordinates[0] -= 1;
                        break;
                }

                //update current player tile and check for new cover
                CurrentTile = Methods.FindTile(Coordinates[0], Coordinates[1], battleGrid);

                List<GameItems.Cover> GatherCover = CurrentTile.Cover.Where(cover => cover.RoomUsed < cover.Room &&
                !cover.Name.Equals(Player.CoverName)).ToList();

                if (!GatherCover.Count.Equals(0))
                {
                    RandomChoice = Methods.RollD(GatherCover.Count);
                    GatherCover[RandomChoice - 1].RoomUsed += Size;
                    CoverName = GatherCover[RandomChoice - 1].Name;
                    CoverBonus = GatherCover[RandomChoice - 1].CoverBonus;

                    Methods.Typewriter(string.Format("{0} runs and takes cover behind {1}.", Name, CoverName));
                }
                else
                {
                    Methods.Typewriter(string.Format("{0} runs towards {1}.", Name, CurrentTile.Description));
                }
            }
        }

        private void Flee(List<Creature> listOfEnemies, List<Creature> enemiesFled, List<List<Methods.Tile>> battleGrid)
        {
            // check position
            if (Coordinates.SequenceEqual(Player.Coordinates))
            {
                Methods.Typewriter(string.Format("{0} flees from {1}!", Name, Player.Name));

                if (!GrappledBy.Count.Equals(0))
                {
                    BreakGrapple();
                }

                if (GrappledBy.Count.Equals(0))
                {
                    Move(battleGrid);
                }
            }
            else
            {
                Methods.Typewriter(string.Format("{0} escapes!", Name));
                listOfEnemies.Remove(this);
                enemiesFled.Add(this);
            }
        }
    }
}
