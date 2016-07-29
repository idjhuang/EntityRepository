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
    public class ScoreCollection : TransactionalEntityCollection<Score, ScoreId>
    {
        protected override TransactionalEntity<Score> GetEntityImpl(ScoreId id)
        {
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            var scoreTbl = scoreTableAdapter.GetDataByCourseNoAndStudentId(id.CourseNo, id.StudentId);
            if (scoreTbl.Rows.Count == 0) throw new ArgumentException($"Object's Id ({id}) not found.");
            var scoreRow = scoreTbl.Rows[0] as School.ScoreRow;
            // create score
            Debug.Assert(scoreRow != null, "scoreRow != null");
            var score = new Score(id)
            {
                FinalScore = scoreRow.FinalScore
            };
            return new TransactionalEntity<Score>(score);
        }

        protected override void InsertEntityImpl(Score entity)
        {
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            scoreTableAdapter.AddData(entity.CourseNo, entity.StudentId, entity.FinalScore);
        }

        protected override void UpdateEntityImpl(Score entity)
        {
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            scoreTableAdapter.AddData(entity.CourseNo, entity.StudentId, entity.FinalScore);
        }

        protected override void DeleteEntityImpl(Score entity)
        {
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            scoreTableAdapter.DeleteData(entity.CourseNo, entity.StudentId);
        }

        protected override IList<object> GetAllEntitiesImpl()
        {
            var scoreTableAdapter = new ScoreTableAdapter();
            if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                scoreTableAdapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
            var tbl = scoreTableAdapter.GetData();
            // populate all scores
            return (from School.ScoreRow row in tbl.Rows select GetEntityImpl(new ScoreId(row.CourseNo, row.StudentId))).Cast<object>().ToList();
        }
    }
}