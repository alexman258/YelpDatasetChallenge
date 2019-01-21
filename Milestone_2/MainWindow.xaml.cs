using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;
using System.Reflection;
using System.Collections;
using System.IO;

namespace Milestone_2
{

    // Note to self: Work on NOT keeping everything in the MainWindow class
    //               Add and remove needs some work
    //               Start working on time filters and Add Tip
    //               Build GUI for Login

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AddStates();

            AddColumnToGrid(cityDataGrid, "City", "city", 255);

            AddColumnToGrid(zipcodeGrid, "Zipcodes", "zipcode", 255);

            AddColumnToGrid(searchResultsDataGrid, "Business Name", "name", 150);
            AddColumnToGrid(searchResultsDataGrid, "Address", "address", 255);
            AddColumnToGrid(searchResultsDataGrid, "Rating", "stars", 100);

            AddColumnToGrid(categoriesDataGrid, "Categories", "name", 255);

            AddColumnToGrid(addRemoveDataGrid, "Categories", "name", 255);

            AddColumnToGrid(userIDGrid, "User IDs", "uid", 500);

            AddColumnToGrid(friendsDataGrid, "Name", "name", 100);
            AddColumnToGrid(friendsDataGrid, "Rating", "stars", 75);
            AddColumnToGrid(friendsDataGrid, "User ID", "uid", 200);

            AddColumnToGrid(tipsByFriendsGrid, "Name", "uid", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Business", "bid", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Tip", "text", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Date", "date", 200);

            Window1 window = new Window1();
            window.ZipChart("Phoenix");

            window.Show();
            //window.ZipChart("Tempe");

            //window.Hide();
        }

        SingletonDB database = SingletonDB.GetInstance;
        

        // Returns the information needed to access my DB
        private string BuildConnString()
        {
            return "Host = localhost; Username = postgres; Password = password; Database = YelpApp";
        }

        // AddStates() runs a query in the database that returns a table
        // of all distinct states. It then populates the combobox
        // with the result
        public void AddStates()
        {
            string query = "SELECT DISTINCT state FROM yelp_business ORDER BY STATE;";
            var data = database.RunQuery(query);

            PopulateComboBox(data, stateComboBox);
        }

        // Populating a given combo box
        public void PopulateComboBox(dynamic data, ComboBox comboBox)
        {
            if (data != null)
            {
                foreach (KeyValuePair<string, dynamic> kvp in data)
                {
                    foreach (var item in data[kvp.Key])
                    {
                        comboBox.Items.Add(item);
                    }
                }
            }
            else
            {
                // Error in case the dictionary data is empty
                throw new ArgumentException("data dictionary is empty!");
            }
        }

        // The purpose of this method is to update the cityDataGrid with new cities when 
        // the user specifies a state from the combo box
        private void stateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cityDataGrid.Items.Clear();
            zipcodeGrid.Items.Clear();
            searchResultsDataGrid.Items.Clear();
            categoriesDataGrid.Items.Clear();

            string query = "SELECT DISTINCT city FROM yelp_business WHERE state = '" + stateComboBox.SelectedItem.ToString() + "' ORDER BY city;";
            var data = database.RunQuery(query);

