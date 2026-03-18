using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string name, string phone)
        {
            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string query = "INSERT INTO users (email, password, name, phone) VALUES (@e, @p, @n, @ph)";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@ph", phone);
                cmd.ExecuteNonQuery();

                ViewBag.Message = "✅ User added successfully!";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Admin()
        {
            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";
            var users = new List<dynamic>();

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM users";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new
                    {
                        Id = reader["id"],
                        Email = reader["email"],
                        Name = reader["name"],
                        Phone = reader["phone"]
                    });
                }
            }

            return View(users);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "DELETE FROM users WHERE id=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Admin");
        }
    }
}