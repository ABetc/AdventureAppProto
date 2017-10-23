using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    public class GameItems
    {
        // QUEST ITEMS:
        public static readonly Item QItems_ToolCart = new QuestItem(
            "tool cart",
            "Loaned for the shipment of Gundren's tools to Phandelin, the cart is packed full of excavation gear and supplies.");

        public static readonly Item QItems_TatteredNote = new QuestItem(
            "tattered note",
            "Pieced together from the goblin ambush scene, the note is addressed to the mayor of Phandelin and signed by Gundren.");

        // WEAPONS:
        public static readonly Item Weapon_HandXBow = new Weapon(
            "hand crossbow",
            "A lightweight crossbow made for one handed use.",
            false, 6, 1, 1, 50,
            "bolt");

        public static readonly Item Weapon_Javelin = new Weapon(
            "javelin",
            "A thin spear made for throwing.",
            false, 6, 1, 1, 5,
            "javelin");

        public static readonly Item Weapon_ShortSword = new Weapon(
            "shortsword",
            "A standard oversized knife.",
            false, 6, 1, 0, 10);

        public static readonly Item Weapon_GoblinBlade = new Weapon(
            "goblin blade",
            "A crude shank with a sharpened edge.",
            false, 4, 1, 0, 1);

        //POTIONS:
        public static readonly Item Potion_BasicHealing = new Potion(
            "healing potion",
            "A small vial of red liquid more healing wounds.",
            50, 6, 2, 4);

        //TRINKETS:
        public static readonly Item Trinket_GoblinDice = new Trinket(
            "goblin dice",
            "Small pieces of carved bone with unknown markings on various sides.",
            5);

        //COVER: 
        public static readonly Item Cover_FallenTree = new Cover(
            "fallen tree",
            8, 4);

        public static readonly Item Cover_LargeBoulder = new Cover(
            "large boulder",
            6, 6);

        public static readonly Item Cover_SmallBoulder = new Cover(
            "small boulder",
            3, 2);

        public static readonly Item Cover_LargeTree = new Cover(
            "large tree",
            4, 4);

        public static readonly Item Cover_SmallTree = new Cover(
            "small tree",
            3, 2);

        public static readonly Item Cover_ClusterSmallTrees = new Cover(
            "cluster of small trees",
            6, 4);

        // BASE CLASSES:
        public class Item
        {
            public string Name { get; set; }
            public int Amount = 1;

            public virtual string Description { get; set; }
            public virtual int Value { get; set; }
            public virtual int DamageDie { get; set; }
            public virtual int NumberOfDice { get; set; }
            public virtual bool StrengthBased { get; set; }
            public virtual int Range { get; set; }
            public virtual string Ammo { get; set; }
            public virtual int HealDieType { get; set; }
            public virtual int HealBaseNum { get; set; }
            public virtual int Room { get; set; }
            public virtual int RoomUsed { get; set; }
            public virtual int CoverBonus { get; set; }

            public virtual int DamageRoll() { return 0; }
            public virtual int DamageCrit() { return 0; }
            public virtual int Healing() { return 0; }

            public Item(string name)
            {
                Name = name;
            }
        }

        public class QuestItem : Item
        {
            public override string Description { get; set; }

            public QuestItem(string name, string desc) : base(name) { Description = desc; }
        }

        public class Weapon : Item
        {
            public override int DamageDie { get; set; }
            public override int NumberOfDice { get; set; }
            public override bool StrengthBased { get; set; }
            public override int Range { get; set; }
            public override string Ammo { get; set; }


            public Weapon(string name, string desc, bool strBased, int damage, int numOfDice, int range, int value, string ammo = null) :
                base(name)
            {
                Description = desc;
                StrengthBased = strBased;
                DamageDie = damage;
                NumberOfDice = numOfDice;
                Range = range;
                Ammo = ammo;
                Value = value;
            }

            public override int DamageRoll()
            {
                return Methods.RollD(DamageDie, NumberOfDice);
            }

            public override int DamageCrit()
            {
                return Methods.RollD(DamageDie, NumberOfDice) + DamageDie;
            }
        }

        public class Potion : Item
        {
            public override int HealDieType { get; set; }
            public override int NumberOfDice { get; set; }
            public override int HealBaseNum { get; set; }
            public override int Value { get; set; }

            public Potion(string name, string desc, int value, int dieType, int numOfDice, int healBase) :
                base(name)
            {
                Description = desc;
                HealDieType = dieType;
                NumberOfDice = numOfDice;
                HealBaseNum = healBase;
                Value = value;
            }

            public override int Healing()
            {
                return Methods.RollD(HealDieType, NumberOfDice) + HealBaseNum;
            }
        }

        public class Trinket : Item
        {
            public override string Description { get; set; }
            public override int Value { get; set; }

            public Trinket(string name, string desc, int value) : base(name)
            {
                Description = desc;
                Value = value;
            }
        }

        public class Cover : Item
        {
            public override int Room { get; set; }
            public override int RoomUsed { get; set; }
            public override int CoverBonus { get; set; }

            public Cover(string name, int room, int coverBonus, int roomUsed = 0) : base(name)
            {
                Room = room;
                RoomUsed = roomUsed;
                CoverBonus = coverBonus;
            }
        }
    }
}
