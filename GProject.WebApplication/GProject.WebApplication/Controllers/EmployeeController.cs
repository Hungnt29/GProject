﻿using GProject.Api.MyServices.IServices;
using GProject.Api.MyServices.Services;
using GProject.Data.DomainClass;
using GProject.WebApplication.Helpers;
using GProject.WebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace GProject.WebApplication.Controllers
{
    [Authorize(Roles = "manager, employee")]
    public class EmployeeController : Controller
    {
        private IEmployeeService iEmployeeService;
        public EmployeeController()
        {
            iEmployeeService = new EmployeeService();
        }

        public async Task<ActionResult> Index(string sId, string sName, string sEmail, string sPhone, int? sGender, int? sStatus, int pg = 1)
        {
            try
            {
                //-- Lấy danh sách từ api
                var lstObjs = await Commons.GetAll<Employee>(String.Concat(Commons.mylocalhost, "Employee/get-all-Employee"));

                int valsGender = sGender.HasValue ? sGender.Value : -1;
                int valsStatus = sStatus.HasValue ? sStatus.Value : -1;

                if (!string.IsNullOrEmpty(sId))
                    lstObjs = lstObjs.Where(c => c.EmployeeId.ToLower().Contains(sId.ToLower())).ToList();
                if (!string.IsNullOrEmpty(sName))
                    lstObjs = lstObjs.Where(c => c.Name.ToLower().Contains(sName.ToLower())).ToList();
                if (!string.IsNullOrEmpty(sEmail))
                    lstObjs = lstObjs.Where(c => c.Email.ToLower().Contains(sEmail.ToLower())).ToList();
                if (!string.IsNullOrEmpty(sPhone))
                    lstObjs = lstObjs.Where(c => c.PhoneNumber.ToLower().Contains(sPhone.ToLower())).ToList();
                
                if (valsGender != -1)
                    lstObjs = lstObjs.Where(c => c.Sex == valsGender).ToList();
                if (valsStatus != -1)
                    lstObjs = lstObjs.Where(c => c.Status == valsStatus).ToList();
                const int pageSize = 5;
                if (pg < 1)
                    pg = 1;
                var pager = new Pager(lstObjs.Count(), pg, pageSize);
                var lstData = lstObjs.Skip((pg - 1) * pageSize).Take(pageSize).ToList();
                this.ViewBag.Pager = pager;
                this.ViewData[nameof(sName)] = (object)sName;
                this.ViewData[nameof(sEmail)] = (object)sEmail;
                this.ViewData[nameof(sPhone)] = (object)sPhone;
                this.ViewData[nameof(sId)] = (object)sId;
                this.ViewData[nameof(sGender)] = (object)valsGender;
                this.ViewData[nameof(sStatus)] = (object)valsStatus;
                var data = new EmployeeDTO() { EmployeeList = lstData };

                //-- truyền vào message nếu có thông báo
                if (!string.IsNullOrEmpty(HttpContext.Session.GetString("mess")))
                    ViewData["Mess"] = HttpContext.Session.GetString("mess");
                HttpContext.Session.Remove("mess");
                return View(data);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Save( EmployeeDTO Employee)
        {
            try
            {
                //-- Parse lại dữ liệu từ ViewModel
                var emp = new Employee();
                emp.Id = Employee.Id;
                emp.Name = Employee.Name;
                emp.Email = Employee.Email;
                emp.Position = Employee.Position;
                emp.PersonalId = Employee.PersonalId;
                emp.EmployeeId = Employee.EmployeeId;
                if (!string.IsNullOrEmpty(Employee.Password)) emp.Password = Employee.Password;
                emp.CreateDate = DateTime.Now;
                emp.UpdateDate = DateTime.Now;
                emp.DateOfBirth = Employee.DateOfBirth;
                emp.PhoneNumber = Employee.PhoneNumber;
                emp.Sex = Employee.Sex;
                emp.Address = Employee.Address;
                emp.Status = Employee.Status;
                if (!string.IsNullOrEmpty(Employee.Description)) emp.Description = Employee.Description;
                if (Employee.Image_Upload != null)
                {
                    string full_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", Employee.Image_Upload.FileName);
                    using (var file = new FileStream(full_path, FileMode.Create))
                    {
                        Employee.Image_Upload.CopyTo(file);
                    }
                    emp.Image = Employee.Image_Upload.FileName;
                }
                string url = Commons.mylocalhost;


                //-- Check hành động là Create hay update
                if (Employee.Id == null) url += "Employee/add-Employee";
                else url += "Employee/update-Employee";

                //-- Gửi request cho api sử lí
                bool result = await Commons.Add_or_UpdateAsync(emp, url);
                if (!result) 
                    HttpContext.Session.SetString("mess", "Failed");
                else 
                    HttpContext.Session.SetString("mess", "Success");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        public async Task<JsonResult> Detail(Guid id)
        {
            var lstObjs = await Commons.GetAll<Employee>(String.Concat(Commons.mylocalhost, "Employee/get-all-Employee"));
            var data = lstObjs.FirstOrDefault(c => c.Id == id);
            return Json(data);
        }
    }
}
