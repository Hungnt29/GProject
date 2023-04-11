using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Authorization;
using GProject.WebApplication.Models;
using GProject.WebApplication.Helpers;
using GProject.Data.DomainClass;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace GProject.WebApplication.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public void SetViewBagInt(int? selected_id = null)
        {
            List<SexDTO> SexSTO = new List<SexDTO>()
                {
                    new SexDTO { ID = 0, Sex = "Nam"},
                    new SexDTO { ID = 1, Sex = "Nữ"},
                };
            ViewBag.Sex = new SelectList(SexSTO, "ID", "Sex", selected_id);
        }
        public IActionResult Register()
        {
            SetViewBagInt();
            return View();
        }

        public async Task<ActionResult> SaveRegister(CustomerDTO Customer)
        {
            try
            {
                string image = "";
                if (Customer.Image_Upload != null)
                {
                    string full_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", Customer.Image_Upload.FileName);
                    using (var file = new FileStream(full_path, FileMode.Create))
                    {
                        Customer.Image_Upload.CopyTo(file);
                    }
                    image = Customer.Image_Upload.FileName;
                }
                SetViewBagInt(Customer.Sex);
                //-- Parse lại dữ liệu từ ViewModel
                var cus = new Customer()
                {
                    Id = Customer.Id,
                    Name = Customer.Name,
                    Email = Customer.Email,
                    CustomerId = Commons.RandomString(10),
                    Password = Customer.Password,
                    CreateDate = DateTime.Now,
                    DateOfBirth = Customer.DateOfBirth,
                    PhoneNumber = Customer.PhoneNumber,
                    Sex = Customer.Sex,
                    Address = Customer.Address,
                    Status = 0,
                    Description = Customer.Description,
                    Image = image
                };

                //-- Gửi request cho api sử lí
                bool result = await Commons.Add_or_UpdateAsync(cus, Commons.mylocalhost + "Customer/add-Customer");
                if (!result)
                    HttpContext.Session.SetString("mess", "Failed");
                else
                    HttpContext.Session.SetString("mess", "Success");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserInfoDTO user)
        {
            try
            {
                #region Đăng nhập tạm khi chưa tạo đc user
                //ViewBag.Error = "Đăng nhập không thành công! Vui lòng nhập lại thông tin đăng nhập!";
                //if (ModelState.IsValid == true)
                //{
                //    if ((user.Email == "manager@gmail.com" || user.Email == "employee@gmail.com") && user.password == "123")
                //    {
                //        var claims = new List<Claim>();
                //        claims.Add(new Claim("username", user.Email));
                //        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Email));
                //        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                //        claims.Add(new Claim(ClaimTypes.Name, "Trần Văn B"));
                //        claims.Add(new Claim(ClaimTypes.Country, user.Image != null ? user.Image : ""));
                //        if (user.Email == "manager@gmail.com")
                //        {
                //            claims.Add(new Claim(ClaimTypes.Role, "manager"));
                //        }
                //        else
                //        {
                //            claims.Add(new Claim(ClaimTypes.Role, "employee"));
                //        }
                //        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                //        await HttpContext.SignInAsync(claimsPrincipal);
                //        return RedirectToAction("Index", "ProductMGR");
                //    }
                //    else if (user.Email == "customer@gmail.com" && user.password == "123")
                //    {
                //        var claims = new List<Claim>();
                //        claims.Add(new Claim("username", user.Email));
                //        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Email));
                //        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                //        claims.Add(new Claim(ClaimTypes.Name, "Nguyễn Văn A"));
                //        claims.Add(new Claim(ClaimTypes.Role, "customer"));
                //        claims.Add(new Claim(ClaimTypes.Country, user.Image != null ? user.Image : ""));
                //        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                //        await HttpContext.SignInAsync(claimsPrincipal);
                //        return RedirectToAction("Index", "Product");
                //    }
                //    else { return View(); }
                //}
                //return BadRequest();
                #endregion
                #region Đăng nhập khi đã tạo đc user
                ViewBag.Error = "Đăng nhập không thành công! Vui lòng nhập lại thông tin đăng nhập!";
                //--Kiểm tra dữ liệu đầu vào
                if (ModelState.IsValid == true)
                {
                    var Employees = await Commons.GetAll<Employee>(String.Concat(Commons.mylocalhost, "Employee/get-all-Employee"));
                    Employee Emp = Employees.FirstOrDefault(c => c.Email.ToLower() == user.Email.ToLower() && c.Password == user.password.ToLower());

                    var Customers = await Commons.GetAll<Customer>(String.Concat(Commons.mylocalhost, "Customer/get-all-Customer"));
                    Customer Cus = Customers.FirstOrDefault(c => c.Email.ToLower() == user.Email.ToLower() && c.Password == user.password.ToLower());
                    if (Emp != null)
                    {
                        Commons.setObjectAsJson(HttpContext.Session, "userLogin", Emp);
                        var claims = new List<Claim>();
                        claims.Add(new Claim("username", Emp.Name));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, Emp.EmployeeId));
                        claims.Add(new Claim(ClaimTypes.Email, Emp.EmployeeId));
                        claims.Add(new Claim(ClaimTypes.Name, Emp.Name));
                        claims.Add(new Claim(ClaimTypes.Country, user.Image != null ? user.Image : ""));
                        if (Emp.Position == Data.Enums.EmployeePosition.Manager)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, "manager"));
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, "employee"));
                        }
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        return RedirectToAction("Index", "Color");
                    }
                    else if (Cus != null)
                    {
                        Commons.setObjectAsJson(HttpContext.Session, "userLogin", Cus);
                        var claims = new List<Claim>();
                        claims.Add(new Claim("username", Cus.Name));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, Cus.Id.ToString()));
                        claims.Add(new Claim(ClaimTypes.Email, Cus.Email));
                        claims.Add(new Claim(ClaimTypes.Name, Cus.Name));
                        claims.Add(new Claim(ClaimTypes.Role, "customer"));
                        claims.Add(new Claim(ClaimTypes.Country, user.Image != null ? user.Image : ""));
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        return RedirectToAction("Index", "Product");
                    }
                    else { return View(); }
                }
                return BadRequest();
                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetLocalhost(string str)
        {
            Commons.mylocalhost = str + "/api/";
            return Ok();
        }


        [HttpGet]
        [Route("Thong-tin-khach-hang.html", Name = ("ProfileCustomer"))]
        public async Task<ActionResult> UpdateProfileCustomer()
        {

            var customer = HttpContext.Session.GetObjectFromJson<Customer>("userLogin");

            if (customer == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var lstCustomer = await Commons.GetAll<Customer>(String.Concat(Commons.mylocalhost, "Customer/get-all-Customer"));
            var dataCustomer = lstCustomer.FirstOrDefault(c => c.Id == customer.Id);

            var userProfile = new UserProfileDTO()
            {
                Image = dataCustomer.Image,
                Name = dataCustomer.Name,
                Email = dataCustomer.Email,
                DateOfBirth = dataCustomer.DateOfBirth,
                PhoneNumber = dataCustomer.PhoneNumber,
                Address = dataCustomer.Address
            };
            return View(userProfile);
        }
        [HttpPost]
        [Route("Thong-tin-khach-hang.html", Name = ("ProfileCustomer"))]
        public async Task<ActionResult> UpdateProfileCustomer(UserProfileDTO user)
        {
            var customer = HttpContext.Session.GetObjectFromJson<Customer>("userLogin");
            if (user.Image_Upload != null)
            {
                string full_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", user.Image_Upload.FileName);
                using (var file = new FileStream(full_path, FileMode.Create))
                {
                    user.Image_Upload.CopyTo(file);
                }
                customer.Image = user.Image_Upload.FileName;
            }
            else
            {
                customer.Image = customer.Image;
            }
            customer.Name = user.Name;
            customer.Email = user.Email;
            customer.DateOfBirth = user.DateOfBirth;
            customer.PhoneNumber = user.PhoneNumber;
            customer.Address = user.Address;
            customer.UpdateDate = DateTime.Now;
            string url = Commons.mylocalhost + "Customer/update-Customer";
            //Gửi request cho API để cập nhập thông tin
            bool result = await Commons.Add_or_UpdateAsync(customer, url);
            if (!result)
            {
                HttpContext.Session.SetString("mess", "Failed");
            }
            else
            {
                // Lưu thông tin người dùng mới vào session
                HttpContext.Session.SetString("userLogin", JsonConvert.SerializeObject(customer));
                HttpContext.Session.SetString("mess", "Success");
            }
            return RedirectToAction("Index", "Product");
        }

        [HttpGet]
        [Route("Thong-tin-nhan-vien.html", Name = ("ProfileEmployee"))]
        public async Task<ActionResult> UpdateProfileEmployee()
        {
            var employee = HttpContext.Session.GetObjectFromJson<Employee>("userLogin");

            if (employee == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var lstEmployee = await Commons.GetAll<Customer>(String.Concat(Commons.mylocalhost, "Employee/get-all-Employee"));
            var dataEmployee = lstEmployee.FirstOrDefault(c => c.Id == employee.Id);

            var userProfile = new UserProfileDTO()
            {
                Image = dataEmployee.Image,
                Name = dataEmployee.Name,
                Email = dataEmployee.Email,
                DateOfBirth = dataEmployee.DateOfBirth,
                PhoneNumber = dataEmployee.PhoneNumber,
                Address = dataEmployee.Address
            };
            return View(userProfile);
        }
        [HttpPost]
        [Route("Thong-tin-nhan-vien.html", Name = ("ProfileEmployee"))]
        public async Task<ActionResult> UpdateProfileEmployee(UserProfileDTO user)
        {
            var employee = HttpContext.Session.GetObjectFromJson<Employee>("userLogin");
            if (user.Image_Upload != null)
            {
                string full_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", user.Image_Upload.FileName);
                using (var file = new FileStream(full_path, FileMode.Create))
                {
                    user.Image_Upload.CopyTo(file);
                }
                employee.Image = user.Image_Upload.FileName;
            }
            else
            {
                employee.Image = employee.Image;
            }
            employee.Name = user.Name;
            employee.Email = user.Email;
            employee.DateOfBirth = user.DateOfBirth;
            employee.PhoneNumber = user.PhoneNumber;
            employee.Address = user.Address;
            employee.UpdateDate = DateTime.Now;
            string url = Commons.mylocalhost + "Employee/update-Employee";
            //Gửi request cho API để cập nhập thông tin
            bool result = await Commons.Add_or_UpdateAsync(employee, url);
            if (!result)
            {
                HttpContext.Session.SetString("mess", "Failed");
            }
            else
            {
                // Lưu thông tin người dùng mới vào session
                HttpContext.Session.SetString("userLogin", JsonConvert.SerializeObject(employee));
                HttpContext.Session.SetString("mess", "Success");
            }
            return RedirectToAction("Index", "Color");
        }

        [HttpGet]
        [Route("Doi-mat-khau-khach-hang.html", Name = ("ChangepasswordCustomer"))]
        public async Task<ActionResult> ChangePasswordCustomer()
        {
            return View();
        }

        [HttpPost]
        [Route("Doi-mat-khau-khach-hang.html", Name = ("ChangepasswordCustomer"))]
        public async Task<ActionResult> ChangePasswordCustomer(ChangePasswordDTO changePasswordDTO)
        {
            var customer = HttpContext.Session.GetObjectFromJson<Customer>("userLogin");
            if (changePasswordDTO.CurrentPassword != customer.Password)
            {
                ViewData["checkCurrentPass"] = "Mật khẩu hiện tại không đúng";
                return RedirectToAction("ChangePasswordCustomer", "Login");
            }
            customer.Password = changePasswordDTO.NewPassword;
            customer.UpdateDate = DateTime.Now;
            string url = Commons.mylocalhost + "Customer/update-Customer";
            //Gửi request cho API để cập nhập thông tin
            bool result = await Commons.Add_or_UpdateAsync(customer, url);
            if (!result)
            {
                HttpContext.Session.SetString("mess", "Failed");
            }
            else
            {
                // Lưu thông tin người dùng mới vào session
                HttpContext.Session.SetString("userLogin", JsonConvert.SerializeObject(customer));
                HttpContext.Session.SetString("mess", "Success");
            }
            return RedirectToAction("Index", "Product");
        }

        [HttpGet]
        [Route("Doi-mat-khau-nhan-vien.html", Name = ("ChangepasswordEmployee"))]
        public async Task<ActionResult> ChangePasswordEmployee()
        {
            return View();
        }

        [HttpPost]
        [Route("Doi-mat-khau-nhan-vien.html", Name = ("ChangepasswordEmployee"))]
        public async Task<ActionResult> ChangePasswordEmployee(ChangePasswordDTO changePasswordDTO)
        {
            var employee = HttpContext.Session.GetObjectFromJson<Employee>("userLogin");
            if (changePasswordDTO.CurrentPassword != employee.Password)
            {
                ViewData["checkCurrentPass"] = "Mật khẩu hiện tại không đúng";
                return RedirectToAction("ChangePasswordEmployee", "Login");
            }
            employee.Password = changePasswordDTO.NewPassword;
            employee.UpdateDate = DateTime.Now;
            string url = Commons.mylocalhost + "Employee/update-Employee";
            //Gửi request cho API để cập nhập thông tin
            bool result = await Commons.Add_or_UpdateAsync(employee, url);
            if (!result)
            {
                HttpContext.Session.SetString("mess", "Failed");
                return RedirectToAction("ChangePasswordEmployee", "Login");
            }
            else
            {
                // Lưu thông tin người dùng mới vào session
                HttpContext.Session.SetString("userLogin", JsonConvert.SerializeObject(employee));
                HttpContext.Session.SetString("mess", "Success");
            }
            return RedirectToAction("Index", "Color");
        }
    }
}
