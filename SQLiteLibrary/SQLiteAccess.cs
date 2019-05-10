using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;


namespace SQLiteAccessLibrary
{
    public static class SQLiteAccess
    {
        /* **********************************************************************************************
         * This method creates the database when the application first starts.
         * **********************************************************************************************/
        public static void InitializeDatabases()
        {
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                List<string> tables = new List<string>();

                // This query string builds the user, stock and portfolio data tables.
                tables.Add("CREATE TABLE IF NOT EXISTS account(accnt_num INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255), email VARCHAR(255), UNIQUE(name), UNIQUE(email)); ");
                tables.Add("CREATE TABLE IF NOT EXISTS stock(stock_id INTEGER PRIMARY KEY AUTOINCREMENT, stock_name VARCHAR(255),  stock_price FLOAT, stock_rating INT); ");
                tables.Add("CREATE TABLE IF NOT EXISTS portfolio(user_accnt INT, stock_num INT, FOREIGN KEY (user_accnt) REFERENCES account(accnt_num), FOREIGN KEY (stock_num) REFERENCES stock(stock_id) );");

                tables.Add("CREATE TABLE IF NOT EXISTS BucketListStrategy (" +
                    "strategyName VARCHAR(50) PRIMARY KEY, " +
                    "dataTimeFrame VARCHAR(15), " +
                    "futureReturnDate VARCHAR(15), " +
                    "normalizationFunction VARCHAR(15), " +
                    "similarityThreshold FLOAT(5,3))");

                tables.Add("CREATE TABLE IF NOT EXISTS BucketListStrategyTickers (" +
                    "strategyName VARCHAR(50), " +
                    "ticker VARCHAR(15), " +
                    "CONSTRAINT pk_strategyTicker PRIMARY KEY(strategyName, ticker))");
                
                //for swing
                tables.Add("CREATE TABLE IF NOT EXISTS SwingListStrategy (" +
                   "strategyName VARCHAR(50) PRIMARY KEY, " +
                   "daysToAnalyze VARCHAR(15))");

                tables.Add("CREATE TABLE IF NOT EXISTS SwingListStrategyTickers (" +
                    "strategyName VARCHAR(50), " +
                    "ticker VARCHAR(15), " +
                    "CONSTRAINT pk_strategyTicker PRIMARY KEY(strategyName, ticker))");

                SqliteCommand createTableCommand;

                foreach(var table in tables)
                {
                    createTableCommand = new SqliteCommand(table, db);
                    createTableCommand.ExecuteReader();
                }

                db.Close();
            }            
        }

