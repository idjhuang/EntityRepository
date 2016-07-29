using System;
using EntityRepository;

namespace SchoolModels
{
    [Serializable]
    public class Student : Entity
    {
        public Student(string id) : base(id)
        {
            StudentId = id;
            Courses = new TransactionalEntityList<Course>();
            Scores = new TransactionalEntityList<Score>();
        }
        // primary key should readonly
        public string StudentId { get; private set; }
        public string Name { get; set; }
        // references should readonly, except for collection
        public TransactionalEntityList<Course> Courses { get; internal set; }
        public TransactionalEntityList<Score> Scores { get; internal set; } 
    }
}