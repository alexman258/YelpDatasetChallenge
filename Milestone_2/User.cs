using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milestone_2
{
    class User
    {
        public User()
        {

        }

        public string uid { get; set; }
        public string name { get; set; }

        // Yelping since, given in month-year
        private string yelpingSince;

        // Rating given to user between 0-5
        public double stars { get; set; }

        // List of users friends
        public string[] friends { get; set; }

        // number of fans a user can have
        private int fans;
        private int reviewCount;

        // An array of 3 for the votes where
        // Index 0: cool votes
        // Index 1: funny votes
        // Index 2: useful votes
        private int[] votes = new int[3];



    }
}
