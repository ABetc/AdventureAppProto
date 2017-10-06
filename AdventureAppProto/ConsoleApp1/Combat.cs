using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Creatures;

namespace Main
{
    class Combat
    {
        private List<List<Methods.Tile>> BattleGrid;
        private List<Creature> Enemies;
        private List<Creature> EnemiesEscaped;
        private List<Creature> EnemiesKilled;
        private int SurpriseInitiative;
        private bool ParleyExists;
        private int TurnActions;

        private string GridImage_dimensions;

        Dictionary<string, string> GridImages = new Dictionary<string, string>()
        {
            { "2x2", "|(1,1)|(2,1)| ^\n|(1,2)|(2,2)| N\n" },
            { "2x3", "|(1,1)|(2,1)| ^\n|(1,2)|(2,2)| |\n|(1,3)|(2,3)| N\n" },
            { "3x2", "|(1,1)|(2,1)|(3,1)| ^\n|(1,2)|(2,2)|(3,2)| N\n" },
            { "3x3", "|(1,1)|(2,1)|(3,1)| ^\n|(1,2)|(2,2)|(3,2)| |\n|(1,3)|(2,3)|(3,3)| N\n" },
            { "3x4", "|(1,1)|(2,1)|(3,1)| ^\n|(1,2)|(2,2)|(3,2)| |\n|(1,3)|(2,3)|(3,3)| N\n|(1,4)|(2,4)|(3,4)|\n" },
            { "4x3", "|(1,1)|(2,1)|(3,1)|(4,1)| ^\n|(1,2)|(2,2)|(3,2)|(4,2)| |\n|(1,3)|(2,3)|(3,3)|(4,3)| N\n" },
            { "4x4", "|(1,1)|(2,1)|(3,1)|(4,1)| ^\n|(1,2)|(2,2)|(3,2)|(4,2)| |\n|(1,3)|(2,3)|(3,3)|(4,3)| N\n|(1,4)|(2,4)|(3,4)|(4,4)|\n" }
        };

        enum Player_Enum
        {
            Attack,
            Move,
            Grapple,
            BreakGrapple,
            ReleaseGrapple,
            Parley,
            Retreat
        }

        public Combat(List<List<Methods.Tile>> grid, List<Creature> enemies, string gridDimenstions, int surpriseBonus = 0, bool parley = false)
        {
            BattleGrid = grid;
            Enemies = enemies;
            SurpriseInitiative = surpriseBonus;
            ParleyExists = parley;

            GridImage_dimensions = gridDimenstions;

            FightStart();
        }

        public void FightStart()
        {
            // assign enemy positions if defaults exist disallowing player position
            foreach (Creature enemy in Enemies)
            {
                while (enemy.Coordinates.Sum().Equals(0) || enemy.Coordinates.SequenceEqual(Player.Coordinates))
                {
                    enemy.Coordinates.Clear();
                    int coordX = Methods.RollD(BattleGrid.Count);
                    int coordY = Methods.RollD(BattleGrid[0].Count);
                    enemy.Coordinates.Add(coordX);
                    enemy.Coordinates.Add(coordY);
                }
            }

            // determine first turn by highest initiative 
            Player.Initiative = Methods.RollStat(Player.DEX) + SurpriseInitiative;
            int MaxInitiative = 0;
            foreach (Creature enemy in Enemies)
            {
                if (enemy.Initiative > MaxInitiative) { MaxInitiative = enemy.Initiative; }
            }

            if (Player.Initiative >= MaxInitiative) { PlayerTurn(); } else { EnemyTurn(); }
        }