            foreach(var value in data.Values)
            {
                foreach(var item in value)
                {
                    Business temp = new Business();

                    foreach (KeyValuePair<string, dynamic> kvp in data)
                    {
                        temp.GetType().GetProperty(kvp.Key).SetValue(temp, item, null);

                    }
                    cityDataGrid.Items.Add(temp);
                }
            }
        }

        // AddColumnToGrid adds a column to specified DataGrid, it has 4 parameters:
        // 1. a DataGrid
        // 2. a title for that column
        // 3, and a variable to bind to
        // 4, width of the column
        public void AddColumnToGrid(DataGrid myGrid, string header, string binding, int width)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = header;
            col1.Binding = new Binding(binding);
            col1.Width = width;
            myGrid.Columns.Add(col1);
        }

        // This method is the SelectionChanged event for the cityDataGrid
        // it first clears all other grids that depend on the city chose (i.e zipcode, search results and categories)
        // and then runs a querry which returns all distinct zipcodes in this city and state
        private void cityDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            zipcodeGrid.Items.Clear();
            searchResultsDataGrid.Items.Clear();
            categoriesDataGrid.Items.Clear();

            object item = cityDataGrid.SelectedItem;
            string state = stateComboBox.SelectedItem.ToString();
            string city = ((cityDataGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text).ToString();

            string query = "SELECT DISTINCT REVERSE(substring(REVERSE(address) from 1 for 5)) as zipcode " +
                            "FROM yelp_business " +
                            "WHERE state = '" + state + "' " +
                            "AND city = '" + city + "' " +
                            "ORDER BY zipcode;";

            var data = database.RunQuery(query);

            foreach (var value in data.Values)
            {
                foreach (var val in value)
                {
                    Business temp = new Business();

                    foreach (KeyValuePair<string, dynamic> kvp in data)
                    {
                        temp.GetType().GetProperty(kvp.Key).SetValue(temp, val, null);

                    }
                    zipcodeGrid.Items.Add(temp);
                }
            }
        }

        // Creates a list of a specific type needed
        public IList CreateList(Type type)
        {
            Type genericList = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericList);
        }

        // Populating a specific dataGrid
        private void PopulateDataGrid(DataGrid dataGrid, dynamic data, Type type)
        {
            IList objs = CreateList(type);
            dynamic element;

            int i = 0;
            foreach (var kvp in data)
            {
                int j = 0;
                foreach (var listItem in data[kvp.Key])
                {
                    if (IsArray(listItem))
                    {
                        element = ParseTextToArray(listItem, kvp.Key.ToString());
                        int g = 0;
                    }
                    else
                    {
                        string returnType = data[kvp.Key].GetType().GetGenericArguments()[0].ToString();
                        element = database.ConvertToSpecificType(returnType, listItem.ToString());
                    }

                    if (objs.Count < data[kvp.Key].Count)
                    {
                        var temp = Activator.CreateInstance(type);
                        temp.GetType().GetProperty(kvp.Key).SetValue(temp, element, null);
                        objs.Add(temp);
                        i++;
                    }
                    else if (j < i)
                    {
                        objs[j].GetType().GetProperty(kvp.Key).SetValue(objs[j], element, null);
                        j++;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid information from the database");
                    }
                }
            }

            // Populate the actual grid
            foreach (var item in objs)
            {
                dataGrid.Items.Add(item);
            }
        }

        private bool IsArray(dynamic item)
        {
            string text = item.ToString();

            if (text.EndsWith("}") && text.StartsWith("{"))
            {
                return true;
            }

            return false;
        }

        private dynamic ParseTextToArray(dynamic item, string varName)
        {
            string text = item.ToString();

            int counter = 0;
            foreach (char c in text)
            {
                if (c == '{')
                {
                    counter++;
                }

                if (c == '}')
                {
                    break;
                }
            }

            if (counter > 1)
            {
                return GetAvailability(item.ToString());
            }
            else
            {
                if (varName == "hours")
                {
                    return new string[7, 2];
                }

                return text.TrimEnd('}').TrimStart('{').Split(',').ToArray();
            }

        }

        // This method is the event handler for the SelectionChanged on the zipcode data grid.
        // It first clears all datagrids that depend on zipcode,
        // then it runs a querry which returns all categories of business in that state, city and zipcode.
        // Finally using a stringbuilder and stringbuilder methods, the categories data grid
        // gets filled in order from a-z
        private void zipcodeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searchResultsDataGrid.Items.Clear();
            categoriesDataGrid.Items.Clear();

            object item = cityDataGrid.SelectedItem;
            object item2 = zipcodeGrid.SelectedItem;
            string state = stateComboBox.SelectedItem.ToString();
            string city = ((cityDataGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text).ToString();
            string zipcode = ((zipcodeGrid.SelectedCells[0].Column.GetCellContent(item2) as TextBlock).Text).ToString();

            //Dont forget to add hoursopen
            string query = "SELECT name, address, stars, categories FROM yelp_business WHERE state = '"
                                + state + "' AND city = '" + city + "' AND REVERSE(substring(REVERSE(address) from 1 for 5)) = '"
                                + zipcode + "' ORDER BY name;";

            var data = database.RunQuery(query);
            PopulateDataGrid(searchResultsDataGrid, data, typeof(Business));

            List<string> temp = new List<string>();

            foreach (Business business in searchResultsDataGrid.Items)
            {
                foreach (string category in business.categories)
                {
                    if (category[0] == ' ')
                    {
                        temp.Add(category.Substring(1));
                        continue;
                    }

                    temp.Add(category);
                }
            }

            temp.Sort();
            var distinctSorted = temp.Select(x => x).Distinct().ToList();

            foreach (var categories in distinctSorted)
            {
                categoriesDataGrid.Items.Add(new Category() { name = categories });
            }

            PopulateTimeFilter();



            int gfg = 69;

            /*
            foreach (var value in data.Values)
            {
                foreach (var val in value)
                {
                    Business temp = new Business();

                    foreach (KeyValuePair<string, dynamic> kvp in data)
                    {
                        dynamic list = data[kvp.Key];
                        string returnType = list.GetType().GetGenericArguments()[0].ToString();

                        temp.GetType().GetProperty(kvp.Key).SetValue(temp, database.ConvertToSpecificType(returnType, val), null);

                    }
                    searchResultsDataGrid.Items.Add(temp);
                }
            }
            */



            int i = 0;


            /*

            if (zipcodeGrid.SelectedItem != null)
            {
                object item = cityDataGrid.SelectedItem;
                object item2 = zipcodeGrid.SelectedItem;
                string state = stateComboBox.SelectedItem.ToString();
                string city = ((cityDataGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text).ToString();
                string zipcode = ((zipcodeGrid.SelectedCells[0].Column.GetCellContent(item2) as TextBlock).Text).ToString();

                StringBuilder sb = new StringBuilder();

                using (var connection = new NpgsqlConnection(BuildConnString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT categories FROM yelp_business WHERE " +
                            "state = '" + state + "' AND " +
                            "city = '" + city + "' AND " +
                            "REVERSE(substring(REVERSE(address) from 1 for 5)) = '" + zipcode + "';";

                        // appends the stringbuilder with the categories that postgres returns
                        using (var reader1 = cmd.ExecuteReader())
                        {
                            while (reader1.Read())
                            {
                                sb.Append(reader1.GetString(0));
                            }
                        }

                        // This section trims the string we got, and put all unique categories in an array
                        sb.Replace("{", "");
                        sb.Replace("}", "");
                        string[] temp = (sb.ToString()).Split(',');
                        string[] text = temp.Distinct().ToArray();
                        Array.Sort(text,
                            StringComparer.InvariantCulture);

                        // Adding the categories to the categories data grid
                        foreach (string s in text)
                        {
                            categoriesDataGrid.Items.Add(new Business() { categories = s});
                        }  

                        cmd.CommandText = "SELECT name, address, stars, hoursopen FROM yelp_business WHERE state = '"
                                + state + "' AND city = '" + city + "' AND REVERSE(substring(REVERSE(address) from 1 for 5)) = '"
                                + zipcode + "' ORDER BY name;";

                        // Filling the searchResultsDataGrid with Business names, address, and ratings
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string[,] availability = GetAvailability(reader.GetString(3));

                                searchResultsDataGrid.Items.Add(new Business() { hours = availability, businessName = reader.GetString(0),
                                    fullAddress = reader.GetString(1), stars = reader.GetDouble(2)});

                            }
                        }
                    }
                    connection.Close();
                }


            }

            */
        }

        public string[,] GetAvailability(string times)
        {
            string[,] availability = new string[7, 2];

            times = times.Replace("{", "").Replace("}", "").Replace(" ", "");

            string[] temp;

            if (times != null && times != "")
            {
                temp = times.Split(',');

                for (int i = 0; i < temp.Count(); i += 3)
                {
                    switch (temp[i])
                    {
                        case "Monday":
                            availability[0, 0] = temp[i + 1];
                            availability[0, 1] = temp[i + 2];
                            break;
                        case "Tuesday":
                            availability[1, 0] = temp[i + 1];
                            availability[1, 1] = temp[i + 2];
                            break;
                        case "Wednesday":
                            availability[2, 0] = temp[i + 1];
                            availability[2, 1] = temp[i + 2];
                            break;
                        case "Thursday":
                            availability[3, 0] = temp[i + 1];
                            availability[3, 1] = temp[i + 2];
                            break;
                        case "Friday":
                            availability[4, 0] = temp[i + 1];
                            availability[4, 1] = temp[i + 2];
                            break;
                        case "Saturday":
                            availability[5, 0] = temp[i + 1];
                            availability[5, 1] = temp[i + 2];
                            break;
                        case "Sunday":
                            availability[6, 0] = temp[i + 1];
                            availability[6, 1] = temp[i + 2];
                            break;
                    }
                }
            }

            return availability;
        }

        // This is the button click handler for the "add" button which essentially just
        // adds the selected category from the categories data grid to the the grid right below it
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesDataGrid.SelectedIndex > -1)
            {
                var itemToMove = categoriesDataGrid.SelectedItem;
                categoriesDataGrid.Items.RemoveAt(categoriesDataGrid.SelectedIndex);
                addRemoveDataGrid.Items.Add(itemToMove);

            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (addRemoveDataGrid.SelectedIndex > -1)
            {
                var itemToMove = addRemoveDataGrid.SelectedItem;
                categoriesDataGrid.Items.RemoveAt(categoriesDataGrid.SelectedIndex);
                addRemoveDataGrid.Items.Add(itemToMove);
            }
        }



        // FindUserIDs opens a connection in the PGadmin DB.
        // It then runs a querry that returns all the user IDs whos names
        // are the same as the name that was entered in the searchTextbox
        private void FindUserIDs(string name)
        {
            string query = "SELECT uid FROM yelp_user WHERE name = '" + name + "'";
            var data = database.RunQuery(query);
            PopulateDataGrid(userIDGrid, data, typeof(User));


            /* Code above works, keeping this just in case
             * 
            using (var connection = new NpgsqlConnection(BuildConnString()))
            {
                userIDGrid.Items.Clear();
                if (userIDGrid.SelectedIndex >= -1)
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT uid FROM yelp_user WHERE uname = '" + name + "'";
                        using (var reader = cmd.ExecuteReader())
                        {
                            // Add all the user IDs until the reader is done reading
                            while (reader.Read())
                            {
                                userIDGrid.Items.Add(new User() { uid = reader.GetString(0) });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            */
        }

        // PopulateUser takes in a user's ID and runs a querry in PGadmin
        // that returns all the information of that user. It then fills all the User information textboxes
        // with certain the information retrieved from the DB
        void PopulateUserInformation(string ID)
        {
            string query = "SELECT * FROM yelp_user WHERE uid = '" + ID + "'";
            var data = database.RunQuery(query);

            nameTextbox.Text = data["name"][0].ToString();
            starsTextbox.Text = data["stars"][0].ToString();
            fansTextbox.Text = data["fans"][0].ToString();
            funnyTextbox.Text = data["funnyvotes"][0].ToString();
            coolTextbox.Text = data["coolvotes"][0].ToString();
            usefulTextbox.Text = data["usefulvotes"][0].ToString();
            yelpingSinceTextbox.Text = data["yelpingsince"][0].ToString();

            string[] friendsList = data["friends"][0];

            PopulateFriends(ID, friendsList);
        }

        // PopulateFriends populates the friends list of the current selected 
        // user, if they don't have an friends on their account the friends list will remain blank.    
        //
        // It also populates the tipsByFriendsDataGrid ordering it by the most current date
        private void PopulateFriends(string userID, string[] friendsList)
        {
            List<User> friends = new List<User>();
            string query = "SELECT name, stars, uid FROM yelp_user WHERE uid = '" + userID + "'";

            foreach (string s in friendsList)
            {
                query += "OR uid = '" + s + "'";
            }

            var data = database.RunQuery(query);
            IList objs = CreateList(typeof(User));

            for (int i = 0; i < friendsList.Count(); i++)
            {
                User user = new User();
                foreach (var kvp in data)
                {
                    var item = kvp.Value[i];
                    user.GetType().GetProperty(kvp.Key).SetValue(user, kvp.Value[i], null);
                }
                friendsDataGrid.Items.Add(user);
            }

            foreach (var friend in friendsDataGrid.Items)
            {

            }

            string query2 = "SELECT b.name, t.tip, t.date FROM yelp_business b, yelp_tip t where t.bid = b.bid AND t.uid = '" + "'";


            /*

            // Building connection with PGadmin DB
            using (var connection = new NpgsqlConnection(BuildConnString()))
            {
                // Clear friendsDataGrid
                friendsDataGrid.Items.Clear();
                // Clear tipsByFriendsDataGrid
                tipsByFriendsGrid.Items.Clear();

                // Check to make sure an item is selected
                if (userIDGrid.SelectedIndex >= -1)
                {
                    // Use string builder to get the friends and parse them
                    StringBuilder sb = new StringBuilder();

                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;

                        // Querry to PGadmin that gives all the information about a user based on ID
                        cmd.CommandText = "SELECT friends FROM yelp_user WHERE uid = '" + userID + "'";

                        // Reader readers the DB result, and enters them into the textboxes
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sb.Append(reader.GetString(0));

                                // This section trims the string we got, and put all unique categories in an array
                                sb.Replace("{", "");
                                sb.Replace("}", "");
                            }
                        }

                        // Array of strings that holds all the users friends IDs
                        string[] temp = (sb.ToString()).Split(',');

                        // Check to make sure friends list is not empy
                        if (temp[0] != "")
                        {
                            List<Tip> tip = new List<Tip>();

                            // Loop and add each friends name, rating, and ID to the friendsdataGrid
                            foreach (string s in temp)
                            {

                                // Create hash index on user



                                // Query that returns the name, rating, and the ID of a specific user
                                cmd.CommandText = "SELECT uname, avgstars, uid FROM yelp_user where uid = '" + s + "';";
                                using (var reader2 = cmd.ExecuteReader())
                                {
                                    reader2.Read();

                                    // Add friend to the friendDataGrid
                                    friendsDataGrid.Items.Add(new User()
                                    {
                                        name = reader2.GetString(0),
                                        stars = reader2.GetDouble(1),
                                        uid = reader2.GetString(2)
                                    });
                                    
                                    // Call PopulateTipsByFriends so it can get a list of all the tips
                                    // the friends provided, and sort it by the most recent date
                                    PopulateTipsByFriends(reader2.GetString(0), s, tip);
                                }
                            }

                            // new Tip list that is ordered descending by the date, latest to oldest
                            List<Tip> orderedList = tip.OrderByDescending(o => o.date).ToList();

                            // Loop through the orderedList and add it to the tipsByFriendsGrid
                            foreach (object o in orderedList)
                            {
                                tipsByFriendsGrid.Items.Add(o);
                            }
                        }
                    }
                    connection.Close();
                }
            }

    */
        }

        // The purpose of this method is to fill a List of Tip object with all
        // the tips a user's friends have left. This is because later we will sort
        // the list
        private void PopulateTipsByFriends(string name, string ID, List<Tip> tip)
        {           
            using (var connection = new NpgsqlConnection(BuildConnString()))
            {
                // Check if a user is selected
                if (userIDGrid.SelectedIndex >= -1)
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;

                        // Querry to PGadmin that returns business name, tip and the date given by a user
                        cmd.CommandText = "SELECT name, tip, date FROM yelp_tip natural join yelp_business WHERE uid = '" + ID + "';";

                        // Reader readers the DB result, and enters them into the textboxes
                        using (var reader = cmd.ExecuteReader())
                        {
                            // While there is something to read, the reader
                            // adds a new Tip object to the list
                            while (reader.Read())
                            {
                                tip.Add(new Tip {
                                    uid = name,
                                    bid = reader.GetString(0),
                                    text = reader.GetString(1),
                                    date = reader.GetString(2)
                                });
                            }
                        }                       
                    }
                    connection.Close();
                }
            }
        }

        // Button click event for the search button
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            FindUserIDs(searchTextbox.Text);
        }



        // This method is the SelectionChanged event for userIDGrid DataGrid
        // that checks if an item from userIDGrid has been selected, if it has, it calls the PopulateUser method
        // passing the ID of the user as a arguement
        private void userIDGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userIDGrid.SelectedItem != null)
            {
                object item = userIDGrid.SelectedItem;             
                string ID = ((userIDGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text).ToString();

                PopulateUserInformation(ID);
            }
        }

        public void PopulateTimeFilter()
        {
            dayOfWeekCombo.Items.Add("Monday");
            dayOfWeekCombo.Items.Add("Tuesday");
            dayOfWeekCombo.Items.Add("Wednesday");
            dayOfWeekCombo.Items.Add("Thursday");
            dayOfWeekCombo.Items.Add("Friday");
            dayOfWeekCombo.Items.Add("Saturday");
            dayOfWeekCombo.Items.Add("Sunday");

            
            // Adding AM's
            for (int i = 0; i < 24; i++)
            {
                if (i < 10)
                {
                    FromCombo.Items.Add("0" + i.ToString() + ":00");
                    toCombo.Items.Add("0" + i.ToString() + ":00");
                }
                else
                {
                    FromCombo.Items.Add(i.ToString() + ":00");
                    toCombo.Items.Add(i.ToString() + ":00");
                }
            }
        }

        public void PopulateFilterBox()
        {
            if (zipcodeGrid.SelectedItem != null)
            {
                List<string> results = new List<string>();

                using (var connection = new NpgsqlConnection(BuildConnString()))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        string query = "Select hoursopen from yelp_business where state = 'AZ' AND city = 'Phoenix' AND REVERSE(substring(REVERSE(address) from 1 for 5)) = '85003';";
                        cmd.Connection = connection;
                        cmd.CommandText = query;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                // Got list of strings of the double array {{},{},{}}

                string test = results.ElementAt(1);
                test = test.Replace("{", "").Replace("}","").Replace(" ", "");

                string[] daysOfWeek;

                if (test != null && test != "")
                {
                    daysOfWeek = test.Split(',');
                }


            }
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (dayOfWeekCombo.SelectedIndex > -1 && FromCombo.SelectedIndex > -1 && toCombo.SelectedIndex > -1)
            {
                if (toCombo.SelectedIndex > FromCombo.SelectedIndex)
                {
                    PopulateFilterBox();

                    int day = dayOfWeekCombo.SelectedIndex;
                    string open = FromCombo.SelectedItem.ToString();
                    string close = toCombo.SelectedItem.ToString();

                    var tempGrid = new DataGrid();

                    int counter = 0;

                    foreach (var item in searchResultsDataGrid.Items)
                    {
                        var business = item as Business;
                        var hours = business.hours;
                        var name = business.name;

                        string openTime = hours[day, 0];
                        string closeTime = hours[day, 1];

                        if (openTime != null)
                        {
                            bool one = openTime.CompareTo(open) < 0;
                            bool two = closeTime.CompareTo(close) > 0;

                            bool three = openTime.Equals(open);
                            bool four = closeTime.Equals(close);

                            bool five = openTime.CompareTo(open) < 0;
                            bool six = closeTime.Equals(close);

                            bool seven = openTime.Equals(open);
                            bool eight = closeTime.CompareTo(close) > 0;
                        }

                        if ((hours[day,0] != null) &&
                                (
                                    (hours[day, 0].CompareTo(open) < 0 && hours[day, 1].CompareTo(close) > 0) ||
                                    (hours[day, 0].Equals(open) && hours[day, 1].Equals(close)) ||
                                    (hours[day, 0].CompareTo(open) < 0 && hours[day, 1].Equals(close)) ||
                                    (hours[day, 0].Equals(open) && hours[day, 1].CompareTo(close) < 0)                           
                                )
                            )
                        {
                            tempGrid.Items.Add(item);
                        }
                        counter++;

                    }

                    int count = tempGrid.Items.Count;

                    searchResultsDataGrid.Items.Clear();
                    //searchResultsDataGrid = tempGrid;

                    foreach (var item in tempGrid.Items)
                    {
                        searchResultsDataGrid.Items.Add(item);
                    }

                   // searchResultsDataGrid.Items.RemoveAt(0);
                }
            }
            else
            {
                // Opens window that says something like "Invalid filter requests"
                invalidWindow.Visibility = Visibility.Visible;
            }
        }


        private void addRemoveDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Needs work! When adding new category it doesn't remove it from the previous dataGrid
            // and it doesn't sort the categories alphabetically. Also need remove button working
        }

        private void categoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void searchResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }
}
