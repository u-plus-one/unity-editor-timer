using System;
using System.IO;
using UnityEngine;

namespace EditorTimeTracker
{
	[System.Serializable]
	public class TrackedUserTimes
	{
		[System.Serializable]
		public class SavedData
		{
			public string userId;
			public string userName;
			public SessionTimers times;

			public SavedData(UserInfo user, SessionTimers times)
			{
				userId = user.id;
				userName = user.displayName;
				this.times = times;
			}

			public static SavedData FromJson(string json)
			{
				return JsonUtility.FromJson<SavedData>(json);
			}

			public string ToJson()
			{
				return JsonUtility.ToJson(this, true);
			}
		}

		public readonly UserInfo user;
		public SessionTimers storedSample;
		public SessionTimers currentSessionSample;

		public bool IsDirty => currentSessionSample.CombinedTime > 0;

		public bool IsAnon => user.IsEmpty || user.id.ToLower() == "anonymous";

		public float TotalCurrentSessionTime => currentSessionSample.GetTotal(TrackedTimeType.All);

		public string FileLocation
		{
			get
			{
				string name = user.IsEmpty ? "anon" : user.id;
				return Path.Combine(EditorTimeTracker.FileRootDirectory, name + ".json");
			}
		}

		private TrackedUserTimes(UserInfo user)
		{
			this.user = user;
		}

		public static TrackedUserTimes Create(UserInfo user)
		{
			var times = new TrackedUserTimes(user);
			times.Initialize();
			return times;
		}

		public static TrackedUserTimes Load(string filename, out UserInfo user)
		{
			var json = File.ReadAllText(filename);
			var save = SavedData.FromJson(json);
			user = new UserInfo(save.userId, save.userName);
			var times = Create(user);
			times.storedSample = save.times;
			return times;
		}

		public void Initialize()
		{
			currentSessionSample = new SessionTimers();
		}

		public void Increase(float delta)
		{
			currentSessionSample.Increase(delta);
		}

		public void SaveToFile()
		{
			try
			{
				if(!Directory.Exists(EditorTimeTracker.FileRootDirectory))
				{
					Directory.CreateDirectory(EditorTimeTracker.FileRootDirectory);
				}

				var totals = SessionTimers.Combine(currentSessionSample, storedSample);
				var save = new SavedData(user, totals);
				File.WriteAllText(FileLocation, save.ToJson());

				//Restart timers
				storedSample = totals;
				currentSessionSample = new SessionTimers();
			}
			catch(Exception e)
			{
				Debug.LogException(new Exception("Failed to save editor time data for user " + user, e));
			}
		}

		public float GetTotalTime(TrackedTimeType typeFlags = TrackedTimeType.All)
		{
			float t = 0;
			if(storedSample != null) t += storedSample.GetTotal(typeFlags);
			t += currentSessionSample.GetTotal(typeFlags);
			return t;
		}
	}
}
