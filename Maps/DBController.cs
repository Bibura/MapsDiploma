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

        public List<string>? basicSelect(string table)
        {
            try
            {
                string select = $"select id from {table}";

                List<string> result = new List<string>();

                NpgsqlCommand cmd = new NpgsqlCommand(select, con);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                int i = 0;

                while (reader.Read())
                {
                    result.Add(reader[0].ToString());
                    i++;
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public List<string>? searchSelect(string table)
        {
            try
            {
                string select = $"select search from {table}";

                List<string> result = new List<string>();

                NpgsqlCommand cmd = new NpgsqlCommand(select, con);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                int i = 0;

                while (reader.Read())
                {
                    result.Add(reader[0].ToString());
                    i++;
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }


        public void basicInsertIntoSearchTable(string table, List<string> values)
        {
            try
            {
                string insert = $"insert into {table} values ({values[0]},{values[1]},{values[2]})";

                NpgsqlCommand cmd = new NpgsqlCommand(insert, con);

                NpgsqlDataReader reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
