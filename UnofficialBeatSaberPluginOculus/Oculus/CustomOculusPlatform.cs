using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace UnofficialLeaderBoardPlugin
{
    class CustomOculusPlatform : OculusPlatformLeaderboardsHandler
    {
        public override void GetPlayerId(HMAsyncRequest asyncRequest, LeaderboardsModel.GetPlayerIdCompletionHandler completionHandler)
        {
            Request<User> oculusRequest = Users.GetLoggedInUser().OnComplete(delegate(Message<User> message)
            {
                if (!this.CheckMessageForValidRequest(message))
                {
                    return;
                }
                if (message.IsError)
                {
                    if (completionHandler != null)
                    {
                        completionHandler(LeaderboardsModel.GetPlayerIdResult.Failed, null);
                    }
                }
                else
                {
                    string playerId = message.Data.ID.ToString();
                   
                    string playerName = message.Data.OculusID;
                    Global.playerId = playerId;
                    Global.playerName = playerName;
                    if (completionHandler != null)
                    {
                        completionHandler(LeaderboardsModel.GetPlayerIdResult.OK, playerId);
                    }
                }
            });
            this.AddOculusRequest(oculusRequest, asyncRequest);
        }
    }
}