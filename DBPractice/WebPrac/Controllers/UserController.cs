using PMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        //public ActionResult Logout ()
        //{
        //    Session["LoginUsrID"] = null;
        //    Session["NewUserName"] = null;
        //    Session["adminID"] = null;
        //    Session["PictureName"] = null;            
        //    Session["user"] = null;
        //    Session["editUsrPicName"] = null;
        //    return Redirect ( "~/Home/Index" );
        //}

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
            if (Session["user"] == null)
                return Redirect ( "~/Home/Index" );
            else
            {
                int id = Convert.ToInt32(Session["uidFromComment"]);
                UserDTO usr = PMS.BAL.UserBO.GetUserById ( id );
                if (usr == null) 
                    usr = new UserDTO ();
                return View ( usr );
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            SessionManager.ClearSession();
            return Redirect("/Home/Index");
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
        }//end of valdateUser()
         //////////////////////////////////////////////////////
        [HttpPost]
        public ActionResult ResetPassword ( String Login, String Email )
        {
            Random random = new Random ();
            //   int num = random.Next ( 1000, 9999 );
            int num = 1234;
            String body = num.ToString ();
            Session["code"] = body;
            UserDTO usr = PMS.BAL.UserBO.getUserByLoginEmail ( Login, Email );
            if (usr == null)
            {
                ViewBag.msg2 = "Record Not Found Against this Email and Login!";
                return View ( "UserLogin" );
            }
            Session["PasswdUpdateId"] = usr.UserID;
            if (usr.Email.Equals ( Email ))
            {
                Boolean rv = SendEmail ( usr.Email, "Mail To Reset Password", body );
                if (rv)
                    return View ();

            }
            return View ();
        }//End of ResetPassword

        public static Boolean SendEmail ( String toEmailAddress, String subject, String body )
        {
            try
            {
                String fromDisplayEmail = "ead.csf15@gmail.com";
                String fromPassword = "EAD_csf15m";
                String fromDisplayName = "GUA Admin";
                MailAddress fromAddress = new MailAddress ( fromDisplayEmail, fromDisplayName );

                MailAddress toAddress = new MailAddress ( toEmailAddress );
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential ( fromAddress.Address, fromPassword )
                };

                using (var message = new MailMessage ( fromAddress, toAddress )
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send ( message );
                }
                return true;
            }
            catch (Exception exp)
            {

                return false;
            }
        }

        public ActionResult verifyResetCode ()
        {
            String enteredCode = Request["txtCode"];
            String sendCode = Session["code"].ToString ();

            if (enteredCode.Equals ( sendCode ))
            {
                return View ( "NewPassword" );
            }
            return View ( "ResetPassword" );
        }

        public ActionResult UpdatePassword ( String txtNewPassword )
        {
            int usrID = Convert.ToInt32(Session["PasswdUpdateId"]);
            UserDTO usr = PMS.BAL.UserBO.GetUserById ( usrID );
            Session["PasswdUpdateId"] = null;
            usr.Password = txtNewPassword;
            PMS.BAL.UserBO.UpdatePassword ( usr ); //It will Update User.
            Session["code"] = null;
            ViewBag.msg = "PassWord is Updated Successfully";
            return View ( "UserWelcome",usr  );
        }

    }//end of class
}