        /**
         *   This method will add a new user to the DB
         */
        public static void AddUser(String user, String email)
        {
            //connect to DB in using block
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                String cmd = "INSERT INTO account (name, Email) VALUES (?, ?);";
                SqliteCommand command = new SqliteCommand(cmd);
                command.Parameters.AddWithValue("name", user);
                command.Parameters.AddWithValue("email", email);

                command.ExecuteReader();

                db.Close();
            }
        }
        public static void AddStock(String name, float price, int rating)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                String cmd = "INSERT INTO stock (stock_name, stock_price, stock_rating) VALUES (?,?,?);";
                SqliteCommand command = new SqliteCommand(cmd);
                command.Parameters.AddWithValue("stock_name", name);
                command.Parameters.AddWithValue("stock_price", price);
                command.Parameters.AddWithValue("stock_rating", -1);

                command.ExecuteReader();
                db.Close();
            }
        }
        //TODO WRITE SELECT QUERY FOR FOREIGN KEY INSERTION
        public static void AddStockToPortfolioByNames(String stock_name, String user_name)
        {
            using (SqliteConnection db = new SqliteConnection("Filename = data.db"))
            {
                db.Open();
                String cmd = "INSERT INTO portfolio VALUES (user_accnt, stock_num) VALUES" +
                    "('user_accnt', SELECT accnt_num from account WHERE name = ?)" +
                    "('stock_num', SELECT stock_id from stock WHERE stock_name = ?) ;" ;

                SqliteCommand command = new SqliteCommand(cmd);
                command.Parameters.AddWithValue("name",user_name);
                command.Parameters.AddWithValue("stock_name", stock_name);

                command.ExecuteReader();
                db.Close();
            }
        }

        public static void AddPortfolioToUser()
        {
        }

        /* **********************************************************************************************
         * Retrieve the name of the page that was last viewed.
         * **********************************************************************************************/
        public static string GetLastPageVisited()
        {
            string pageName = "";

            using (SqliteConnection db = new SqliteConnection("Filename=lastPageViewed.db"))
            {
                db.Open();

                // create the select all command
                SqliteCommand selectCommand = new SqliteCommand("SELECT pageName FROM LastPageViewed", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                if (query.HasRows)
                {
                    query.Read();
                    pageName = query.GetString(0);
                }

                db.Close();
            }

            if (pageName != "")
                return pageName;
            else
                return null;
        }

        /* **********************************************************************************************
         * Set the name of the page that was last viewed.
         * **********************************************************************************************/
        public static void SetLastViewedPage(string pageName)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=lastPageViewed.db"))
            {
                db.Open();

                SqliteCommand command = new SqliteCommand
                {
                    Connection = db
                };

                if (GetLastPageVisited() == null)
                {
                    // insert instead of update
                    command.CommandText = "INSERT INTO LastPageViewed VALUES (@rowNumber, @pageName);";
                    command.Parameters.AddWithValue("@rowNumber", 1);
                    command.Parameters.AddWithValue("@pageName", pageName.ToLower());
                }
                else
                {
                    // update instead of insert
                    command.CommandText = "UPDATE LastPageViewed SET pageName = @pageName WHERE rowNumber = 1;";
                    command.Parameters.AddWithValue("@pageName", pageName.ToLower());
                }

                command.ExecuteReader();

                db.Close();
            }
        }

        /* **********************************************************************************************
         * Determine whether or not a strategy name exists in the database
         * **********************************************************************************************/
        public static bool StrategyExists(string strategyName)
        {
            bool results = false;

            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                // create the select all command
                SqliteCommand selectCommand = new SqliteCommand("SELECT strategyName FROM BucketListStrategy WHERE strategyName = @strategyName", db);
                selectCommand.Parameters.AddWithValue("@strategyName", strategyName);

                SqliteDataReader query = selectCommand.ExecuteReader();

                if (query.HasRows)
                    results = true;

                db.Close();
            }

            return results;
        }

        /* **********************************************************************************************
         * Add a strategy to the bucket strategy table
         * **********************************************************************************************/
        public static void AddBucketStrategy(string strategyName, List<string> tickers, string dataTimeFrame, string futureReturnDate, string normalizationFunction, float similarityThreshold)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO BucketListStrategy VALUES (@strategyName, @dataTimeFrame, @futureReturnDate, @normalizationFunction, @similarityThreshold);";
                insertCommand.Parameters.AddWithValue("@strategyName", strategyName);
                insertCommand.Parameters.AddWithValue("@dataTimeFrame", dataTimeFrame);
                insertCommand.Parameters.AddWithValue("@futureReturnDate", futureReturnDate);
                insertCommand.Parameters.AddWithValue("@normalizationFunction", normalizationFunction);
                insertCommand.Parameters.AddWithValue("@similarityThreshold", similarityThreshold);

                insertCommand.ExecuteReader();

                db.Close();
            }

            foreach(var ticker in tickers)
                AddBucketStrategyTickers(strategyName, ticker);
        }

        private static void AddBucketStrategyTickers(string strategyName, string ticker)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO BucketListStrategyTickers VALUES (@strategyName, @ticker);";
                insertCommand.Parameters.AddWithValue("@strategyName", strategyName);
                insertCommand.Parameters.AddWithValue("@ticker", ticker);

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /* **********************************************************************************************
         * Add a strategy to the swing strategy table
         * **********************************************************************************************/
        public static void AddSwingStrategy(string strategyName, List<string> tickers, string daysToAnalyze)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                insertCommand.CommandText = "INSERT INTO SwingListStrategy VALUES (@strategyName, @daysToAnalyze);";
                insertCommand.Parameters.AddWithValue("@strategyName", strategyName);
                insertCommand.Parameters.AddWithValue("@daysToAnalyze", daysToAnalyze);

                insertCommand.ExecuteReader();

                db.Close();

            }
            foreach (var ticker in tickers)
                AddSwingStrategyTickers(strategyName, ticker);



        }


        private static void AddSwingStrategyTickers(string strategyName, string ticker)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                insertCommand.CommandText = "INSERT INTO SwingListStrategyTickers VALUES (@strategyName, @ticker);";
                insertCommand.Parameters.AddWithValue("@strategyName", strategyName);
                insertCommand.Parameters.AddWithValue("@ticker", ticker);

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

    }
}
