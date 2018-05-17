using System;
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
        
        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {

        }
        
        public bool loaded = false;
        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {
            if (scene.buildIndex != 1) return;
            if (!loaded)
            {
                var leaderBoardsModel = PersistentSingleton<LeaderboardsModel>.instance;
                ReflectionUtil.SetPrivateField(leaderBoardsModel, "_platformLeaderboardsHandler", new CustomSteamPlatformLeaderboardsHandler());
                loaded = true;
            }
        }
        
        public void OnSceneUnloaded(Scene scene) {
            
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) {
            
        }
    }
}