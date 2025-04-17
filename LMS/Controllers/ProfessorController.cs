using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from student in db.Students
                        join enrollment in db.Enrolleds on student.UId equals enrollment.UId
                        join classes in db.Classes on enrollment.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where (classes.SemSeason == season && classes.SemYear == year)
                        && courses.Subject == subject && courses.CourseNum == num
                        select new { fname = student.FName, lname = student.LName, uid = student.UId, dob = student.Dob, enrollment.Grade };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category is not null)
            {
                var query = from assignmentCategories in db.AssignmentCategories
                            join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                            join courses in db.Courses on classes.CourseId equals courses.CourseId
                            join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                            where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                            && classes.SemYear == year && assignmentCategories.Name == category
                            select new
                            {
                                aname = assigments.Name,
                                cname = assignmentCategories.Name,
                                due = assigments.DueDate,
                                submissions = (from s in db.Submissions
                                               where s.AssignmentId == assigments.AssignmentId
                                               select s).Count()
                            };
                return Json(query.ToArray());
            }

            else
            {
                var query = from assignmentCategories in db.AssignmentCategories
                            join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                            join courses in db.Courses on classes.CourseId equals courses.CourseId
                            join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                            where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                            && classes.SemYear == year
                            select new
                            {
                                aname = assigments.Name,
                                cname = assignmentCategories.Name,
                                due = assigments.DueDate,
                                submissions = (from s in db.Submissions
                                               where s.AssignmentId == assigments.AssignmentId
                                               select s).Count()
                            };
                return Json(query.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from assignmentCategories in db.AssignmentCategories
                        join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                        && classes.SemYear == year
                        select new
                        {
                            name = assignmentCategories.Name,
                            weight = assignmentCategories.Weight
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from assignmentCategories in db.AssignmentCategories
                        join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                        && classes.SemYear == year && assignmentCategories.Name == category
                        select assignmentCategories;

            if (query.Any())
                Json(new { success = false });
            else
            {
                var assignmentCategory = new AssignmentCategory
                {
                    Name = category,
                    Weight = (uint)catweight,
                    ClassId = (from classes in db.Classes
                               join courses in db.Courses on classes.CourseId equals courses.CourseId
                               where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                               && classes.SemYear == year
                               select classes.ClassId).SingleOrDefault()
                };
                db.AssignmentCategories.Add(assignmentCategory);
                db.SaveChanges();
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var query = from assignmentCategories in db.AssignmentCategories
                        join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                        where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                        && classes.SemYear == year && assignmentCategories.Name == category && assigments.Name == asgname &&
                        assigments.Contents == asgcontents && assigments.Points == asgpoints && assigments.DueDate == asgdue
                        select assigments;
            if (query.Any())
                return Json(new { success = false });
            else
            {
                var Assignment = new Assignment()
                {
                    Name = asgname,
                    Points = (uint)asgpoints,
                    DueDate = asgdue,
                    Contents = asgcontents,
                    CategoryId = (from assignmentCategories in db.AssignmentCategories
                                  join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                                  join courses in db.Courses on classes.CourseId equals courses.CourseId
                                  where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                                  && classes.SemYear == year && assignmentCategories.Name == category
                                  select assignmentCategories.CategoryId).SingleOrDefault()
                };
                db.Assignments.Add(Assignment);
                //db.SaveChanges();

            }
            var students = from student in db.Students
                        join enrollment in db.Enrolleds on student.UId equals enrollment.UId
                        join classes in db.Classes on enrollment.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where (classes.SemSeason == season && classes.SemYear == year)
                        && courses.Subject == subject && courses.CourseNum == num
                        select student;
            var classID = from classes in db.Classes 
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where(classes.SemSeason == season && classes.SemYear == year)
                        && courses.Subject == subject && courses.CourseNum == num
                        select classes.ClassId;
            var studentList = students.ToList();
            var cID = classID.First();
            foreach (Student student in studentList)
            {
                UpdateClassGrade(cID, student.UId);
            }
            return Json(new { success = true });
        }
        //TODO: ADD COMMENT HERE AND OTHER METHOD
        private void UpdateClassGrade(uint classID, string uid)
        {
            var categories = (from cat in db.AssignmentCategories where cat.ClassId == classID select cat).ToArray();

            float postScaleScore = 0.0f;
            uint catTotal = 0;

            foreach (AssignmentCategory cat in categories)
            {
                uint catTotalPoints = 0;
                uint catPointsEarned = 0;
                var assignments = (from assign in db.Assignments
                                   where assign.CategoryId == cat.CategoryId
                                   select assign).ToArray();
                if (assignments.Count() == 0)
                    continue;
                catTotal += cat.Weight;

                foreach (Assignment asg in assignments)
                {
                    catTotalPoints += asg.Points;
                    var submission = (from sub in db.Submissions
                                      where sub.UId == uid && sub.AssignmentId == asg.AssignmentId
                                      select sub.Score).ToArray();
                    if (submission.Count() == 1)
                        catPointsEarned += submission.First(); //Problem, grabbing old score?
                }
                float categoryPercentage = (float)catPointsEarned / (float)catTotalPoints;
                postScaleScore += (categoryPercentage *= cat.Weight);

            }
            float scale = 100.0f / catTotal;
            float totalCoursePercentage = (postScaleScore * scale);
            string letterGrade = LetterGrade(totalCoursePercentage);

            var enrollment = from enroll in db.Enrolleds
                             where enroll.UId == uid && enroll.ClassId == classID
                             select enroll;
            if (enrollment.Count() != 1)
                throw new ArgumentException("Cannot calculate grade for non-existing enrollment :(");
            Enrolled newEnrollment = enrollment.First();
            newEnrollment.Grade = letterGrade;
            db.SaveChanges();
        }

        private string LetterGrade(float coursePercentage)
        {
            string letterGrade = "";
            if (coursePercentage >= 93)
                letterGrade = "A";
            else if (coursePercentage >= 90)
                letterGrade = "A-";
            else if (coursePercentage >= 87)
                letterGrade = "B+";
            else if (coursePercentage >= 83)
                letterGrade = "B";
            else if (coursePercentage >= 80)
                letterGrade = "B-";
            else if (coursePercentage >= 77)
                letterGrade = "C+";
            else if (coursePercentage >= 73)
                letterGrade = "C";
            else if (coursePercentage >= 70)
                letterGrade = "C-";
            else if (coursePercentage >= 67)
                letterGrade = "D+";
            else if (coursePercentage >= 63)
                letterGrade = "D";
            else if (coursePercentage >= 60)
                letterGrade = "D-";
            else
                letterGrade = "E";
            return letterGrade;
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from assignmentCategories in db.AssignmentCategories
                        join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                        join submissions in db.Submissions on assigments.AssignmentId equals submissions.AssignmentId
                        join students in db.Students on submissions.UId equals students.UId
                        where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                        && classes.SemYear == year && assignmentCategories.Name == category && assigments.Name == asgname
                        select new
                        {
                            fname = students.FName,
                            lname = students.LName,
                            uid = students.UId,
                            time = submissions.Time,
                            score = submissions.Score
                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from assignmentCategories in db.AssignmentCategories
                        join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                        join submissions in db.Submissions on assigments.AssignmentId equals submissions.AssignmentId
                        join students in db.Students on submissions.UId equals students.UId
                        where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                        && classes.SemYear == year && assignmentCategories.Name == category && assigments.Name == asgname
                        select submissions;
            if (!query.Any())
            {
                return Json(new { success = false });
            }
            foreach (Submission sub in query)

                sub.Score = (uint)score;
            try
            {
                db.SaveChanges();
                var classID = from classes in db.Classes
                              join courses in db.Courses on classes.CourseId equals courses.CourseId
                              where (classes.SemSeason == season && classes.SemYear == year)
                              && courses.Subject == subject && courses.CourseNum == num
                              select classes.ClassId;
                var cID = classID.First();
                UpdateClassGrade(cID, uid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }
            return Json(new { success = true });
        }

        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from classes in db.Classes
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where classes.UId == uid
                        select new
                        {
                            subject = courses.Subject,
                            number = courses.CourseNum,
                            name = courses.CourseName,
                            season = classes.SemSeason,
                            year = classes.SemYear,
                        };
            return Json(query.ToArray());
        }



        /*******End code to modify********/
    }
}

