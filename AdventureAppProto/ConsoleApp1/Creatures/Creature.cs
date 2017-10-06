using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Creatures
{
    public abstract class Creature
    {
        public abstract string Name { get; set; }
        public abstract int HitPoints { get; set; }
        public abstract int Damage { get; set; }
        public abstract int Size { get; set; }
        public abstract int ArmorClass { get; set; }
        public abstract int STR { get; set; }
        public abstract int DEX { get; set; }
        public abstract int CON { get; set; }
        public abstract int WIS { get; set; }
        public abstract int INT { get; set; }
        public abstract int CHA { get; set; }
        public abstract int Initiative { get; set; }
        public abstract string CoverName { get; set; }
        public abstract int CoverBonus { get; set; }

        public abstract List<GameItems.Item> Inventory { get; set; }
        public abstract List<int> Coordinates { get; set; }
        public abstract List<string> GrappledBy { get; set; }

        public enum Sizes_Enum
        {
            Tiny,
            Small,
            Medium,
            Large,
            Huge
        }

        public abstract bool IsAlive();
        public abstract void Fight(List<Creature> listOfEnemies, List<Creature> enemiesEscaped, List<List<Methods.Tile>> battleGrid);
    }
}
