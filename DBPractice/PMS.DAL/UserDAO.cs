using MySql.Data.MySqlClient;
using PMS.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.DAL
{
    public static class UserDAO
    {
        public static int Save(UserDTO dto)
        {
            String sqlQuery = "";
            if (dto.UserID > 0)
            {
                sqlQuery = String.Format( "Update Users Set Name='{0}', PictureName='{1}', email='{2}' Where UserID={3} ;select @@IDENTITY",
                    dto.Name, dto.PictureName, dto.Email,dto.UserID);
                using (DBHelper helper = new DBHelper ())
                {
                     int rowsAffected=helper.ExecuteQuery ( sqlQuery );
                    if (rowsAffected > 0)
                        return -1;
                    else
                        return -2;
                }

            }
            else
            {
                sqlQuery = String.Format("INSERT INTO Users(Name, Login,Password, PictureName, IsAdmin,IsActive,email) VALUES('{0}','{1}','{2}','{3}',{4},{5},'{6}');select @@IDENTITY",
                dto.Name, dto.Login, dto.Password, dto.PictureName, 0,1,dto.Email);
                using (DBHelper helper = new DBHelper ())
                {
                    return Convert.ToInt32 ( helper.ExecuteScalar ( sqlQuery ) );
                }
            }

            
        }

        public static int UpdatePassword(UserDTO dto)
        {
            String sqlQuery = "";
            sqlQuery = String.Format("Update Users Set Password='{0}' Where UserID={1}", dto.Password, dto.UserID);


            using (DBHelper helper = new DBHelper())
            {
                return helper.ExecuteQuery(sqlQuery);
            }
        }

        public static UserDTO ValidateUser(String pLogin, String pPassword)
        {
            var query = String.Format(@"Select * from Users Where Login=@login and Password=@pwd");
            MySqlCommand cmd = new MySqlCommand ( query );

            
            cmd.Parameters.AddWithValue ( "login", pLogin );

            //            parm = new SqlParameter ( "pwd", System.Data.SqlDbType.VarChar );
            cmd.Parameters.AddWithValue ( "pwd", pPassword );


            using (DBHelper helper = new DBHelper())
            {
                var reader = helper.ExecuteReaderParm(cmd);

                UserDTO dto = null;

                if (reader.Read())
                {
                    dto = FillDTO(reader);
                }

                return dto;
            }
        }

        public static UserDTO GetUserById(int pid)
        {

            var query = String.Format("Select * from Users Where UserId={0}", pid);

            using (DBHelper helper = new DBHelper())
            {
                var reader = helper.ExecuteReader(query);

                UserDTO dto = null;

                if (reader.Read())
                {
                    dto = FillDTO(reader);
                }

                return dto;
            }
        }

        public static List<UserDTO> GetAllUsers()
        {
            using (DBHelper helper = new DBHelper())
            {
                var query = "Select * from dbo.Users Where IsActive = 1;";
                var reader = helper.ExecuteReader(query);
                List<UserDTO> list = new List<UserDTO>();

                while (reader.Read())
                {
                    var dto = FillDTO(reader);
                    if (dto != null)
                    {
                        list.Add(dto);
                    }
                }

                return list;
            }
        }

        public static int DeleteUser(int pid)
        {
            String sqlQuery = String.Format("Update Users Set IsActive=0 Where UserID={0}", pid);

            using (DBHelper helper = new DBHelper())
            {
                return helper.ExecuteQuery(sqlQuery);
            }
        }

        private static UserDTO FillDTO(MySqlDataReader reader)
        {
            var dto = new UserDTO();
            dto.UserID = reader.GetInt32(0);
            dto.Name = reader.GetString(1);
            dto.Login = reader.GetString(2);
            dto.Password = reader.GetString(3);
            dto.PictureName = reader.GetString(4);
            dto.IsAdmin = reader.GetBoolean(5);
            dto.IsActive = reader.GetBoolean(6);
            dto.Email = reader.GetString ( 7 );


            return dto;
        }
        ///////////////////////////////////////////////////////
        public static UserDTO getUserByLoginEmail ( String login, String email )
        {
            String conStr = @"Data Source=.\SQLEXPRESS2012;Initial Catalog=Assignment7PMS; User Id=sa;Password=zahid123";
            UserDTO usr = new UserDTO ();
            using (SqlConnection conn = new SqlConnection ( conStr ))
            {
                conn.Open ();
                String query = String.Format ( @"SELECT * FROM Users WHERE Login='{0}' AND Email='{1}'", login, email );
                try
                {
                    SqlCommand cmd = new SqlCommand ( query, conn );
                    SqlDataReader reader = cmd.ExecuteReader ();
                    reader.Read ();
                    usr.UserID = Convert.ToInt32 ( reader.GetValue ( reader.GetOrdinal ( "UserID" ) ) );
                    //usr.txtID = Convert.ToInt32(id);
                    usr.Name = Convert.ToString ( reader.GetValue ( reader.GetOrdinal ( "Name" ) ) );
                    usr.Login = Convert.ToString ( reader.GetValue ( reader.GetOrdinal ( "Login" ) ) );
                    usr.Password = Convert.ToString ( reader.GetValue ( reader.GetOrdinal ( "Password" ) ) );
                    usr.PictureName = Convert.ToString ( reader.GetValue ( reader.GetOrdinal ( "PictureName" ) ) );
                    usr.Email = Convert.ToString (reader.GetValue(reader.GetOrdinal("email")));
                }
                catch (Exception exp)
                {
                    return null;
                }

            }
            return usr;

        }

    }
}
