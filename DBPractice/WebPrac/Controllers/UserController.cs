using PMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebPrac.Security;

namespace WebPrac.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(String login, String password)
        {
            var obj = PMS.BAL.UserBO.ValidateUser(login, password);
            if (obj != null)
            {
                Session["user"] = obj;
                if (obj.IsAdmin == true)
                    return Redirect("~/Home/Admin");
                else
                    return Redirect("~/Home/NormalUser");
            }

            ViewBag.MSG = "Invalid Login/Password";
            ViewBag.Login = login;

            return View();
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            UserDTO usr=(UserDTO) Session["user"];
            if (usr != null) //edit request
                Session["editUsrPicName"] = usr.PictureName;
            else
                usr = new UserDTO ();
            return View(usr);
        }

        [HttpPost]
        public ActionResult Save(UserDTO user)
        {
            var file = Request.Files["PictureName"];
            String uniqName = "";
            if (file.FileName != "")
            {
                var ext = System.IO.Path.GetExtension ( file.FileName );

                //Generate a unique name using Guid
                uniqName = Guid.NewGuid () + ext;
                //Getting physical Location where We have to save picture
                var rootPath = Server.MapPath ( "~/userpics" );

                var fileSavePath = System.IO.Path.Combine ( rootPath, uniqName );

                //Save the uploaded image
                file.SaveAs ( fileSavePath );
            }
            else if(Session["user"]!=null)
            {
                UserDTO usr = (UserDTO)Session["user"];
                uniqName =usr.PictureName;
            }
                
                // System.IO.File.Copy(uniqName,rootPath);
                user.PictureName = uniqName;
            
                user.IsAdmin = false;
                user.IsActive = true;
                int rv = PMS.BAL.UserBO.Save ( user ); //rv contain id of user
            user.UserID = rv;
            if (rv > 0) //When new user is saved.
            {
                Session["NewUserName"] = user.Name;
                Session["LoginUsrID"] = rv;
                TempData["msg"] = "Record is saved successfully!";
                Session["PictureName"] = user.PictureName;
                ViewBag.usr = user;
                Session["user"] = user;
                return View ( "UserWelcome", user );
            }
            else if (rv == -1)//When record is updated
            {
                TempData["msg"] = "Record is Updated successfully!";
                user.PictureName = Session["editUsrPicName"].ToString();
                return View ( "UserWelcome", user );
            }
            else
                return View ( "Error" );
           }


        public ActionResult UserWelcome()
        {
            return View ();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            SessionManager.ClearSession();
            return RedirectToAction("Login");
        }


        [HttpGet]
        public ActionResult Login2()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ValidateUser(String login, String password)
        {
            Object data = null;

            try
            {
                var url = "";
                var flag = false;

                var obj = PMS.BAL.UserBO.ValidateUser(login, password);
                if (obj != null)
                {
                    flag = true;
                    SessionManager.User = obj;

                    if (obj.IsAdmin == true)
                        url = Url.Content("~/Home/Admin");
                    else
                        url = Url.Content("~/Home/NormalUser");
                }

                data = new
                {
                    valid = flag,
                    urlToRedirect = url
                };
            }
            catch (Exception)
            {
                data = new
                {
                    valid = false,
                    urlToRedirect = ""
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
	}
}