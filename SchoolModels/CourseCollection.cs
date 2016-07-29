using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using EntityRepository;
using SchoolModels.SchoolTableAdapters;

namespace SchoolModels
{
    public class CourseCollection : TransactionalEntityCollection<Course, string>
    {
        protected override TransactionalEntity<Course> GetEntityImpl(string id)
        {
            var courseTableAdapter = new CourseTableAdapter();
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
            {
                courseTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            }
            var courseTbl = courseTableAdapter.GetDataByCourseNo(id);
            if (courseTbl.Rows.Count == 0) throw new ArgumentException($"Object's Id ({id}) not found.");
            var courseRow = courseTbl.Rows[0] as School.CourseRow;
            // create course
            Debug.Assert(courseRow != null, "courseRow != null");
            var course = new Course(id)
            {
                CourseName = courseRow.CourseName,
                Capacity = courseRow.Capacity,
                Point = courseRow.Point,
                // populate reference of teacher
                Teacher = new TransactionalEntityReference<Teacher>(courseRow.Teacher)
            };
            // populate reference of student
            var scoreTbl = scoreTableAdapter.GetDataByCourseNo(id);
            var studentList = (from School.ScoreRow row in scoreTbl.Rows select row.StudentId).Cast<object>().ToList();
            // set reference as unloaded state by specific constructor
            course.Students = new TransactionalEntityList<Student>(studentList);
            return new TransactionalEntity<Course>(course);
        }

        protected override void InsertEntityImpl(Course entity)
        {
            var adapter = new CourseTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.AddData(entity.CourseNo, entity.CourseName, entity.Teacher.Reference?.GetEntity().TeacherId, entity.Capacity, entity.Point);
        }

        protected override void UpdateEntityImpl(Course entity)
        {
            var adapter = new CourseTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.AddData(entity.CourseNo, entity.CourseName, entity.Teacher.Reference?.GetEntity().TeacherId, entity.Capacity, entity.Point);
        }

        protected override void DeleteEntityImpl(Course entity)
        {
            var adapter = new CourseTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.DeleteData(entity.CourseNo);
        }

        protected override IList<object> GetAllEntitiesImpl()
        {
            var adapter = new CourseTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            var tbl = adapter.GetData();
            // populate all courses
            return (from School.CourseRow row in tbl.Rows select GetEntityImpl(row.CourseNo)).Cast<object>().ToList();
        }
    }
}