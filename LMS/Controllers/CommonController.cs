using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from departments in db.Departments
                        select new { name = departments.Name, subject = departments.Subject };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from department in db.Departments
                        join course in db.Courses on department.Subject equals course.Subject
                        group new { department, course } by department.Subject into subjectGroup
                        select new
                        {
                            subject = subjectGroup.Key,
                            dname = subjectGroup.FirstOrDefault().department.Name,
                            courses = (from c in subjectGroup
                                       select new
                                       {
                                           number = c.course.CourseNum,
                                           cname = c.course.CourseName
                                       }).ToList()
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query = from classes in db.Classes
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        join professors in db.Professors on classes.UId equals professors.UId
                        where courses.Subject == subject && courses.CourseNum == number
                        select new
                        {
                            season = classes.SemSeason,
                            year = classes.SemYear,
                            location = classes.Location,
                            start = classes.StartTime,
                            end = classes.EndTime,
                            fname = professors.FName,
                            lname = professors.LName
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = (from assignments in db.Assignments
                         join assignmentCat in db.AssignmentCategories on assignments.CategoryId equals assignmentCat.CategoryId
                         join classes in db.Classes on assignmentCat.ClassId equals classes.ClassId
                         join courses in db.Courses on classes.CourseId equals courses.CourseId
                         where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season &&
                         classes.SemYear == year && assignmentCat.Name == category && assignments.Name == asgname
                         select assignments.Contents).SingleOrDefault();
            return Content(query!);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query = (from submission in db.Submissions
                         join assignments in db.Assignments on submission.AssignmentId equals assignments.AssignmentId
                         join assignmentCat in db.AssignmentCategories on assignments.CategoryId equals assignmentCat.CategoryId
                         join classes in db.Classes on assignmentCat.ClassId equals classes.ClassId
                         join courses in db.Courses on classes.CourseId equals courses.CourseId
                         where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season &&
                         classes.SemYear == year && assignmentCat.Name == category && assignments.Name == asgname && submission.UId == uid
                         select submission.Contents).SingleOrDefault();
            return Content(query!);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var studentQuery = from student in db.Students
                               join departments in db.Departments on student.Subject equals departments.Subject
                               where uid == student.UId
                               select new
                               {
                                   fname = student.FName,
                                   lname = student.LName,
                                   uid = student.UId,
                                   department = departments.Name
                               };
            if (studentQuery.Any())
                return Json(studentQuery.SingleOrDefault());
           
            var professorQuery = from professor in db.Professors
                                 join departments in db.Departments on professor.Subject equals departments.Subject
                                 where uid == professor.UId
                               select new
                               {
                                   fname = professor.FName,
                                   lname = professor.LName,
                                   uid = professor.UId,
                                   department = departments.Name
                               };
            if (professorQuery.Any())
                return Json(professorQuery.SingleOrDefault());
            
            var adminQuery = from admin in db.Administrators
                                 where uid == admin.UId
                                 select new
                                 {
                                     fname = admin.FName,
                                     lname = admin.LName,
                                     uid = admin.UId
                                 };
            if (adminQuery.Any())
                return Json(adminQuery.SingleOrDefault());

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

