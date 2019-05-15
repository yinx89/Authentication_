using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Authentication.Models;

namespace Authentication.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            foreach (Claim claim in ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type.Equals("extension_Access") && claim.Value.Equals("False"))
                {
                    ViewBag.Error = "You don't have permissions to see the claims.";
                    ViewBag.Explain = "Contact your admin to obtain permission to access the panel.";
                    return View("Error");
                }
            }
            return View();
        }
        
        [Authorize]
        public ActionResult Upload_file()
        {
            foreach (Claim claim in ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type.Equals("extension_Access") && claim.Value.Equals("False"))
                {
                    ViewBag.Error = "You don't have permissions to upload a file.";
                    ViewBag.Explain = "Contact your admin to obtain permissions.";
                    return View("Error");
                }
            }
            return View();
        }

        // The file can be uploaded from the corresponding tab in the header. It will always be saved in ~/App_Data/data.json
        [HttpPost]
        public ActionResult Upload_file(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/App_Data/"),
                                               Path.GetFileName("data.json"));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }

        // The calculations for the "Category" tab are sought.
        [Authorize]
        public ActionResult Category()
        {
            // The values of weight and height are obtained if you have permission.
            double height_user = 0;
            double weight_user = 0;

            foreach (Claim claim in ClaimsPrincipal.Current.Claims)
            {
                    if (claim.Type.Equals("extension_Access") && claim.Value.Equals("False"))
                    {
                        ViewBag.Error = "You don't have permissions.";
                        ViewBag.Explain = "Contact your admin to obtain permission to access the panel.";
                        return View("Error");
                }
                else if (claim.Type.Equals("extension_Weight"))
                {
                    weight_user = Double.Parse(claim.Value);
                }
                else if (claim.Type.Equals("extension_Height"))
                {
                    height_user = Double.Parse(claim.Value);
                }
            }

            // You get the value of BMI
            ViewBag.bmi = (weight_user / (height_user * height_user)) * 10000;

            // Initializes values to post the counters of the categories
            double valor = 0;
            int underweight = 0;
            int normal = 0;
            int preobesity = 0;
            int obesity = 0;

            // Gets the values from the uploaded file and posts them to the corresponding counters
            string file = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/data.json");
            string Json = System.IO.File.ReadAllText(file);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ListValues personlist = ser.Deserialize<ListValues>(Json);
            foreach (var item in personlist.Values)
            {
                valor = (item.Weight / (item.Height * item.Height));
                if (valor < 18.5)
                {
                    underweight++;
                }
                else if (valor <= 24.9)
                {
                    normal++;
                }
                else if (valor <= 29.9)
                {
                    preobesity++;
                }
                else
                {
                    obesity++;
                }
            }

            // Values are passed on to the view
            ViewBag.underweight = underweight;
            ViewBag.normal = normal;
            ViewBag.preobesity = preobesity;
            ViewBag.obesity = obesity;

            // Through the user's data, it determines which category corresponds to the user
            int category;
            if (ViewBag.bmi < 18.5)
            {
                category = underweight;
            }
            else if (ViewBag.bmi <= 24.9)
            {
                category = normal;
            }
            else if (ViewBag.bmi <= 29.9)
            {
                category = preobesity;
            }
            else
            {
                category = obesity;
            }

            // Once it is known which category corresponds to the user, it is counted in the corresponding
            // category and the percentile is found with respect to the counter of people that obtains each category.

            // IMPORTANT: The percentile is calculated on this sample of category counters as simple data, it could 
            // also have been calculated by obtaining a list with the calculated values of each user's BMI and 
            // obtaining a more precise value. Since, for 1-2 million entries I think that is not adequate, this 
            // could have been solved if instead of loading a file, that file was used to update data through the 
            // Entity Framework.
            var percentile = 0;
            var values = new int[] { underweight, normal, preobesity, obesity };
            var n = values.Length;

            foreach (var i in values)
            {
                var pr = ((values.Count(v => v < i) + (.5 * values.Count(v => v == i))) / n);
                if (pr < category)
                    percentile += 1;
            }

            ViewBag.percentile = percentile;

            return View();
        }
        public ActionResult Error(string message)
        {
            ViewBag.Error = "An error occurred while processing your request.";
            return View("Error");
        }
    }
}