        private void PlayerTurn()
        {
            Methods.Typewriter("(player turn)");    // temp

            TurnActions = 0;
            Dictionary<int, string> choices = new Dictionary<int, string>();
            Dictionary<int, int> results = new Dictionary<int, int>();

            // display map view along with Player and Enemy coordinates
            Console.WriteLine(GridImages[GridImage_dimensions]);                                                                                    // temp
            Console.WriteLine();                                                                                                                    // temp    
            Console.WriteLine(Player.Name + ": " + Player.Coordinates[0] + ", " + Player.Coordinates[1]);                                           // temp
            foreach (Creature enemy in Enemies) { Console.WriteLine(enemy.Name + ": " + enemy.Coordinates[0] + ", " + enemy.Coordinates[1]); }      // temp
            Console.WriteLine();                                                                                                                    // temp    

            // Player Turn Loop
            while (Player.IsAlive() && Enemies.Count > 0 && TurnActions < 2)
            {
                if (Player.GrappledBy.Count > 0)
                {
                    List<Creature> GatherSmall = Player.GrappledBy.Where(enemy => enemy.Size.Equals(Creature.Sizes_Enum.Small)).ToList();
                    List<string> GatherNames = Player.GrappledBy.Select(enemy => enemy.Name).ToList();

                    if (GatherSmall.Count > 2 || Player.GrappledBy.Count - GatherSmall.Count > 1)
                    {
                        Methods.Typewriter(string.Format("{0} is completely restrained by {1} and {2}!",
                            Player.Name, Methods.ManyNames(GatherNames), GatherNames.Last()));

                        choices[1] = "Break Grapple";
                        results[1] = (int)Player_Enum.BreakGrapple;

                        if (ParleyExists)
                        {
                            choices[2] = "Try and negotiate";
                            results[2] = (int)Player_Enum.Parley;
                        }
                    }
                    else
                    {
                        Methods.Typewriter(string.Format("{0} is caught by {1} and can't move!", Player.Name, GatherNames.First()));

                        choices[1] = "Attack";
                        choices[2] = "Break Grapple";

                        results[1] = (int)Player_Enum.Attack;
                        results[2] = (int)Player_Enum.BreakGrapple;

                        foreach (Creature enemy in Enemies)
                        {
                            if (enemy.GrappledBy.Contains(Player.Name))
                            {
                                Methods.Typewriter(string.Format("{0} still has {1} in a tight grip!", Player.Name, enemy.Name));

                                choices[3] = "Release Grapple";
                                results[3] = (int)Player_Enum.ReleaseGrapple;
                            }
                            else
                            {
                                choices[3] = "Grapple";
                                results[3] = (int)Player_Enum.Grapple;
                            }
                        }
                    }
                }
                else
                {
                    choices[1] = "Attack";
                    choices[2] = "Move";

                    results[1] = (int)Player_Enum.Attack;
                    results[2] = (int)Player_Enum.Move;

                    List<Creature> GatherClose = Enemies.Where(enemy => enemy.Coordinates.SequenceEqual(Player.Coordinates)).ToList();

                    if (GatherClose.Count > 0)
                    {
                        choices[Methods.AddKey(choices)] = "Grapple";
                        results[Methods.AddKey(results)] = (int)Player_Enum.Grapple;
                    }

                    if (ParleyExists)
                    {
                        choices[Methods.AddKey(choices)] = "Try and negotiate";
                        results[Methods.AddKey(results)] = (int)Player_Enum.Parley;
                    }

                    // if Tile at Player coordinates has a "flee" object, add "retreat" options
                }

                Methods.PrintOptions(choices);
                int PlayerChoice = Methods.GetPlayerChoice(choices.Count);
                int _enumNumber = results[PlayerChoice];

                switch (_enumNumber)
                {
                    case (int)Player_Enum.Attack:
                        Attack();
                        break;

                    case (int)Player_Enum.Move:
                        Move();
                        break;

                    case (int)Player_Enum.Grapple:
                        Grapple();
                        break;

                    case (int)Player_Enum.BreakGrapple:
                        BreakGrapple();
                        break;

                    case (int)Player_Enum.ReleaseGrapple:
                        ReleaseGrapple();
                        break;

                    case (int)Player_Enum.Parley:
                        Parley();
                        break;

                    case (int)Player_Enum.Retreat:
                        Retreat();
                        break;
                }
            }

            if (Enemies.Count.Equals(0)) { return; }

            else if (!Player.IsAlive())
            {
                Methods.Typewriter(string.Format("The darkness closes in as {0} breathes his last... " +
                    "So ends the tale of this gallant rogue.", Player.Name));
                Environment.Exit(0);
            }

            EnemyTurn();
        }

