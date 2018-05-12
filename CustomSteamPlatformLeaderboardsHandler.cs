using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace UnofficialLeaderBoardPlugin
{
    public class CustomSteamPlatformLeaderboardsHandler : SteamPlatformLeaderboardsHandler
    {

        #region CustomScoreUI
        public override void GetScores(string leaderboadId, int count, int fromRank, LeaderboardsModel.ScoresScope scope, string referencePlayerId, HMAsyncRequest asyncRequest, LeaderboardsModel.GetScoresCompletionHandler completionHandler)
        {
            if (leaderboadId.Contains("∎"))
            {
                leaderboadId = FormatLeaderBoard(leaderboadId);

                switch (scope)
                {
                    case LeaderboardsModel.ScoresScope.AroundPlayer:
                        GetCustomScoreBehaviour.GetScore("http://scoresaber.com/getscores.php?id=" + leaderboadId + "&steamId=" + SteamUser.GetSteamID().m_SteamID.ToString(), completionHandler, leaderboadId, asyncRequest, OnGetScore);
                        break;
                    case LeaderboardsModel.ScoresScope.Global:
                        GetCustomScoreBehaviour.GetScore("http://scoresaber.com/getscores.php?id=" + leaderboadId, completionHandler, leaderboadId, asyncRequest, OnGetScore);
                        break;
                }
            }
            else
            {
                base.GetScores(leaderboadId, count, fromRank, scope, referencePlayerId, asyncRequest, completionHandler);
            }
        }
        private void OnGetScore(byte[] data, LeaderboardsModel.GetScoresCompletionHandler completionHandler, string leaderboadID, HMAsyncRequest asyncRequest)
        {
            string scoreData = System.Text.Encoding.UTF8.GetString(data);

            bool cancelRequest = false;
            if (asyncRequest != null)
            {
                asyncRequest.CancelHandler = delegate(HMAsyncRequest request)
                {
                    cancelRequest = true;
                };
            }
            int playerScoreIndex = -1;
            List<LeaderboardsModel.LeaderboardScore> leaders = PassLeaderBoardInfo(scoreData, ref playerScoreIndex);

            if (leaders.Count > 0)
            {
                if (cancelRequest == false)
                {
                    OkCompletionHandler(completionHandler, leaders.ToArray(), playerScoreIndex, asyncRequest);
                }
                else
                {
                    FailCompletionHandler(completionHandler, asyncRequest);
                }
            }
            else
            {
                FailCompletionHandler(completionHandler, asyncRequest);
            }
        }

        private void OkCompletionHandler(LeaderboardsModel.GetScoresCompletionHandler handler, LeaderboardsModel.LeaderboardScore[] leaders, int playerScoreIndex, HMAsyncRequest request)
        {
            if (request != null)
            {
                if (handler != null)
                {
                    handler(LeaderboardsModel.GetScoresResult.OK, leaders.ToArray(), playerScoreIndex);
                }
            }
        }
        private void FailCompletionHandler(LeaderboardsModel.GetScoresCompletionHandler handler, HMAsyncRequest request)
        {
            if (request != null)
            {
                if (handler != null)
                {
                    handler(LeaderboardsModel.GetScoresResult.Failed, new LeaderboardsModel.LeaderboardScore[0], 0);
                }
            }

        }
        #endregion

        #region Uploading
        public override void UploadScore(string leaderboadId, int score, HMAsyncRequest asyncRequest, LeaderboardsModel.UploadScoreCompletionHandler completionHandler)
        {
            try
            {
                if (leaderboadId.Contains("∎"))
                {
                    string text = "lb_" + leaderboadId;

                    this.PrepareCustomScore(text, score);
                    if (completionHandler != null)
                    {
                        completionHandler(LeaderboardsModel.UploadScoreResult.OK);
                    }

                    return;
                }
                else
                {
                    base.UploadScore(leaderboadId, score, asyncRequest, completionHandler);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }
        private void PrepareCustomScore(string leaderBoard, int score)
        {
            string[] array = leaderBoard.Split(new char[]
	        {
		        '∎'
	        });
            string leaderboardId = array[0];
            string songName = array[1];
            string songSubName = array[2];
            string authorName = array[3];
            string bpm = array[4];
            string diff = array[5];
            string steamId = SteamUser.GetSteamID().m_SteamID.ToString();
            UploadCustomScore(leaderBoard, songName, songSubName, authorName, bpm, diff, steamId);
        }
        private void UploadCustomScore(string leaderboardId, string songName, string songSubName, string authorName, string bpm, string diff, string steamid)
        {
            //ScoreUploaderRedactedForSecurity
        }
        #endregion

        #region Helpers
        private string FormatLeaderBoard(string leaderboard)
        {
            string difficulty = leaderboard.Split('∎')[5].Replace("Expert+", "ExpertPlus");
            leaderboard = leaderboard.Split('∎')[0];
            return "lb_" + leaderboard + difficulty;
        }
        private void Log(string data)
        {

            File.AppendAllText(@"LeaderBoardPluginLog.txt", data + Environment.NewLine);
        }
        private LevelStaticData.Difficulty GetDifficultyFromString(string diff)
        {
            switch (diff)
            {
                case "Easy":
                    return LevelStaticData.Difficulty.Easy;
                case "Normal":
                    return LevelStaticData.Difficulty.Normal;
                case "Hard":
                    return LevelStaticData.Difficulty.Hard;
                case "Expert":
                    return LevelStaticData.Difficulty.Expert;
                case "ExpertPlus":
                    return LevelStaticData.Difficulty.ExpertPlus;
            }
            return LevelStaticData.Difficulty.Expert;
        }
        //Probably the most disgusting function in existence, but it gets the job done
        private List<LeaderboardsModel.LeaderboardScore> PassLeaderBoardInfo(string scoreData, ref int playerScoreIndex)
        {
            List<LeaderboardsModel.LeaderboardScore> leaders = new List<LeaderboardsModel.LeaderboardScore>();
            try
            {
                string mySteamId = SteamUser.GetSteamID().m_SteamID.ToString();
                if (scoreData != string.Empty)
                {
                    var decodedScoreData = SimpleJSON.JSON.Parse(scoreData);
                    for (int i = 0; i < decodedScoreData.Count; i += 3)
                    {
                        string rawId = decodedScoreData[i + 1];
                        string steamId = string.Empty;
                        string name = string.Empty;
                        if (rawId.Contains("|"))
                        {
                            steamId = rawId.Split('|')[0];
                            name = rawId.Split('|')[1];
                        }
                        else
                        {
                            steamId = rawId;
                        }

                        int rank = decodedScoreData[i];
                        int score = decodedScoreData[i + 2];

                        if (steamId == mySteamId)
                        {
                            playerScoreIndex = i / 3;
                        }

                        leaders.Add(new LeaderboardsModel.LeaderboardScore(score, rank, name, steamId));
                    }
                    return leaders;
                }
                return leaders;
            }
            catch (Exception ex)
            {
                return leaders;
            }

        }
        #endregion
    }
}
