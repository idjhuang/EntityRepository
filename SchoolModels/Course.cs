using System;
using EntityRepository;

namespace SchoolModels
{
    [Serializable]
    public class Course : Entity
    {
        public Course(string id) : base(id)
        {
            CourseNo = id;
            Teacher = new TransactionalEntityReference<Teacher>();
            Students = new TransactionalEntityList<Student>();
            Capacity = 10;
            Point = 1;
        }
        // primary key should readonly
        public string CourseNo { get; private set; }
        public string CourseName { get; set; }
        public int Capacity { get; set; }
        public int Point { get; set; }
        // references should readonly too, except for collection
        public TransactionalEntityReference<Teacher> Teacher { get; internal set; } 
        public TransactionalEntityList<Student> Students { get; internal set; }
    }
}