        private void Attack()
        {
            Methods.Typewriter("(attack)");     // temp

            // choose weapon
            GameItems.Item ChosenWeapon;

            while (true)
            {
                Console.WriteLine(" Attack with:");
                int weaponNumber = 1;
                List<GameItems.Item> WeaponsCarried = new List<GameItems.Item>();

                foreach (GameItems.Item item in Player.Inventory)
                {
                    if (item.GetType().Equals(typeof(GameItems.Weapon)))
                    {
                        Console.WriteLine(" " + weaponNumber + " - " + item.Name + " (range: " + item.Range + " tile(s))");
                        WeaponsCarried.Add(item);
                        weaponNumber++;
                    }
                }

                Console.WriteLine();
                int _playerChoice = Methods.GetPlayerChoice(WeaponsCarried.Count);
                ChosenWeapon = WeaponsCarried[_playerChoice - 1];

                // a ranged weapon can't be chosen unless all enemies are behind cover
                if (!Enemies.Any(enemy => enemy.CoverName.Count().Equals(0)) && ChosenWeapon.Range > 0)
                {
                    List<Creature> EnemiesOut = Enemies.Where(enemy => enemy.CoverName.Count().Equals(0)).ToList();

                    if (EnemiesOut.Count > 1)
                    {
                        List<string> EnemiesOut_Names = EnemiesOut.Select(enemy => enemy.Name).ToList();

                        Methods.Typewriter(string.Format("The {0} can't be used while {1} and {2} are out in the open and " +
                            "about to attack!", ChosenWeapon.Name, Methods.ManyNames(EnemiesOut_Names), EnemiesOut_Names.Last()));
                    }
                    else
                    {
                        Methods.Typewriter(string.Format("The {0} can't be used while {1} is out in open and about to attack!",
                            ChosenWeapon.Name, EnemiesOut.First().Name));
                    }
                }
                else { break; }
            }

            // choose target
            List<Creature> GatherInRange = Enemies.Where(enemy =>
                ChosenWeapon.Range >= Math.Abs(enemy.Coordinates[0] - Player.Coordinates[0]) &&
                ChosenWeapon.Range >= Math.Abs(enemy.Coordinates[1] - Player.Coordinates[1])).ToList();

            if (!Player.GrappledBy.Count.Equals(0))
            {
                GatherInRange = GatherInRange.Where(enemy => !enemy.CoverName.Count().Equals(0)).ToList();
            }

            if (GatherInRange.Count.Equals(0))
            {
                Methods.Typewriter("Nothing is in range!");
                return;
            }

            Console.WriteLine(" Choose a target:");
            int targetNumber = 1;

            foreach (Creature enemy in GatherInRange)
            {
                if (!enemy.CoverName.Count().Equals(0))
                {
                    Console.WriteLine(" " + targetNumber + " - " + enemy.Name + " (behind " + enemy.CoverName + ")");
                }
                else
                {
                    Console.WriteLine(" " + targetNumber + " - " + enemy.Name);
                }

                targetNumber++;
            }

            Console.WriteLine();
            int __playerChoice = Methods.GetPlayerChoice(GatherInRange.Count);
            Creature Target = GatherInRange[__playerChoice - 1];

            // roll to hit
            if (Player.GrappledBy.Count.Equals(0))
            {
                Methods.Typewriter(string.Format("{0} makes an unwieldy strike on {1}!", Player.Name, Target.Name));
            }
            else if (!Player.CoverName.Count().Equals(0) && ChosenWeapon.Range < 1)
            {
                Methods.Typewriter(string.Format("{0} rushes out from cover to attack {1}!", Player.Name, Target.Name));
                Player.CoverName = "";
            }
            else if (!Player.CoverName.Count().Equals(0) && ChosenWeapon.Range > 0)
            {
                Methods.Typewriter(string.Format("{0} leans out and takes aim at {1}!", Player.Name, Target.Name));
            }
            else if (Player.CoverName.Count().Equals(0) && ChosenWeapon.Range < 1)
            {
                Methods.Typewriter(string.Format("{0} rushes forward to attack {1}!", Player.Name, Target.Name));
            }
            else if (Player.CoverName.Count().Equals(0) && ChosenWeapon.Range > 0)
            {
                Methods.Typewriter(string.Format("{0} takes aim at {1}!", Player.Name, Target.Name));
            }

            int RollToHit = Methods.RollD(20);

            if (RollToHit.Equals(20))
            {
                Methods.Print("Attack Roll", RollToHit);

                int damage = ChosenWeapon.DamageCrit() + Methods.StatMod(Player.WeaponStat(ChosenWeapon));
                Target.Damage += damage;

                Methods.Typewriter("Critical Hit!");
                Methods.Print("Damage", damage);
            }
            else if (!RollToHit.Equals(20))
            {
                RollToHit += Methods.StatMod(Player.WeaponStat(ChosenWeapon)) + Player.Proficiency;
                Methods.Print("Attack Roll", RollToHit);

                if (ChosenWeapon.Range > 0)
                {
                    if (RollToHit >= Target.ArmorClass + Target.CoverBonus)
                    {
                        int damage = ChosenWeapon.DamageRoll() + Methods.StatMod(Player.WeaponStat(ChosenWeapon));
                        Target.Damage += damage;
                        Methods.Typewriter("Hit!");
                        Methods.Print("Damage", damage);
                    }
                    else
                    {
                        if (Target.CoverBonus > 0 && RollToHit >= Target.ArmorClass)
                        {
                            Methods.Typewriter(string.Format("The {0} smacks harmlessly into the {1}.", ChosenWeapon.Ammo, Target.CoverName));
                        }
                        else
                        {
                            Methods.Typewriter("Miss!");
                        }
                    }
                }
                else
                {
                    if (RollToHit > Target.ArmorClass)
                    {
                        int damage = ChosenWeapon.DamageRoll() + Methods.StatMod(Player.WeaponStat(ChosenWeapon));
                        Target.Damage += damage;
                        Methods.Typewriter("Hit!");
                        Methods.Print("Damage", damage);
                    }
                    else
                    {
                        Methods.Typewriter("Miss!");
                    }
                }
            }

            // check if Target was killed
            if (!Target.IsAlive())
            {
                Methods.Typewriter(string.Format("{0} is dead!", Target.Name));

                EnemiesKilled.Add(Enemies.Where(enemy => enemy.Name.Equals(Target.Name)).First());
                Player.GrappledBy = Player.GrappledBy.Where(enemy => !enemy.Name.Equals(Target.Name)).ToList();
                Enemies = Enemies.Where(enemy => !enemy.Name.Equals(Target.Name)).ToList();
            }

            TurnActions += 2;
            if (TurnActions.Equals(2)) { Move(); }
        }

