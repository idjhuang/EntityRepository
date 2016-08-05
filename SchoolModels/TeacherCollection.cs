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
    public class TeacherCollection : TransactionalEntityCollection<Teacher, string>
    {
        protected override Teacher GetEntityImpl(string id)
        {
            var teacherTableAdapter = new TeacherTableAdapter();
            var courseTableAdapter = new CourseTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
            {
                teacherTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                courseTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            }
            var teacherTbl = teacherTableAdapter.GetDataById(id);
            if (teacherTbl.Rows.Count == 0) throw new ArgumentException($"Object's Id ({id}) not found.");
            var thacherRow = teacherTbl.Rows[0] as School.TeacherRow;
            // create teacher
            Debug.Assert(thacherRow != null, "thacherRow != null");
            var teacher = new Teacher(id) {Name = thacherRow.Name};
            // populate reference of course
            var courseTbl = courseTableAdapter.GetDataByTeacherId(id);
            var courseList = (from School.CourseRow row in courseTbl.Rows select row.CourseNo).Cast<object>().ToList();
            // set reference as unloaded state by specific constructor
            teacher.Courses = new TransactionalEntityList<Course>(courseList);
            return teacher;
        }

        protected override void InsertEntityImpl(Teacher entity)
        {
            var adapter = new TeacherTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.AddData(entity.TeacherId, entity.Name);
        }

        protected override void UpdateEntityImpl(Teacher entity)
        {
            var adapter = new TeacherTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.AddData(entity.TeacherId, entity.Name);
        }

        protected override void DeleteEntityImpl(Teacher entity)
        {
            var adapter = new TeacherTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.DeleteData(entity.TeacherId);
        }

        protected override IList<object> GetAllEntitiesImpl()
        {
            var adapter = new TeacherTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            var tbl = adapter.GetData();
            // populate all teachers
            return (from School.TeacherRow row in tbl.Rows select GetEntityImpl(row.TeacherId)).Cast<object>().ToList();
        }
    }
}