using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM users WHERE email=@e AND password=@p";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    HttpContext.Session.SetString("user", reader["email"].ToString());
                    HttpContext.Session.SetString("name", reader["name"].ToString());
                    HttpContext.Session.SetString("role", reader["role"].ToString());
                    return RedirectToAction("Admin");
                }
                else
                {
                    ViewBag.Message = "❌ Login Failed! Try again.";
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string name, string phone, string role)
        {
            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO users (email, password, name, phone, role) VALUES (@e, @p, @n, @ph, @r)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@ph", phone);
                cmd.Parameters.AddWithValue("@r", role);
                cmd.ExecuteNonQuery();

                ViewBag.Message = "✅ User added successfully!";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Admin()
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction("Login");
            }

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
                        Phone = reader["phone"],
                        Role = reader["role"]
                    });
                }
            }

            ViewBag.LoggedInUser = HttpContext.Session.GetString("name");
            ViewBag.UserRole = HttpContext.Session.GetString("role");
            return View(users);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction("Login");
            }

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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction("Login");
            }

            string userRole = HttpContext.Session.GetString("role");
            if (userRole != "CEO" && userRole != "CTO" && userRole != "Developer")
            {
                return RedirectToAction("Admin");
            }

            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM users WHERE id=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ViewBag.Id = reader["id"];
                    ViewBag.Name = reader["name"];
                    ViewBag.Email = reader["email"];
                    ViewBag.Phone = reader["phone"];
                    ViewBag.Role = reader["role"].ToString();
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, string name, string email, string phone, string role)
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction("Login");
            }

            string userRole = HttpContext.Session.GetString("role");
            if (userRole != "CEO" && userRole != "CTO" && userRole != "Developer")
            {
                return RedirectToAction("Admin");
            }

            string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "UPDATE users SET name=@n, email=@e, phone=@ph, role=@r WHERE id=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@ph", phone);
                cmd.Parameters.AddWithValue("@r", role);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Admin");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult GoogleLogin()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Google:ClientId"]) ||
                string.IsNullOrWhiteSpace(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Google:ClientSecret"]))
            {
                TempData["Message"] = "Google login is not configured yet. Add Google ClientId and ClientSecret first.";
                return RedirectToAction("Login");
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = "/Account/GoogleCallback"
            };
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Cookies");

            if (result.Succeeded)
            {
                var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var googleId = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (email == null ||
                    (!email.EndsWith("@ags-thailand.com") &&
                     email != "thantsinnyan@gmail.com"))
                {
                    await HttpContext.SignOutAsync("Cookies");
                    ViewBag.Message = "❌ Access denied! Only company emails are allowed.";
                    return View("Login");
                }

                string connStr = "server=localhost;user=root;password=tsn2822003;database=company_db";
                string userDbRole = "";

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string check = "SELECT COUNT(*) FROM users WHERE google_id=@gid";
                    MySqlCommand checkCmd = new MySqlCommand(check, conn);
                    checkCmd.Parameters.AddWithValue("@gid", googleId);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exists == 0)
                    {
                        string insert = "INSERT INTO users (email, name, google_id) VALUES (@e, @n, @gid)";
                        MySqlCommand insertCmd = new MySqlCommand(insert, conn);
                        insertCmd.Parameters.AddWithValue("@e", email);
                        insertCmd.Parameters.AddWithValue("@n", name);
                        insertCmd.Parameters.AddWithValue("@gid", googleId);
                        insertCmd.ExecuteNonQuery();
                    }

                    string getRoleQuery = "SELECT role FROM users WHERE google_id=@gid";
                    MySqlCommand getRoleCmd = new MySqlCommand(getRoleQuery, conn);
                    getRoleCmd.Parameters.AddWithValue("@gid", googleId);
                    userDbRole = getRoleCmd.ExecuteScalar()?.ToString() ?? "";
                }

                HttpContext.Session.SetString("user", email);
                HttpContext.Session.SetString("name", name);
                HttpContext.Session.SetString("role", userDbRole);

                return RedirectToAction("Admin");
            }

            return RedirectToAction("Login");
        }
    }
}
