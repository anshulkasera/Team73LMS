﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from enrollment in db.Enrolleds
                        join classes in db.Classes on enrollment.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where enrollment.UId == uid
                        select new
                        {
                            subject = courses.Subject,
                            number = courses.CourseNum,
                            name = courses.CourseName,
                            season = classes.SemSeason,
                            year = classes.SemYear,
                            grade = enrollment.Grade
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from enrollment in db.Enrolleds
                        join classes in db.Classes on enrollment.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        join assignmentCategory in db.AssignmentCategories on classes.ClassId equals assignmentCategory.ClassId
                        join assignment in db.Assignments on assignmentCategory.CategoryId equals assignment.CategoryId
                        where enrollment.UId == uid && courses.Subject == subject && courses.CourseNum == num
                        && classes.SemSeason == season && classes.SemYear == year
                        select new
                        {
                            aname = assignment.Name,
                            cname = assignmentCategory.Name,
                            due = assignment.DueDate,
                            score = (from submission in db.Submissions
                                     where submission.AssignmentId == assignment.AssignmentId && submission.UId == uid
                                     select (uint?)submission.Score).FirstOrDefault()
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query = from assignmentCategories in db.AssignmentCategories
                        join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                        join submissions in db.Submissions on assigments.AssignmentId equals submissions.AssignmentId
                        join students in db.Students on submissions.UId equals students.UId
                        where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                        && classes.SemYear == year && assignmentCategories.Name == category && assigments.Name == asgname
                        && students.UId == uid
                        select submissions;
            if (query.Any()) //If there is a submission, update the submission
            {
                foreach (Submission sub in query)
                {
                    sub.Time = DateTime.Now;
                    sub.Contents = contents;
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Json(new { success = false });
                }
            }
            else
            {
                var submission = new Submission
                {
                    Time = DateTime.Now,
                    Contents = contents,
                    Score = 0,
                    UId = uid,
                    AssignmentId = (from assignmentCategories in db.AssignmentCategories
                                    join classes in db.Classes on assignmentCategories.ClassId equals classes.ClassId
                                    join courses in db.Courses on classes.CourseId equals courses.CourseId
                                    join assigments in db.Assignments on assignmentCategories.CategoryId equals assigments.CategoryId
                                    where courses.Subject == subject && courses.CourseNum == num && classes.SemSeason == season
                                    && classes.SemYear == year && assignmentCategories.Name == category && assigments.Name == asgname
                                    select assigments.AssignmentId).SingleOrDefault()
                };
                db.Submissions.Add(submission);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from student in db.Students
                        join enrollment in db.Enrolleds on student.UId equals enrollment.UId
                        join classes in db.Classes on enrollment.ClassId equals classes.ClassId
                        join courses in db.Courses on classes.CourseId equals courses.CourseId
                        where (classes.SemSeason == season && classes.SemYear == year)
                        && courses.Subject == subject && courses.CourseNum == num && student.UId == uid
                        select student;

            var query2 = from classes in db.Classes
                         join courses in db.Courses on classes.CourseId equals courses.CourseId
                         where (classes.SemSeason == season && classes.SemYear == year)
                         && courses.Subject == subject && courses.CourseNum == num
                         select classes.ClassId;

            uint result = query2.FirstOrDefault();
            if (query.Any())
                return Json(new { success = false });
            else
            {
                var enrollment = new Enrolled()
                {
                   Grade = "--", 
                   UId = uid,
                   ClassId = result
                };
                db.Enrolleds.Add(enrollment);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = (from students in db.Students
                         join enrolled in db.Enrolleds on students.UId equals enrolled.UId
                         where students.UId == uid && enrolled.Grade != "--"
                         select enrolled.Grade).ToArray();
            if (!query.Any())
                return Json(new
                {
                    gpa = 0.0
                });
               
            float totalGrade = 0.0f;
            foreach(string grade in query)
            {
                if (grade == "A")
                    totalGrade += 4.0f * 4;
                else if (grade == "A-")
                    totalGrade += 3.7f * 4; 
                else if (grade == "B+")
                    totalGrade += 3.3f * 4; 
                 else if (grade == "B")
                    totalGrade += 3.0f * 4; 
                 else if (grade == "B-")
                    totalGrade += 2.7f * 4; 
                 else if (grade == "C+")
                    totalGrade += 2.3f * 4; 
                 else if (grade == "C")
                    totalGrade += 2.0f * 4;
                else if (grade == "C-")
                    totalGrade += 1.7f * 4;
                else if (grade == "D+")
                    totalGrade += 1.3f * 4;
                else if (grade == "D")
                    totalGrade += 1.0f * 4;
                else if (grade == "D-")
                    totalGrade += 0.7f * 4;
            }
            float GPA = totalGrade / (query.Length * 4);

            return Json(new
            {
                gpa = GPA
            });
        }
                
        /*******End code to modify********/

    }
}

