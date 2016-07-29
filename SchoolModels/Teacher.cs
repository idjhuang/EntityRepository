using System;
using EntityRepository;

namespace SchoolModels
{
    [Serializable]
    public class Teacher : Entity
    {
        public Teacher(string id) : base(id)
        {
            TeacherId = id;
            Courses = new TransactionalEntityList<Course>();
        }
        // primary key should readonly
        public string TeacherId { get; private set; }

        public string Name { get; set; }
        // references should readonly, except for collection
        public TransactionalEntityList<Course> Courses { get; internal set; }
    }
}