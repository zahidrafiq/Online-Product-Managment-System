using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMS.Entities;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace PMS.DAL
{
    public static class ProductDAO
    {
        public static int Save(ProductDTO dto)
        {
            using (DBHelper helper = new DBHelper ())
            {
                String sqlQuery = "";
                if (dto.ProductID > 0)
                {
//                    sqlQuery = String.Format ( "Update dbo.Products Set Name='{0}',Price='{1}',PictureName='{2}',ModifiedOn='{3}',ModifiedBy='{4}' Where ProductID={5}",
  //                      dto.Name, dto.Price, dto.PictureName, dto.ModifiedOn, dto.ModifiedBy, dto.ProductID );

                    sqlQuery = String.Format ( @"Update dbo.Products Set Name=@name,Price=@price,PictureName=@picname,ModifiedOn=@modifiedon,ModifiedBy=@modifiedby Where ProductID=@pid");
                    MySqlCommand cmd = new MySqlCommand ( sqlQuery );
                    //cmd.Connection = conn;
                    //SqlParameter parm = new SqlParameter ();
                    cmd.Parameters.AddWithValue ( "name", dto.Name );
                    cmd.Parameters.AddWithValue ("price",dto.Price);
                    cmd.Parameters.AddWithValue ( "picname", dto.PictureName );
                    cmd.Parameters.AddWithValue ( "modifiedon", dto.ModifiedOn );
                    cmd.Parameters.AddWithValue ( "modifiedby", dto.ModifiedBy );
                    cmd.Parameters.AddWithValue ( "pid", dto.ProductID );
                   
                   int a= helper.ExecuteQueryParm ( cmd );
                    return dto.ProductID;
                }
                else
                {
                    //sqlQuery = String.Format("INSERT INTO dbo.Products(Name, Price, PictureName, CreatedOn, CreatedBy,IsActive) VALUES('{0}','{1}','{2}','{3}','{4}',{5}); Select @@IDENTITY",
                    //dto.Name, dto.Price, dto.PictureName, dto.CreatedOn, dto.CreatedBy, 1);

                    sqlQuery = String.Format ( @"INSERT INTO Products(Name, Price, PictureName, CreatedOn, CreatedBy,IsActive) VALUES
                        (@name,@price,@picname,@createdon,@createdby,@isactive); Select @@IDENTITY");
                  //  String _connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
                    //SqlConnection conn = new SqlConnection ( _connStr );
                    //conn.Open ();
                   
                    MySqlCommand cmd = new MySqlCommand ( sqlQuery );
                    //cmd.Connection = conn;
                    cmd.Parameters.AddWithValue ("name", dto.Name);
                    cmd.Parameters.AddWithValue ("price", dto.Price);
                    cmd.Parameters.AddWithValue ( "picname", dto.PictureName );
                    cmd.Parameters.AddWithValue ( "createdon", dto.CreatedOn );
                    cmd.Parameters.AddWithValue ( "createdby", dto.CreatedBy );
                    cmd.Parameters.AddWithValue ( "isactive", 1 );
                    
                    //int a = Convert.ToInt32 ( cmd.ExecuteScalar () );
                   Object obj= helper.ExecuteScalarParm ( cmd );
                    int a= Convert.ToInt32(obj);
                    return a;
                }
            }
        }
        public static ProductDTO GetProductById(int pid)
        {
            var query = String.Format("Select * from Products Where ProductId={0}", pid);

            using (DBHelper helper = new DBHelper())
            {
                var reader = helper.ExecuteReader(query);

                ProductDTO dto = null;

                if (reader.Read())
                {
                    dto = FillDTO(reader);
                }

                return dto;
            }
        }

        public static List<ProductDTO> GetAllProducts(Boolean pLoadComments=false)
        {
            var query = "Select Products.ProductID, Products.Name, Products.Price, Products.PictureName, CreatedOn,CreatedBy, ModifiedOn, ModifiedBy, Products.IsActive,Users.Name from Products,Users Where Products.IsActive = 1 AND CreatedBy=Users.UserID;";

            using (DBHelper helper = new DBHelper())
            {
                var reader = helper.ExecuteReader(query);
                List<ProductDTO> list = new List<ProductDTO>();

                while (reader.Read())
                {
                    var dto = FillDTO(reader);
                    dto.createrName = reader.GetValue ( 9 ).ToString();
                    if (dto != null)
                    {
                        list.Add(dto);
                    }
                }
                if (pLoadComments == true)
                {
                    //var commentsList = CommentDAO.GetAllComments();

                    var commentsList = CommentDAO.GetTopComments(2);

                    foreach (var prod in list)
                    {
                        List<CommentDTO> prodComments = commentsList.Where(c => c.ProductID == prod.ProductID).ToList();
                        prod.Comments = prodComments;
                    }
                }
                return list;
            }
        }

        public static int DeleteProduct(int pid)
        {
            String sqlQuery = String.Format(@"Update Products Set IsActive=0 Where ProductID={0}", pid);

            using (DBHelper helper = new DBHelper())
            {
                return helper.ExecuteQuery(sqlQuery);
            }
        }

        private static ProductDTO FillDTO(MySqlDataReader reader)
        {
            var dto = new ProductDTO();
            dto.ProductID = reader.GetInt32(0);
            dto.Name = reader.GetString(1);
            dto.Price = reader.GetDouble(2);
            dto.PictureName = reader.GetString(3);
            dto.CreatedOn = reader.GetDateTime(4);
            dto.CreatedBy = reader.GetInt32(5);
            if (reader.GetValue(6) != DBNull.Value)
                dto.ModifiedOn = reader.GetDateTime(6);
            if (reader.GetValue(7) != DBNull.Value)
                dto.ModifiedBy = reader.GetInt32(7);

            dto.IsActive = reader.GetBoolean(8);
            return dto;
        }
    }
}
