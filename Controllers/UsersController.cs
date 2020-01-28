using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC.Models;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace MVC.Controllers
{
    public class UsersController : Controller
    {
        private TestEntities db = new TestEntities();
        private static Random random;


        // this function to send sms code to mobile for confirmation mobile number
        public void SendSMS(string number)
        {
            int otpvalue = new Random().Next(100000, 999999);
            var status = "";
            try
            {
                string recipient = number;
                string APIKey = System.Configuration.ConfigurationManager.AppSettings["APIKey"].ToString();
                string message = "Your OTP number is " + otpvalue + "(Send by : Gerges Asham)";
                string encodmessage = HttpUtility.UrlEncode(message);
               
                using (var wb = new WebClient())
                {
                    byte[] response = wb.UploadValues("https://api.textlocal.in/send/", new NameValueCollection()
                        {
                        {"apikey" , APIKey},
                        {"numbers" , number},
                        {"message" , message},
                        {"sender" , "TXTLCL"}
                        });
                    string result = System.Text.Encoding.UTF8.GetString(response);
                    var jsonobject = JObject.Parse(result);
                    status = jsonobject["status"].ToString();
                    Session["SMSCode"] = otpvalue;

                }

            }
            catch(Exception e)
            {
                throw (e);

            }


        }

        // this function to send sms code to Email for confirmation Email
        public void SendMail(string To ,string name)
        {
            random = new Random();
            var s = random.Next();
            Session["CodeConfirm"] = s;

            MailMessage mc = new MailMessage(System.Configuration.ConfigurationManager.AppSettings["Email"].ToString(), To);
            mc.Subject = "Email Confirm";
            mc.Body = "<h1> Hello "+name+"</h1><br><p>Code for conformation Email is " + s + "</p>";
            mc.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Timeout = 1000000;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            NetworkCredential nc = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["Email"].ToString(), System.Configuration.ConfigurationManager.AppSettings["Password"].ToString());
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = nc;
            smtp.Send(mc);
        }

        // GET: Users
        public ActionResult Index()
        {
          
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,UserName,Address,Email,Phone")] User user)
        {

            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                SendMail(user.Email,user.UserName);
                //SendSMS(user.Phone);
                return RedirectToAction("RedirectUser");
               
            }

            return View(user);
        }
        public ActionResult RedirectUser()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RedirectUser(string code)
        {
            if (Session["CodeConfirm"].ToString()==code)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.err = "ErrorCode";
                return View();
            }
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,UserName,Address,Email,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
