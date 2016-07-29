using System;
using System.Collections.Generic;
using System.Diagnostics;
using EntityRepository;

namespace SchoolModels
{
    public static class BusinessMethods
    {
        public static void CreateEntities()
        {
            var courseList = new List<TransactionalEntity<Course>>();
            for (var i = 1; i <= 5; i++)
            {
                var entity = new Course($"C#{i.ToString("00")}")
                {
                    CourseName = $"Course Name {i}",
                    Capacity = 4,
                    Point = i
                };
                var course = new TransactionalEntity<Course>(entity);
                course.Update();
                courseList.Add(course);
            }
            var teacherList = new List<TransactionalEntity<Teacher>>();
            for (var i = 1; i <= 2; i++)
            {
                var entity = new Teacher($"T#{i.ToString("00")}")
                {
                    Name = $"Teacher Name {i}"
                };
                var teacher = new TransactionalEntity<Teacher>(entity);
                teacher.Update();
                teacherList.Add(teacher);
            }
            var studentList = new List<TransactionalEntity<Student>>();
            for (var i = 1; i <= 10; i++)
            {
                var entity = new Student($"S#{i.ToString("00")}")
                {
                    Name = $"Student Name {i}"
                };
                var student = new TransactionalEntity<Student>(entity);
                student.Update();
                studentList.Add(student);
            }
            Debug.Print("Done!");
        }

        public static void LoadEntities()
        {
            var courseList = CollectionRepository.GetCollection(typeof (Course)).GetAllEntities(typeof (Course));
            var course = courseList[0] as TransactionalEntity<Course>;
            var courseEntity = course.GetEntity();
            Debug.Print($"course name: {courseEntity.CourseName}, capacity: {courseEntity.Capacity}, students count: {courseEntity.Students.Count}");
            var teacher = courseEntity.Teacher.Reference;
            var teacherEntity = teacher.GetEntity();
            Debug.Print($"teacher name: {teacherEntity.Name}, courses count: {teacherEntity.Courses.Count}");
            var student = courseEntity.Students[0];
            var studentEntity = student.GetEntity();
            Debug.Print($"student name: {studentEntity.Name}, courses count: {studentEntity.Courses.Count}");
            var score = studentEntity.Scores[0];
            var scoreEntity = score.GetEntity();
            Debug.Print($"score final score: {scoreEntity.FinalScore}");
            Debug.Print("Done!");
        }
        /*
        // use extension method of transactional entity to implement business method
        public static bool Register(this TransactionalEntity<Student> student, TransactionalEntity<Course> course)
        {
            using (var ts = TransactionScopeUtil.GetTransactionScope())
            using (var conn = TransactionScopeUtil.CreateDbConnection(Properties.Settings.Default.EntityRepositoryConnectionString))
            {
                try
                {
                    var courseEntity = course.GetEntity();
                    if (courseEntity.Students.Count >= courseEntity.Capacity)
                    {
                        Debug.Print($"Course {courseEntity.CourseName} is full.");
                        return false;
                    }
                    var studentEntity = student.GetEntity();
                    Debug.Print($"student {studentEntity.Name} try to register course {courseEntity.CourseName}.");
                    conn.Open();
                    var scoreEntity = new Score(new ScoreId(courseEntity.CourseNo, studentEntity.StudentId));
                    var score = new TransactionalEntity<Score>(scoreEntity);



                    ts.Complete();
                }
                catch (Exception e)
                {
                    Debug.Print($"Register Failure: {e.Message}");
                    return false;
                }
            }
            return true;
        }
        */
    }
}