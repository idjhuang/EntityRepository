using System;
using EntityRepository;

namespace SchoolModels
{
    // composite primary key
    [Serializable]
    public class ScoreId
    {
        public ScoreId(string courseNo, string studentId)
        {
            CourseNo = courseNo;
            StudentId = studentId;
        }
        public string CourseNo { get; }
        public string StudentId { get; }
    }

    [Serializable]
    public class Score : Entity
    {
        public Score(ScoreId id) : base(id)
        {
            CourseNo = id.CourseNo;
            StudentId = id.StudentId;
        }
        // primary keys should readonly
        public string CourseNo { get; private set; }
        public string StudentId { get; private set; }
        public int FinalScore { get; set; }
    }
}