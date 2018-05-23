using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMS.Entities;
using System.Data.SqlClient;

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
                    SqlCommand cmd = new SqlCommand ( sqlQuery );
                    //cmd.Connection = conn;
                    SqlParameter parm = new SqlParameter ();
                    parm.ParameterName = "name";
                    parm.SqlDbType = System.Data.SqlDbType.VarChar;
                    parm.Value = dto.Name;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "price";
                    parm.SqlDbType = System.Data.SqlDbType.Float;
                    parm.Value = dto.Price;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "picname";
                    parm.SqlDbType = System.Data.SqlDbType.VarChar;
                    parm.Value = dto.PictureName;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "modifiedon";
                    parm.SqlDbType = System.Data.SqlDbType.DateTime;
                    parm.Value = dto.ModifiedOn;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "modifiedby";
                    parm.SqlDbType = System.Data.SqlDbType.Int;
                    parm.Value = dto.ModifiedBy;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "pid";
                    parm.SqlDbType = System.Data.SqlDbType.Int;
                    parm.Value = dto.ProductID;
                    cmd.Parameters.Add ( parm );

                   int a= helper.ExecuteQueryParm ( cmd );
                    return dto.ProductID;
                }
                else
                {
                    //sqlQuery = String.Format("INSERT INTO dbo.Products(Name, Price, PictureName, CreatedOn, CreatedBy,IsActive) VALUES('{0}','{1}','{2}','{3}','{4}',{5}); Select @@IDENTITY",
                    //dto.Name, dto.Price, dto.PictureName, dto.CreatedOn, dto.CreatedBy, 1);

                    sqlQuery = String.Format ( @"INSERT INTO dbo.Products(Name, Price, PictureName, CreatedOn, CreatedBy,IsActive) VALUES
                        (@name,@price,@picname,@createdon,@createdby,@isactive); Select @@IDENTITY");
                  //  String _connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
                    //SqlConnection conn = new SqlConnection ( _connStr );
                    //conn.Open ();
                   
                    SqlCommand cmd = new SqlCommand ( sqlQuery );
                    //cmd.Connection = conn;
                    SqlParameter parm = new SqlParameter ();
                    parm.ParameterName = "name";
                    parm.SqlDbType = System.Data.SqlDbType.VarChar;
                    parm.Value = dto.Name;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "price";
                    parm.SqlDbType = System.Data.SqlDbType.Float;
                    parm.Value = dto.Price;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "picname";
                    parm.SqlDbType = System.Data.SqlDbType.VarChar;
                    parm.Value = dto.PictureName;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "createdon";
                    parm.SqlDbType = System.Data.SqlDbType.DateTime;
                    parm.Value = dto.CreatedOn;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "createdby";
                    parm.SqlDbType = System.Data.SqlDbType.Int;
                    parm.Value = dto.CreatedBy;
                    cmd.Parameters.Add ( parm );

                    parm = new SqlParameter ();
                    parm.ParameterName = "isactive";
                    parm.SqlDbType = System.Data.SqlDbType.Bit;
                    parm.Value = 1;
                    cmd.Parameters.Add ( parm );

                    //int a = Convert.ToInt32 ( cmd.ExecuteScalar () );
                   Object obj= helper.ExecuteScalarParm ( cmd );
                    int a= Convert.ToInt32(obj);
                    return a;
                }
            }
        }
        public static ProductDTO GetProductById(int pid)
        {
            var query = String.Format("Select * from dbo.Products Where ProductId={0}", pid);

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
            var query = "Select dbo.Products.ProductID, dbo.Products.Name, dbo.Products.Price, dbo.Products.PictureName, CreatedOn,CreatedBy, ModifiedOn, ModifiedBy, dbo.Products.IsActive,dbo.Users.Name from dbo.Products,dbo.Users Where dbo.Products.IsActive = 1 AND CreatedBy=dbo.Users.UserID;";

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
            String sqlQuery = String.Format(@"Update dbo.Products Set IsActive=0 Where ProductID={0}", pid);

            using (DBHelper helper = new DBHelper())
            {
                return helper.ExecuteQuery(sqlQuery);
            }
        }

        private static ProductDTO FillDTO(SqlDataReader reader)
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
