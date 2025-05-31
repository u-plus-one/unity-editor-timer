using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTimeTracker
{
	public static class EditorTimeTracker
	{
		private const float MIN_SAVE_INTERVAL = 60;

		internal static Dictionary<string, TrackedUserTimes> users = new Dictionary<string, TrackedUserTimes>();

		private static double lastCheckTime;
		private static double lastSaveTime;

		//TODO: find a way to toggle this feature on and off
		public static bool Enabled => true;

		internal static string FileRootDirectory => Path.Combine(Directory.GetCurrentDirectory(), "EditorTimes");

		public static float GetTotalTime(string userId, TrackedTimeType typeFlags = TrackedTimeType.All)
		{
			if(users.TryGetValue(userId, out var u))
			{
				return u.GetTotalTime(typeFlags);
			}
			return 0;
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			EditorApplication.update += Update;
			SceneView.duringSceneGui += DuringSceneGui;
			AssemblyReloadEvents.beforeAssemblyReload += OnDestroy;
			EditorApplication.quitting += OnDestroy;
			//Save data when the project is changed
			EditorApplication.projectChanged += () => Save(false);
		}

		private static void DuringSceneGui(SceneView sv)
		{
			var mousePos = Event.current.mousePosition;
		}

		private static void OnDestroy()
		{
			Save(false);
		}

		private static void Update()
		{
			if(Enabled && lastCheckTime != 0)
			{
				float delta = (float)(EditorApplication.timeSinceStartup - lastCheckTime);
				string user = GetUserName();
				if(!users.ContainsKey(user))
				{
					users.Add(user, TrackedUserTimes.Create(user));
				}
				users[user].Increase(delta);
			}
			lastCheckTime = EditorApplication.timeSinceStartup;
			
		}

		private static string GetUserName()
		{
			string id = CloudProjectSettings.userId;
			if(!string.IsNullOrWhiteSpace(id)) return id;
			else return $"_{Environment.UserName}";
		}

		private static void Save(bool force)
		{
			if((EditorApplication.timeSinceStartup - lastSaveTime) < MIN_SAVE_INTERVAL && !force)
			{
				//We already saved not too long ago, don't save again
				return;
			}
			foreach(var user in users.Values)
			{
				if(user.IsDirty)
				{
					//Only save anonymous user times if they exceed 180 seconds in total
					if(user.IsAnon && user.TotalCurrentSessionTime < 180) continue;
					user.SaveToFile();
				}
			}
			lastSaveTime = EditorApplication.timeSinceStartup;
		}
	}
}
