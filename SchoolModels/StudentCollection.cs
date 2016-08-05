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
    public class StudentCollection : TransactionalEntityCollection<Student, string>
    {
        protected override Student GetEntityImpl(string id)
        {
            var studentTableAdapter = new StudentTableAdapter();
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
            {
                studentTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            }
            var studentTbl = studentTableAdapter.GetDataById(id);
            if (studentTbl.Rows.Count == 0) throw new ArgumentException($"Object's Id ({id}) not found.");
            var studentRow = studentTbl.Rows[0] as School.StudentRow;
            // create student
            Debug.Assert(studentRow != null, "studentRow != null");
            var student = new Student(id)
            {
                Name = studentRow.Name
            };
            // populate reference of courses and scores
            var scoreTbl = scoreTableAdapter.GetDataByStudentId(id);
            var courseList = (from School.ScoreRow row in scoreTbl.Rows select row.CourseNo).Cast<object>().ToList();
            var scoreList = (from School.ScoreRow row in scoreTbl.Rows select new ScoreId(row.CourseNo, row.StudentId)).Cast<object>().ToList();
            // set reference as unloaded state by specific constructor
            student.Courses = new TransactionalEntityList<Course>(courseList);
            student.Scores = new TransactionalEntityList<Score>(scoreList);
            return student;
        }

        protected override void InsertEntityImpl(Student entity)
        {
            var adapter = new StudentTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.AddData(entity.StudentId, entity.Name);
        }

        protected override void UpdateEntityImpl(Student entity)
        {
            var adapter = new StudentTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.AddData(entity.StudentId, entity.Name);
        }

        protected override void DeleteEntityImpl(Student entity)
        {
            var adapter = new StudentTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            adapter.DeleteData(entity.StudentId);
        }

        protected override IList<object> GetAllEntitiesImpl()
        {
            var adapter = new StudentTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            var tbl = adapter.GetData();
            // populate all students
            return (from School.StudentRow row in tbl.Rows select GetEntityImpl(row.StudentId)).Cast<object>().ToList();
        }
    }
}