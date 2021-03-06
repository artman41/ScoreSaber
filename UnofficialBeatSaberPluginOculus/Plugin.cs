﻿using System;
using IllusionPlugin;
//using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnofficialLeaderBoardPlugin;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.IO;

namespace UnofficialLeaderBoardPlugin
{
    public class Plugin : IPlugin
    {
        public string Name
        {
            get { return "Unofficial Leaderboard by Umbranox"; }
        }

        public string Version
        {
            get { return "0.0.8"; }
        }

        public void OnApplicationStart()
        {
            PlayerPrefs.SetInt("lbPatched", 1);
        }

        public void OnApplicationQuit()
        {

        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public bool loaded = false;
        public void OnLevelWasInitialized(int level)
        {

            if (level != 1) return;
            if (!loaded)
            {
                var leaderBoardsModel = PersistentSingleton<LeaderboardsModel>.instance;
                ReflectionUtil.SetPrivateField(leaderBoardsModel, "_platformLeaderboardsHandler", new CustomPlatformLeaderboardsHandle());
                new CustomOculusPlatform().GetPlayerId(new HMAsyncRequest(), null);
                loaded = true;
            }

        }
   
        
        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {

        }
    }
}