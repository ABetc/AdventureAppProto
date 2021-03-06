﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Creatures;
using Main.Locations;

namespace Main
{
    class Fight
    {
        private List<List<Methods.Tile>> BattleGrid;
        private List<Creature> Enemies;
        private List<Creature> EnemiesEscaped = new List<Creature>();
        private List<Creature> EnemiesKilled = new List<Creature>();
        private int SurpriseInitiative;
        private bool ParleyExists;
        private int TurnActions;

        private string GridImage_dimensions;

        Dictionary<string, string> GridImages = new Dictionary<string, string>()
        {
            { "2x2", "|(1,1)|(2,1)| N\n|(1,2)|(2,2)| ^\n" },
            { "2x3", "|(1,1)|(2,1)| N\n|(1,2)|(2,2)| ^\n|(1,3)|(2,3)| |\n" },
            { "3x2", "|(1,1)|(2,1)|(3,1)| N\n|(1,2)|(2,2)|(3,2)| ^\n" },
            { "3x3", "|(1,1)|(2,1)|(3,1)| N\n|(1,2)|(2,2)|(3,2)| ^\n|(1,3)|(2,3)|(3,3)| |\n" },
            { "3x4", "|(1,1)|(2,1)|(3,1)| N\n|(1,2)|(2,2)|(3,2)| ^\n|(1,3)|(2,3)|(3,3)| |\n|(1,4)|(2,4)|(3,4)|\n" },
            { "4x3", "|(1,1)|(2,1)|(3,1)|(4,1)| N\n|(1,2)|(2,2)|(3,2)|(4,2)| ^\n|(1,3)|(2,3)|(3,3)|(4,3)| |\n" },
            { "4x4", "|(1,1)|(2,1)|(3,1)|(4,1)| N\n|(1,2)|(2,2)|(3,2)|(4,2)| ^\n|(1,3)|(2,3)|(3,3)|(4,3)| |\n|(1,4)|(2,4)|(3,4)|(4,4)|\n" }
        };

        enum Player_Enum
        {
            Attack,
            Move,
            Grapple,
            BreakGrapple,
            ReleaseGrapple,
            Parley,
            Retreat,
            EndTurn
        }

