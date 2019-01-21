using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milestone_2
{
    class Business
    {
        public Business()
        {

        }

        private string bid;
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        private double latitude, longitude;

        // A 2d array for the hours a business is open
        // where the 0-6 index stands for days of the week
        // and the 0-1 index going down is open, and close times
        // in military time
        public string[,] hours { get; set; }

        // User rating from 0-5
        public double stars { get; set; }
        private int checkins;
        private int reviewCount;

        public string[] categories { get; set; }
    }
}
