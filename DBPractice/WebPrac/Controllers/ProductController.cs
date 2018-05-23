using PMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebPrac.Security;

namespace WebPrac.Controllers
{
    public class ProductController : Controller
    {
        private ActionResult GetUrlToRedirect()
        {
            if (SessionManager.IsValidUser)
            {
                if (SessionManager.User.IsAdmin == false)
                {
                    TempData["Message"] = "Unauthorized Access";
                    return Redirect("~/Home/NormalUser");
                }
            }
            else
            {
                TempData["Message"] = "Unauthorized Access";
                return Redirect("~/User/Login");
            }

            return null;
        }
        public ActionResult ShowAll()
        {
            if (SessionManager.IsValidUser == false)
            {
                return Redirect("~/User/Login");
            }

            var products = PMS.BAL.ProductBO.GetAllProducts(true);
            UserDTO usr = (UserDTO)Session["user"];
            ViewBag.uid = usr.UserID;
            return View(products);
        }

        public ActionResult CommentUser(int id)
        {
            //  UserDTO user=PMS.BAL.UserBO.GetUserById ( id );
            Session["uidFromComment"]=id;
            return Redirect( Url.Content( "~/User/UserWelcome" ) );
        }

        public ActionResult New()
        {
            ActionResult redVal=null;
            if (SessionManager.IsValidUser == false)
            {
                TempData["Message"] = "Unauthorized Access";
                redVal =Redirect("~/User/Login");
            }
            if (redVal == null)
            {
                var dto = new ProductDTO();
                redVal =  View(dto);
            }
           UserDTO usr=(UserDTO) Session["user"];
            return redVal;
        }

        public ActionResult Edit(int id)
        {
            if (SessionManager.IsValidUser)
            {
                ProductDTO prod = PMS.BAL.ProductBO.GetProductById ( id );
                if (prod!=null && (SessionManager.User.IsAdmin || SessionManager.User.UserID == prod.CreatedBy))
                    return View ( "New", prod );
            }
                TempData["Message"] = "Unauthorized Access";
                return Redirect ( "~/User/Logout" );
        }


        public ActionResult Edit2(int ProductID)
        {
            var prod = PMS.BAL.ProductBO.GetProductById(ProductID);
            return View("New", prod);
        }

        public ActionResult Comment (String ProductId ,String txtComment )
        {if (!(SessionManager.IsValidUser))
            {
                TempData["Message"] = "Unauthorized Access";
                return Redirect ( Url.Content ( "~/User/Login" ) );
            }
            CommentDTO cmnt = new CommentDTO ();
            cmnt.UserID = SessionManager.User.UserID;
            cmnt.ProductID = Convert.ToInt32(ProductId);
            cmnt.CommentText = txtComment;
            cmnt.CommentOn = DateTime.Now;
            PMS.BAL.CommentBO.Save ( cmnt );
            return Redirect(Url.Content("~/Product/ShowAll"));
        }

        public ActionResult Delete(int id)
        {

            if (SessionManager.IsValidUser)
            {
               ProductDTO dto= PMS.BAL.ProductBO.GetProductById ( id );
                if(dto == null)
                {
                    return Redirect ( "~/Home/NormalUser" );
                }
                if (SessionManager.User.IsAdmin == true || dto.CreatedBy ==SessionManager.User.UserID)
                {
                   // PMS.BAL.ProductBO.DeleteProduct ( id );
                    TempData["Msg"] = "Record is deleted!";
                }
            }
            else
            {
                return Redirect("~/User/Login");
            }
            return RedirectToAction ( "ShowAll" );
        }
        [HttpPost]
        public ActionResult Save(ProductDTO dto)
        {

            if (!SessionManager.IsValidUser)
            {
                TempData["Message"] = "Unauthorized Access";
                return Redirect ( "~/User/Login" );
            }
            
            var uniqueName = "";

            if (Request.Files["Image"] != null)
            {
                var file = Request.Files["Image"];
                if (file.FileName != "")
                {
                    var ext = System.IO.Path.GetExtension(file.FileName);

                    //Generate a unique name using Guid
                    uniqueName = Guid.NewGuid().ToString() + ext;

                    //Get physical path of our folder where we want to save images
                    var rootPath = Server.MapPath("~/UploadedFiles");

                    var fileSavePath = System.IO.Path.Combine(rootPath, uniqueName);

                    // Save the uploaded file to "UploadedFiles" folder
                    file.SaveAs(fileSavePath);

                    dto.PictureName = uniqueName;
                }
            }



            if (dto.ProductID > 0)
            {
                dto.ModifiedOn = DateTime.Now;
                dto.ModifiedBy = SessionManager.User.UserID;
            }
            else
            {
                dto.CreatedOn = DateTime.Now;
                dto.CreatedBy = SessionManager.User.UserID;
            }

            PMS.BAL.ProductBO.Save(dto);

            TempData["Msg"] = "Record is saved!";

            return RedirectToAction("ShowAll");
        }

    }
}