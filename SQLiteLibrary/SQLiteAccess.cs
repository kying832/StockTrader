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
            using (SqliteConnection db = new SqliteConnection("Filename=lastPageViewed.db"))
            {
                db.Open();

                String tableCommand =
                    "CREATE TABLE IF NOT EXISTS LastPageViewed (" +
                    "rowNumber INT," +
                    "pageName VARCHAR(8) PRIMARY KEY)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }            
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
