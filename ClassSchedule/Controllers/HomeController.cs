﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ClassSchedule.Models;

namespace ClassSchedule.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClassScheduleUnitOfWork data;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IClassScheduleUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            data = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public ViewResult Index(int id)
        {
            // access HttpContext through IHttpContextAccessor
            var httpContext = _httpContextAccessor.HttpContext;

            // if day id passed to action method, store in session
            if (id > 0)
            {
                httpContext.Session.SetInt32("dayid", id);
            }

            // options for Days query
            var dayOptions = new QueryOptions<Day>
            {
                OrderBy = d => d.DayId
            };

            // options for Classes query
            var classOptions = new QueryOptions<Class>
            {
                Includes = "Teacher, Day"
            };

            // order by day if no day id. Otherwise, filter by day and order by time.
            if (id == 0)
            {
                classOptions.OrderBy = c => c.DayId;
            }
            else
            {
                classOptions.Where = c => c.DayId == id;
                classOptions.OrderBy = c => c.MilitaryTime;
            }

            // execute queries
            ViewBag.Days = data.Days.List(dayOptions);
            return View(data.Classes.List(classOptions));
        }
    }
}
