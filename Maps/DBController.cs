using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Maps
{
    internal class DBController
    {
        string conString;
        string username;
        string database;
        string password;
        NpgsqlConnection con;
        public DBController(string username, string database, string password)
        {
            this.username = username;
            this.database = database;
            this.password = password;
            conString = String.Format("Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    "localhost",
                    username,
                    database,
                    5432,
                    password);
            con = new NpgsqlConnection(conString);
        }


        public void openConnection()
        {
            try
            {
                con.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void closeConnection()
        {
            try
            {
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public NpgsqlDataReader? basicSelect(string table)
        {
            try
            {
                string select = $"select * from {table}";

                NpgsqlCommand cmd = new NpgsqlCommand(select, con);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                return reader;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

    }
}
