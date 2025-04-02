using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public uint CourseId { get; set; }
        public string Subject { get; set; } = null!;
        public uint CourseNum { get; set; }
        public string CourseName { get; set; } = null!;

        public virtual Department SubjectNavigation { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