        private void Grapple()
        {
            Methods.Typewriter("(grapple)");    // temp

            Dictionary<int, string> choices = new Dictionary<int, string>();

            // check if already grappling
            foreach (Creature enemy in Enemies)
            {
                if (enemy.GrappledBy.Contains(Player.Name))
                {
                    Methods.Typewriter(string.Format("{0} still has {1} in a tight grip.", Player.Name, enemy.Name));

                    choices[1] = "Release Grapple";
                    choices[2] = "Keep the hold";

                    Methods.PrintOptions(choices);
                    int playerChoice = Methods.GetPlayerChoice(choices.Count);

                    switch (playerChoice)
                    {
                        case 1:
                            ReleaseGrapple();
                            break;

                        case 2:
                            return;
                    }
                }
            }

            // choose target
            List<Creature> GatherClose = Enemies.Where(enemy => enemy.Coordinates.SequenceEqual(Player.Coordinates)).ToList();
            Console.WriteLine(" Choose a target:");

            foreach (Creature enemy in GatherClose)
            {
                Console.WriteLine(" " + GatherClose.IndexOf(enemy) + 1 + " - " + enemy.Name);
            }

            int _playerChoice = Methods.GetPlayerChoice(GatherClose.Count);
            Creature Target = GatherClose[_playerChoice - 1];

            // roll to grapple
            int RollToGrapple = Methods.RollStat(Player.STR) + Player.Proficiency;
            int StrengthContest = Methods.RollStat(Target.STR);

            if (RollToGrapple >= StrengthContest)
            {
                Target.GrappledBy.Add(Player.Name);

                Methods.Typewriter(string.Format("{0} is grappled!", Target.Name));

                TurnActions += 2;
                return;
            }
            else
            {
                Methods.Typewriter(string.Format("Miss! ({0} vs {1})", RollToGrapple, StrengthContest));
            }

            TurnActions += 2;
            if (TurnActions.Equals(2)) { Move(); }
        }

