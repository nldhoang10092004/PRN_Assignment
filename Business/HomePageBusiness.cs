using Repository.DAO;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class HomePageBusiness
    {
        private readonly SpeakingQuestionDAO _speakingQuestionDao;
        private readonly WritingQuestionDAO _writingQuestionDao;
        public HomePageBusiness()
        {
            _speakingQuestionDao = new SpeakingQuestionDAO();
            _writingQuestionDao = new WritingQuestionDAO(new Repository.Models.AiIeltsDbContext());
        }

        public async Task<decimal?> GetLatestSpeakingAnswerScoreAsync()
        {
            var score = await _speakingQuestionDao.GetLastAttemptAsync();
            return score.Grade;
        }

        public async Task<decimal?> GetLatestWritingAnswerScoreAsync()
        {
            var score = await _writingQuestionDao.GetLastAttemptAsync();
            return score.Grade;
        }

        public async Task<decimal?> GetCurrentBandAsync()
        {
            var speakingScores = await _speakingQuestionDao.GetLastFiveAttemptsAsync();
            var writingScores = await _writingQuestionDao.GetLastFiveAttemptsAsync();
            decimal avg = 0;
            foreach (SpeakingAnswer score in speakingScores)
            {
                if (score.Grade != null)
                {
                    avg += score.Grade.Value;
                }
            }

            foreach (WritingAnswer score in writingScores)
            {
                if (score.Grade != null)
                {
                    avg += score.Grade.Value;
                }
            }
            avg /= speakingScores.Count() + writingScores.Count();
            return Math.Round(avg, 1);
        }
    }
}
