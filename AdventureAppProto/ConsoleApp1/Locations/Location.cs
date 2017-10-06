using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    public abstract class Location
    {
        public abstract double LocationID { get; set; }
        public abstract int LocationVisitCount { get; set; }
        public abstract int LastVisitedLocation { get; set; }
        public abstract List<GameItems.Item> LocationInventory { get; set; }

        public abstract void OpeningText();
        public abstract int LocationOptions();
        public abstract void LocationResults(int playerChoice);
    }

    /*
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Locations
    {
        class AreaName_NewSpot : Location
        {
            public override double LocationID { get; set; }
            public override int LocationVisitCount { get; set; }
            public override int LastVisitedLocation { get; set; }
            public override List<GameItems.Item> LocationInventory { get; set; }

            private Dictionary<int, string> NewSpot_Options = new Dictionary<int, string>();
            private Dictionary<int, int> NewSpot_Results = new Dictionary<int, int>();

            enum NewSpot_Enum
            {

            }

            public AreaName_NewSpot(double id)
            {
                LocationID = id;
            }

            public override void OpeningText()
            {
                if (Player.PreviousLocation.LocationID != LocationID)
                {

                }
            }

            public override int LocationOptions()
            {

                Methods.PrintOptions(NewSpot_Options);
                return NewSpot_Options.Count;
            }

            public override void LocationResults(int playerChoice)
            {
                int EnumNumber = NewSpot_Results[playerChoice];

                switch (EnumNumber)
                {

                }
            }
        }
    }
    */

}
