﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ClassSchedule.Models;

namespace ClassSchedule.Controllers
{
    public class ClassController : Controller
    {
        private readonly IClassScheduleUnitOfWork data;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClassController(IClassScheduleUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            data = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public RedirectToActionResult Index()
        {
            // clear session and navigate to list of classes
            _httpContextAccessor.HttpContext.Session.Remove("dayid");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ViewResult Add()
        {
            this.LoadViewBag("Add");
            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            this.LoadViewBag("Edit");
            var c = this.GetClass(id);
            return View("Add", c);
        }

        [HttpPost]
        public IActionResult Add(Class c)
        {
            string operation = (c.ClassId == 0) ? "Add" : "Edit";
            if (ModelState.IsValid)
            {
                if (c.ClassId == 0)
                    data.Classes.Insert(c);
                else
                    data.Classes.Update(c);
                data.Save();

                string verb = (operation == "Add") ? "added" : "updated";
                TempData["msg"] = $"{c.Title} {verb}";

                return this.GoToClasses();
            }
            else
            {
                this.LoadViewBag(operation);
                return View();
            }
        }

        [HttpGet]
        public ViewResult Delete(int id)
        {
            var c = this.GetClass(id);
            ViewBag.DayId = _httpContextAccessor.HttpContext.Session.GetInt32("dayid");
            return View(c);
        }

        [HttpPost]
        public RedirectToActionResult Delete(Class c)
        {
            c = data.Classes.Get(c.ClassId); // so we can get class title for notification message
            data.Classes.Delete(c);
            data.Save();

            TempData["msg"] = $"{c.Title} deleted";

            return this.GoToClasses();
        }

        // private helper methods
        private Class GetClass(int id)
        {
            var classOptions = new QueryOptions<Class>
            {
                Includes = "Teacher, Day",
                Where = c => c.ClassId == id
            };
            return data.Classes.Get(classOptions);
        }

        private void LoadViewBag(string operation)
        {
            ViewBag.Days = data.Days.List(new QueryOptions<Day>
            {
                OrderBy = d => d.DayId
            });
            ViewBag.Teachers = data.Teachers.List(new QueryOptions<Teacher>
            {
                OrderBy = t => t.LastName
            });

            ViewBag.Operation = operation;
            ViewBag.DayId = _httpContextAccessor.HttpContext.Session.GetInt32("dayid");
        }

        private RedirectToActionResult GoToClasses()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            // if session has a value for day id, add to id route segment when redirecting
            if (session.GetInt32("dayid").HasValue)
                return RedirectToAction("Index", "Home", new { id = session.GetInt32("dayid") });
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
