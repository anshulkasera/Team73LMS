using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var query = from departments in db.Departments
                        where departments.Subject == subject
                        && departments.Name == name
                        select departments;
            if (query.Any())
                return Json(new { success = false });
            else
            {
                var department = new Department()
                {
                    Subject = subject,
                    Name = name
                };
                db.Departments.Add(department);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from courses in db.Courses
                        where courses.Subject == subject
                        select new { name = courses.CourseName, number = courses.CourseNum };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from professors in db.Professors
                        where professors.Subject == subject
                        select new { fname = professors.FName, lname = professors.LName, uid = professors.UId };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            var query = from courses in db.Courses
                        where courses.Subject == subject
                        && courses.CourseNum == number
                        select courses;
            if (query.Any())
                return Json(new { success = false });
            else
            {
                var course = new Course()
                {
                    Subject = subject,
                    CourseName = name,
                    CourseNum = (uint)number
                };
                db.Courses.Add(course);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var query = from classes in db.Classes
                        join course in db.Courses
                        on classes.CourseId equals course.CourseId
                        into joined
                        from j in joined.DefaultIfEmpty()
                        where (((classes.StartTime >= TimeOnly.FromDateTime(start) && classes.StartTime <= TimeOnly.FromDateTime(end))
                        || (classes.EndTime >= TimeOnly.FromDateTime(start) && classes.EndTime <= TimeOnly.FromDateTime(end)))
                        && classes.Location == location && (classes.SemSeason == season && classes.SemYear == year))
                        || ((classes.SemSeason == season && classes.SemYear == year) && (j.CourseNum == number && j.Subject == subject))
                        select j;
            var query2 = from courses in db.Courses
                         where courses.Subject == subject && courses.CourseNum == number
                         select courses.CourseId;
            uint result = query2.FirstOrDefault();
            if (query.Any())
                return Json(new { success = false });
            else
            {
                var c = new Class()
                {
                    CourseId = result,                        
                    Location = location,
                    StartTime = TimeOnly.FromDateTime(start),
                    EndTime = TimeOnly.FromDateTime(end),
                    SemYear = (ushort)year,
                    SemSeason = season,
                    UId = instructor
                };
                db.Classes.Add(c);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        /*******End code to modify********/

   }
}