        private void BreakGrapple()
        {
            Methods.Typewriter("(break grapple)");      // temp

            // choose a target
            Console.WriteLine(" Choose a target:");

            foreach (Creature enemy in Player.GrappledBy)
            {
                Console.WriteLine(" " + Player.GrappledBy.IndexOf(enemy) + 1 + " - " + enemy.Name);
            }

            int playerChoice = Methods.GetPlayerChoice(Player.GrappledBy.Count);
            Creature Target = Player.GrappledBy[playerChoice - 1];

            // roll to break grapple
            int RollToBreak = Methods.RollStat(Player.STR) + Player.Proficiency;
            int StrengthContest = Methods.RollStat(Target.STR);

            if (RollToBreak >= StrengthContest)
            {
                Methods.Typewriter(string.Format("{0} broke free of {1}'s grip!", Player.Name, Target.Name));
                Player.GrappledBy.Remove(Target);
            }
            else
            {
                Methods.Typewriter(string.Format("{0} struggles but can't break free!", Player.Name));

                TurnActions += 2;
                return;
            }

            TurnActions += 2;
            if (TurnActions.Equals(2)) { Move(); }
        }

        private void ReleaseGrapple()
        {
            Methods.Typewriter("(release grapple)");    // temp

            foreach (Creature enemy in Enemies)
            {
                if (enemy.GrappledBy.Contains(Player.Name))
                {
                    enemy.GrappledBy.Remove(Player.Name);

                    Methods.Typewriter(string.Format("{0} squirms free!", enemy.Name));
                }
            }
        }

        private void Move()
        {
            Methods.Typewriter("(move)");   // temp

            Dictionary<int, string> choices = new Dictionary<int, string>();

            // check if grappled or grappling
            if (!Player.GrappledBy.Count.Equals(0))
            {
                Methods.Typewriter(string.Format("{0} is still caught and can't move!", Player.Name));
                return;
            }

            foreach (Creature enemy in Enemies)
            {
                if (enemy.GrappledBy.Contains(Player.Name))
                {
                    Methods.Typewriter(string.Format("{0} still has {1} in a tight grip.", Player.Name, enemy.Name));

                    choices[1] = "Release Grapple";
                    choices[2] = "Keep the hold";

                    Methods.PrintOptions(choices);
                    int playerChoice = Methods.GetPlayerChoice(choices.Count);

                    switch (playerChoice)
                    {
                        case 1:
                            ReleaseGrapple();
                            break;

                        case 2:
                            return;
                    }
                }
            }

            // check if already made an action
            if (TurnActions.Equals(2))
            {
                choices[1] = "Keep Position";
                choices[2] = "Move";

                Methods.PrintOptions(choices);
                int playerChoice = Methods.GetPlayerChoice(choices.Count);

                List<string> GatherCover = Methods.FindTile(Player.Coordinates[0], Player.Coordinates[1], BattleGrid)
                    .Cover.Where(cover => cover.RoomUsed < cover.Room).Select(cover => cover.Name).ToList();

                if (GatherCover.Count > 1 && !Player.CoverName.Count().Equals(0))
                {
                    choices[3] = "Change Cover";
                }
                else if (!GatherCover.Count.Equals(0) && !Player.CoverName.Count().Equals(0))
                {
                    choices[3] = "Take Cover";
                }

                switch (playerChoice)
                {
                    case 1:
                        TurnActions += 1;
                        return;

                    case 2:
                        Player.CoverName = "";
                        Player.CoverBonus = 0;
                        break;

                    case 3:
                        OfferCover();
                        return;
                }
            }

            // change coordinate position
            choices.Clear();
            Dictionary<int, int> results = new Dictionary<int, int>();

            if (Methods.TileExists(Player.Coordinates[0], Player.Coordinates[1] - 1, BattleGrid))
            {
                choices[Methods.AddKey(choices)] = "Move north";
                results[Methods.AddKey(results)] = 1;
            }
            if (Methods.TileExists(Player.Coordinates[0], Player.Coordinates[1] + 1, BattleGrid))
            {
                choices[Methods.AddKey(choices)] = "Move south";
                results[Methods.AddKey(results)] = 2;
            }
            if (Methods.TileExists(Player.Coordinates[0] + 1, Player.Coordinates[1], BattleGrid))
            {
                choices[Methods.AddKey(choices)] = "Move east";
                results[Methods.AddKey(results)] = 3;
            }
            if (Methods.TileExists(Player.Coordinates[0] - 1, Player.Coordinates[1], BattleGrid))
            {
                choices[Methods.AddKey(choices)] = "Move west";
                results[Methods.AddKey(results)] = 4;
            }

            Methods.PrintOptions(choices);
            int _playerChoice = Methods.GetPlayerChoice(choices.Count);
            int enumNumber = results[_playerChoice];

            switch (enumNumber)
            {
                case 1:
                    Player.Coordinates[1] -= 1;
                    choices[_playerChoice] = "north";
                    break;

                case 2:
                    Player.Coordinates[1] += 1;
                    choices[_playerChoice] = "south";
                    break;

                case 3:
                    Player.Coordinates[0] += 1;
                    choices[_playerChoice] = "east";
                    break;

                case 4:
                    Player.Coordinates[0] -= 1;
                    choices[_playerChoice] = "west";
                    break;
            }

            Methods.Typewriter(string.Format("{0} runs {1} towards {2}.",
                Player.Name, choices[_playerChoice], Methods.FindTile(Player.Coordinates[0], Player.Coordinates[1], BattleGrid).Description));

            OfferCover();
        }

