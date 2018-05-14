using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
namespace UnofficialLeaderBoardPlugin
{
    public class CustomPlatformLeaderboardsHandle : PlatformLeaderboardsHandler
    {
        public override void GetPlayerId(HMAsyncRequest asyncRequest, LeaderboardsModel.GetPlayerIdCompletionHandler completionHandler)
        {
            CallNonStaticFunctionDynamically("OculusPlatformLeaderboardsHandler", "Assembly-CSharp", "GetPlayerId",
                       new Type[] { typeof(HMAsyncRequest), typeof(LeaderboardsModel.GetScoresCompletionHandler) },
                              new object[] {  asyncRequest, completionHandler });
        }
       
        #region CustomScoreUI
        public override void GetScores(string leaderboadId, int count, int fromRank, LeaderboardsModel.ScoresScope scope, string referencePlayerId, HMAsyncRequest asyncRequest, LeaderboardsModel.GetScoresCompletionHandler completionHandler)
        {
            var leaderBoardsModel = PersistentSingleton<LeaderboardsModel>.instance;
            if (leaderboadId.Contains("∎"))
            {
                leaderboadId = FormatLeaderBoard(leaderboadId);

                switch (scope)
                {
                    case LeaderboardsModel.ScoresScope.AroundPlayer:
                        GetCustomScoreBehaviour.GetScore("http://scoresaber.com/getscores.php?id=" + leaderboadId + "&steamId=" + Global.playerId, completionHandler, leaderboadId, asyncRequest, OnGetScore);
                        break;
                    case LeaderboardsModel.ScoresScope.Global:
                        GetCustomScoreBehaviour.GetScore("http://scoresaber.com/getscores.php?id=" + leaderboadId, completionHandler, leaderboadId, asyncRequest, OnGetScore);
                        break;
                }
            }
            else
            {
                try
                {
                    CallNonStaticFunctionDynamically("OculusPlatformLeaderboardsHandler", "Assembly-CSharp", "GetScores",
                        new Type[] { typeof(string), typeof(int), typeof(int), typeof(LeaderboardsModel.ScoresScope),
                                typeof(string), typeof(HMAsyncRequest), typeof(LeaderboardsModel.GetScoresCompletionHandler) },
                               new object[] { leaderboadId, count, fromRank, scope, referencePlayerId, asyncRequest, completionHandler });
                }
                catch (Exception ex)
                {
                    Global.Log(ex.ToString());
                }
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
                    OkCompletionHandler(completionHandler, leaders.ToArray(), -1, asyncRequest);
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
                    CallNonStaticFunctionDynamically("OculusPlatformLeaderboardsHandler", "Assembly-CSharp", "UploadScore",
                       new Type[] { typeof(string), typeof(int), typeof(HMAsyncRequest), typeof(LeaderboardsModel.UploadScoreCompletionHandler)},
                              new object[] { leaderboadId, score, asyncRequest, completionHandler });
                }
            }
            catch (Exception ex)
            {
               Global.Log(ex.ToString());
            }
        }
        private void PrepareCustomScore(string leaderBoard, int score)
        {
            try
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
                string steamId = Global.playerId;
                new Thread(() =>
                {
                    //UploadScore Function redacted for security
                }).Start();
            }
            catch (Exception ex)
            {
               // Global.Log(ex.ToString());
            }

        }

        #endregion

        #region Helpers
        private void CallNonStaticFunctionDynamically(string functionClass, string dependency, string function, Type[] methodSig, object[] parameters)
        {
            Type FunctionClass = Type.GetType(string.Format("{0},{1}", functionClass, dependency));
            if (FunctionClass != null)
            {
                object FunctionClassInstance = Activator.CreateInstance(FunctionClass);
                if (FunctionClassInstance != null)
                {
                    Type InstanceType = FunctionClassInstance.GetType();
                    MethodInfo Function = InstanceType.GetMethod(function, methodSig);
                    if (Function != null)
                    {
                        Function.Invoke(FunctionClassInstance, parameters);
                    }
                }
            }
        }
        private List<LeaderboardsModel.LeaderboardScore> PassLeaderBoardInfo(string scoreData, ref int playerScoreIndex)
        {
            List<LeaderboardsModel.LeaderboardScore> leaders = new List<LeaderboardsModel.LeaderboardScore>();
            try
            {
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

                        if (steamId == Global.playerId)
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
        private string FormatLeaderBoard(string leaderboard)
        {
            string difficulty = leaderboard.Split('∎')[5].Replace("Expert+", "ExpertPlus");
            leaderboard = leaderboard.Split('∎')[0];
            return "lb_" + leaderboard + difficulty;
        }
     
        #endregion
    }
}