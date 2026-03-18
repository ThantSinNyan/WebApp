using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: show page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: handle login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string connStr = "server=localhost;user=root;password=YOURPASSWORD;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM users WHERE email=@e AND password=@p";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    ViewBag.Message = "✅ Login Successful!";
                }
                else
                {
                    ViewBag.Message = "❌ Login Failed!";
                }
            }

            return View();
        }
    }
}