        public Fight(List<List<Methods.Tile>> grid, List<Creature> enemies, string gridDimenstions, int surpriseBonus = 0, bool parley = false)
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
                    int coordX = Methods.RollD(BattleGrid[0].Count);
                    int coordY = Methods.RollD(BattleGrid.Count);
                    enemy.Coordinates.Add(coordX);
                    enemy.Coordinates.Add(coordY);
                }
            }

            // determine first turn by highest initiative 
            Player.Initiative = Methods.RollStat(Player.DEX) + SurpriseInitiative;
            Dictionary<string, int> EnemyInitiatives = new Dictionary<string, int>();

            foreach (Creature enemy in Enemies)
            {
                if (!EnemyInitiatives.ContainsKey(enemy.Type))
                {
                    EnemyInitiatives.Add(enemy.Type, enemy.Initiative);
                }
                else if (EnemyInitiatives.ContainsKey(enemy.Type) && enemy.Initiative > EnemyInitiatives[enemy.Type])
                {
                    EnemyInitiatives[enemy.Type] = enemy.Initiative;
                }
            }

            if (Player.Initiative >= EnemyInitiatives.Values.Max()) { PlayerTurn(); } else { EnemyTurn(); }

            // List<List<Creature>> FightResults = new List<List<Creature>> { EnemiesEscaped, EnemiesKilled };

            foreach (Creature enemy in EnemiesKilled)
            {
                foreach (GameItems.Item item in enemy.Inventory)
                {
                    Player.CurrentLocation.LocationInventory.Add(item);
                }                
            }
        }

        private void PlayerTurn()
        {
            Methods.Typewriter("(player turn)");    // temp

            TurnActions = 0;
            Dictionary<int, string> choices = new Dictionary<int, string>();
            Dictionary<int, int> results = new Dictionary<int, int>();

            // display map view along with Player and Enemy coordinates
            Console.WriteLine(GridImages[GridImage_dimensions]);                                                                                    // temp
            Console.WriteLine(Player.Name + ": " + Player.Coordinates[0] + ", " + Player.Coordinates[1]);                                           // temp
            foreach (Creature enemy in Enemies) { Console.WriteLine(enemy.Name + ": " + enemy.Coordinates[0] + ", " + enemy.Coordinates[1]); }      // temp
            Console.WriteLine();                                                                                                                    // temp

            // Player Turn Loop
            while (Player.IsAlive() && Enemies.Count > 0 && TurnActions < 2)
            {
                // check if player is grappled
                if (Player.GrappledBy.Count > 0)
                {
                    List<Creature> GatherSmall = Player.GrappledBy.Where(enemy => enemy.Size.Equals((int)Creature.Sizes_Enum.Small)).ToList();
                    List<string> GatherNames = Player.GrappledBy.Select(enemy => enemy.Name).ToList();

                    // check if player is restrained
                    if (GatherSmall.Count > 2 || (Player.GrappledBy.Count - GatherSmall.Count) > 1)
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
                        if (Player.GrappledBy.Count > 1)
                        {
                            Methods.Typewriter(string.Format("{0} is caught by {1} and {2} and can't move!",
                                Player.Name, Methods.ManyNames(GatherNames), GatherNames.Last()));
                        }
                        else
                        {
                            Methods.Typewriter(string.Format("{0} is caught by {1} and can't move!", 
                                Player.Name, GatherNames.First()));
                        }
                        
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

                    if (TurnActions > 0)
                    {
                        choices[Methods.AddKey(choices)] = "End turn";
                        results[Methods.AddKey(results)] = (int)Player_Enum.EndTurn;
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

                    case (int)Player_Enum.EndTurn:
                        TurnActions += 1;
                        break;
                }
            }

            if (Enemies.Count.Equals(0)) { return; }

            else if (!Player.IsAlive())
            {
                Methods.Typewriter(string.Format("The darkness closes in as {0} breathes his last... /n" +
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
                List<GameItems.Item> WeaponsCarried = new List<GameItems.Item>();
                int weaponNumber = 1;

                foreach (GameItems.Item item in Player.Inventory)
                {
                    if (item.GetType().Equals(typeof(GameItems.Weapon)))
                    {
                        Console.WriteLine(" " + weaponNumber + " - " + item.Name + " (range: " + (item.Range+1) + " tile(s))");
                        WeaponsCarried.Add(item);
                        weaponNumber++;
                    }
                }

                Console.WriteLine();
                int _playerChoice = Methods.GetPlayerChoice(WeaponsCarried.Count);
                ChosenWeapon = WeaponsCarried[_playerChoice - 1];

                // a ranged weapon can't be chosen unless all enemies are behind cover
                if (Enemies.Any(enemy => enemy.Coordinates.SequenceEqual(Player.Coordinates)) && ChosenWeapon.Range > 0)
                {
                    List<Creature> GatherClose = Enemies.Where(enemy => enemy.Coordinates.SequenceEqual(Player.Coordinates)).ToList();
                    List<Creature> EnemiesOut = GatherClose.Where(enemy => enemy.CoverName.Count().Equals(0)).ToList();

                    if (EnemiesOut.Count > 1)
                    {
                        List<string> EnemiesOut_Names = EnemiesOut.Select(enemy => enemy.Name).ToList();

                        Methods.Typewriter(string.Format("The {0} can't be used while {1} and {2} are in the open close by and " +
                            "about to attack!", ChosenWeapon.Name, Methods.ManyNames(EnemiesOut_Names), EnemiesOut_Names.Last()));
                    }
                    else
                    {
                        Methods.Typewriter(string.Format("The {0} can't be used while {1} is in the open close by and about to attack!",
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
                GatherInRange = GatherInRange.Where(enemy => enemy.CoverName.Count().Equals(0)).ToList();
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
            if (!Player.GrappledBy.Count.Equals(0))
            {
                Methods.Typewriter(string.Format("{0} makes an unwieldy strike on {1}!", Player.Name, Target.Name));
            }
            else if (Player.CoverName.Count().Equals(0) && ChosenWeapon.Range < 1 && !Target.CoverBonus.Equals(0))
            {
                Methods.Typewriter(string.Format("{0} rushes forward to attack {1}!", Player.Name, Target.Name));
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
            else if (Player.CoverName.Count().Equals(0) && ChosenWeapon.Range > 0)
            {
                Methods.Typewriter(string.Format("{0} takes aim at {1}!", Player.Name, Target.Name));
            }
            else if (Player.CoverName.Count().Equals(0) && ChosenWeapon.Range < 1)
            {
                Methods.Typewriter(string.Format("{0} attacks {1}!", Player.Name, Target.Name));
            }

            int RollToHit = Methods.RollD(20);

            if (RollToHit.Equals(20))
            {
                Console.WriteLine("Attack Roll: Natural 20");
                Console.WriteLine();
                Methods.Pause();
                Methods.Pause();
                Methods.Pause();

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

                // check if target was behind cover and adjust cover figures
                if (!Target.CoverName.Count().Equals(0))
                {
                    Methods.Tile TargetTile = Methods.FindTile(Target.Coordinates[0], Target.Coordinates[1], BattleGrid);
                    GameItems.Item TargetCover = TargetTile.Cover.Where(cover => cover.Name.Equals(Target.CoverName, StringComparison.Ordinal)).First();

                    TargetCover.RoomUsed -= Target.Size;
                    Target.CoverName = "";
                    Target.CoverBonus = 0;
                }

                // add Target to EnemiesKilled list, remove from Enemies and GrappledBy lists
                EnemiesKilled.Add(Target);
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
                Console.WriteLine(" " + (GatherClose.IndexOf(enemy) + 1) + " - " + enemy.Name);
            }

            Console.WriteLine();
            int _playerChoice = Methods.GetPlayerChoice(GatherClose.Count);
            Creature Target = GatherClose[_playerChoice - 1];

            // roll to grapple
            int RollToGrapple = Methods.RollStat(Player.STR) + Player.Proficiency;
            int StrengthContest = Methods.RollStat(Target.STR);

            Methods.Typewriter(string.Format("{0} reaches out for {1}...", Player.Name, Target.Name));

            if (RollToGrapple >= StrengthContest)
            {
                Target.GrappledBy.Add(Player.Name);

                Methods.Typewriter(string.Format("{0} is grappled! ({1} vs {2})", Target.Name, RollToGrapple, StrengthContest));

                // check if target was behind cover and adjust cover figures
                if (!Target.CoverName.Count().Equals(0))
                {
                    Methods.Tile TargetTile = Methods.FindTile(Target.Coordinates[0], Target.Coordinates[1], BattleGrid);
                    GameItems.Item TargetCover = TargetTile.Cover.Where(cover => cover.Name.Equals(Target.CoverName, StringComparison.Ordinal)).First();

                    TargetCover.RoomUsed -= Target.Size;
                    Target.CoverName = "";
                    Target.CoverBonus = 0;
                }

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
                Console.WriteLine(" " + (Player.GrappledBy.IndexOf(enemy) + 1) + " - " + enemy.Name);
            }

            Console.WriteLine();
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
                Methods.Typewriter(string.Format("{0} struggles but can't break free! ({1} vs {2})", 
                    Player.Name, RollToBreak, StrengthContest));

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
                Methods.Typewriter(string.Format("{0} is caught and can't move!", Player.Name));
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

                List<string> GatherCover = Methods.FindTile(Player.Coordinates[0], Player.Coordinates[1], BattleGrid)
                    .Cover.Where(cover => cover.RoomUsed < cover.Room).Select(cover => cover.Name).ToList();

                if (!Player.CoverName.Count().Equals(0) && GatherCover.Count > 1)
                {
                    choices[3] = "Change Cover";
                }
                else if (Player.CoverName.Count().Equals(0) && GatherCover.Count > 0)
                {
                    choices[3] = "Take Cover";
                }

                Methods.PrintOptions(choices);
                int playerChoice = Methods.GetPlayerChoice(choices.Count);

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
                    TurnActions += 1;
                    return;
                }
            }
            else
            {
                Methods.Typewriter("(there is no place to take cover)");
                TurnActions += 1;
                return;
            }

            if (TurnActions < 2) { Console.WriteLine("Take Cover?"); }

            Methods.PrintOptions(choices);
            int PlayerChoice = Methods.GetPlayerChoice(choices.Count);

            if (PlayerChoice.Equals(choices.Count))
            {
                TurnActions += 1;
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

            for (int x=0; x < Enemies.Count; x++)
            {
                Enemies[x].Fight(Enemies, EnemiesEscaped, BattleGrid);
            }

            Player.CheckHealth();
            PlayerTurn();
        }
    }
}
