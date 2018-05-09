using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace UnofficialLeaderBoardPlugin
{
    public class CustomSteamPlatformLeaderboardsHandler : SteamPlatformLeaderboardsHandler
    {
 
        public override void UploadScore(string leaderboadId, int score, HMAsyncRequest asyncRequest, LeaderboardsModel.UploadScoreCompletionHandler completionHandler)
        {
            if (leaderboadId.Contains("∎"))
            {
                string text = "lb_" + leaderboadId;
                if (PlayerPrefs.HasKey(text))
                {
                    if (PlayerPrefs.GetInt(text) < score)
                    {
                        this.PrepareCustomScore(text, score);
                        PlayerPrefs.SetInt(text, score);
                    }
                }
                else
                {
                    this.PrepareCustomScore(text, score);
                    PlayerPrefs.SetInt(text, score);
                }
                completionHandler(LeaderboardsModel.UploadScoreResult.OK);
                return;
            }
            else
            {
                base.UploadScore(leaderboadId, score, asyncRequest, completionHandler);
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
    }
}
