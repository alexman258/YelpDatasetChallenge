using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Milestone_2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        SingletonDB DB = SingletonDB.GetInstance;

        // Function to populate the checkins chart
        public void CheckinChart(string bid)
       {

            string query = String.Format("Select Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday from yelp_checkins where bid = '{0}'", bid);
            var data = DB.RunQuery(query);


            List<KeyValuePair<string, int>> myChartData = new List<KeyValuePair<string, int>>();

            foreach (var kvp in data)
            {

                int checkinsPerDay = 0;
                foreach(var num in data[kvp.Key][0])
                {
                    checkinsPerDay += num;
                }
                myChartData.Add(new KeyValuePair<string, int>(kvp.Key, checkinsPerDay));
            }

            myChart.DataContext = myChartData;
            myColumns.Title = "# of Checkins";
            
        }

        // Function to populate the zipcode chart
        public void ZipChart(string city)
        {

            string query = String.Format("select right(address, 5) as ZipCode, count(bid) as NumBusinesses" +
                " from yelp_business where city = '{0}'" +
                    "group by right(address, 5) order by right(address, 5)", city);
            var data = DB.RunQuery(query);
            List<KeyValuePair<string, int>> myChartData = new List<KeyValuePair<string, int>>();

            for (int i = 0; i < data["zipcode"].Count; i++)
            {
                myChartData.Add(new KeyValuePair<string, int>(data["zipcode"][i].ToString(), Convert.ToInt32(data["numbusinesses"][i].ToString())));

            }

            myChart.DataContext = myChartData;
            myColumns.Title = "# of businesses per Zipcode";
        }
    }
}
