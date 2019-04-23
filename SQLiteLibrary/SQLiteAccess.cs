using System;
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
            // create the table to track which page the user last visited
            using (SqliteConnection db = new SqliteConnection("Filename=data.db"))
            {
                db.Open();

                String tableCommand =
                    "CREATE TABLE IF NOT EXISTS LastPageViewed (" +
                    "rowNumber INT," +
                    "pageName VARCHAR(8) PRIMARY KEY)";
                /*  This query string builds the user, stock and portfolio data tables.
                String buildDB = "CREATE TABLE IF NOT EXISTS account(accnt_num INT PRIMARY KEY AUTO_INCREMENT, name VARCHAR(255), email VARCHAR(255), UNIQUE(name), UNIQUE(email)); " +
                    "CREATE TABLE IF NOT EXISTS stock(stock_id INT PRIMARY KEY AUTO_INCREMENT, stock_name VARCHAR(255),  stock_price FLOAT, stock_rating INT); " +
                    "CREATE TABLE IF NOT EXISTS portfolio(user_accnt INT, stock_num INT, FOREIGN KEY (user_accnt) REFERENCES account(accnt_num), FOREIGN KEY (stock_num) REFERENCES stock(stock_id) );";
                */



                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
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

                String cmd = "INSERT INTO account VALUES (@name, @email);";
                SqliteCommand command = new SqliteCommand(cmd);
                command.Parameters.AddWithValue("@name", user);
                command.Parameters.AddWithValue("@email", email);

                command.ExecuteReader();

                db.Close();
            }
        }
        public static void AddStock(String name, float price, int rating)
        {

        }
        public static void AddStockToPortfolio()
        {

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

                SqliteCommand command = new SqliteCommand();
                command.Connection = db;

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
    }
}