        private void OfferCover()
        {
            Dictionary<int, string> choices = new Dictionary<int, string>();
            Methods.Tile CurrentTile = Methods.FindTile(Player.Coordinates[0], Player.Coordinates[1], BattleGrid);

            if (!CurrentTile.Cover.Count.Equals(0))
            {
                List<string> EnemyCover = Enemies.Where(enemy => enemy.Coordinates.SequenceEqual(Player.Coordinates) &&
                !enemy.CoverName.Count().Equals(0)).Select(enemy => enemy.CoverName).ToList();

                foreach (GameItems.Cover cover in CurrentTile.Cover)
                {
                    if (!EnemyCover.Contains(cover.Name) && !cover.Name.Equals(Player.CoverName))
                    {
                        choices[Methods.AddKey(choices)] = cover.Name;
                    }
                }

                if (!choices.Count.Equals(0))
                {
                    choices[Methods.AddKey(choices)] = "Keep position";
                }
                else
                {
                    Methods.Typewriter("Enemies have taken all positions of cover!");
                }
            }
            else
            {
                Methods.Typewriter("(there is no place to take cover)");
            }

            if (TurnActions < 2) { Console.WriteLine("Take Cover?"); }

            Methods.PrintOptions(choices);
            int PlayerChoice = Methods.GetPlayerChoice(choices.Count);

            if (PlayerChoice.Equals(choices.Count))
            {
                TurnActions += 1;
                return;
            }
            else
            {
                Player.CoverName = choices[PlayerChoice];
                Player.CoverBonus = CurrentTile.Cover.Where(cover => cover.Name.Equals(Player.CoverName)).Select(cover => cover.CoverBonus).First();

                TurnActions += 1;
            }
        }

        private void Retreat()
        {
            Methods.Typewriter("(retreat)");                            // temp
            Methods.Typewriter("(there's no escaping this one...)");    // temp
        }

        private void Parley()
        {
            Methods.Typewriter("(parley)");                                         // temp
            Methods.Typewriter("(there's nothing to be said at the moment...)");    // temp
        }

        private void EnemyTurn()
        {
            Methods.Typewriter("(enemy turn)");                                         // temp

            foreach (Creature enemy in Enemies)
            {
                enemy.Fight(Enemies, EnemiesEscaped, BattleGrid);
            }

            Player.CheckHealth();
            PlayerTurn();
        }
    }
}
