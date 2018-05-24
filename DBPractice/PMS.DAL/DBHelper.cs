using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.DAL
{
    internal class DBHelper : IDisposable
    {
        //String _connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
       // String _connStr = "server=localhost;port=3306;Uid=sa;Pwd=zahid123; database=Assignment7PMS;";
        String _connStr = @"Server=localhost;port=3306;Uid=root;Pwd=;Database=Assignment7PMS;Convert Zero Datetime = true";
        MySqlConnection _conn = null;
        public DBHelper ()
        {
            _conn = new MySqlConnection ( _connStr );
            _conn.Open ();
        }
        public DBHelper getConn ()
        {
            return new DBHelper ();
        }
        public int ExecuteQuery ( String sqlQuery )
        {
            MySqlCommand command = new MySqlCommand ( sqlQuery, _conn );
            var count = command.ExecuteNonQuery ();
            return count;
        }

        public int ExecuteQueryParm ( MySqlCommand command )
        {
            command.Connection = _conn;
            var count = command.ExecuteNonQuery ();
            return count;
        }
        public Object ExecuteScalar ( String sqlQuery )
        {
            MySqlCommand command = new MySqlCommand ( sqlQuery, _conn );
            return command.ExecuteScalar ();
        }

        public Object ExecuteScalarParm ( MySqlCommand cmd )
        {
            // SqlCommand command = new SqlCommand ( sqlQuery, _conn );
            cmd.Connection = _conn;
            return cmd.ExecuteScalar ();
        }


        public MySqlDataReader ExecuteReader ( String sqlQuery )
        {
            MySqlCommand command = new MySqlCommand ( sqlQuery, _conn );
            return command.ExecuteReader ();
        }

        public MySqlDataReader ExecuteReaderParm ( MySqlCommand command )
        {
            command.Connection = _conn;
            return command.ExecuteReader ();
        }

        public void Dispose ()
        {
            if (_conn != null && _conn.State == System.Data.ConnectionState.Open)
                _conn.Close ();
        }
    }
}
