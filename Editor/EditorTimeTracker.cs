using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTimeTracker
{
	public static class EditorTimeTracker
	{
		public enum State
		{
			Enabled = 0,
			DisabledUntilRestart = 1,
			Disabled = 2
		}

		private const float MIN_SAVE_INTERVAL = 10;

		public static State TrackingState
		{
			get
			{
				int i = EditorPrefs.GetInt("TimeTrackingState", 0);
				return (State)i;
			}
			set
			{
				EditorPrefs.SetInt("TimeTrackingState", (int)value);
			}
		}

		internal static Dictionary<UserInfo, TrackedUserTimes> users = new Dictionary<UserInfo, TrackedUserTimes>();

		public static TrackedUserTimes CurrentUser
		{
			get
			{
				var user = UserUtility.GetCurrentUserInfo();
				if(!users.ContainsKey(user))
				{
					Debug.Log("Creating new user: " + user.id);
					users.Add(user, TrackedUserTimes.Create(user));
				}
				return users[user];
			}
		}

		public static bool EditorIsFocussed => UnityEditorInternal.InternalEditorUtility.isApplicationActive;

		private static double lastCheckTime;
		private static double lastSaveTime;

		//TODO: find a way to toggle this feature on and off
		public static bool Enabled => TrackingState == State.Enabled;

		internal static string FileRootDirectory => Path.Combine(Directory.GetCurrentDirectory(), "EditorTimes");

		public static float GetTotalTime(UserInfo user, TrackedTimeType typeFlags = TrackedTimeType.All)
		{
			if(users.TryGetValue(user, out var u))
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
			LoadAll();
			if(!SessionState.GetBool("TimeTrackerFirstInit", false))
			{
				SessionState.SetBool("TimeTrackerFirstInit", true);
				if(TrackingState == State.DisabledUntilRestart)
				{
					//Enable tracker after restart
					TrackingState = State.Enabled;
					Debug.Log("Time tracking restarted");
				}
			}
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
				CurrentUser.Increase(delta);
			}
			lastCheckTime = EditorApplication.timeSinceStartup;
			
		}

		private static void LoadAll()
		{
			foreach(var file in Directory.GetFiles(FileRootDirectory, "*.json"))
			{
				UserInfo user;
				TrackedUserTimes data;
				try
				{
					data = TrackedUserTimes.Load(file, out user);
				}
				catch(Exception e)
				{
					Debug.LogException(new Exception("Failed to load editor time data from file " + file, e));
					continue;
				}
				try
				{
					users.Add(user, data);
				}
				catch(Exception e)
				{
					Debug.LogException(e);
				}
			}
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
