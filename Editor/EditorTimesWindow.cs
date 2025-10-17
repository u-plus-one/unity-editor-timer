using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorTimeTracker
{
	internal class EditorTimesWindow : EditorWindow
	{
		private Vector2 scrollPosition;

		[MenuItem("Window/Time Tracking")]
		public static void ShowWindow()
		{
			var window = GetWindow<EditorTimesWindow>("Editor Times");
			window.Show();
		}

		private void OnGUI()
		{
			EditorGUIUtility.labelWidth = 200;
			GUILayout.Label("Editor Time Tracker", EditorStyles.largeLabel);

			GUILayout.Space(10);
			string state = EditorTimeTracker.TrackingState == EditorTimeTracker.State.Enabled ? "Enabled"
				: EditorTimeTracker.TrackingState == EditorTimeTracker.State.DisabledUntilRestart ? "Disabled Until Restart"
				: "Disabled";
			if(!EditorTimeTracker.Enabled) GUI.contentColor = new Color(1, 0.25f, 0.25f);
			EditorGUILayout.LabelField("State", state, EditorStyles.boldLabel);
			GUI.contentColor = Color.white;
			GUILayout.BeginHorizontal();
			if(EditorTimeTracker.TrackingState == EditorTimeTracker.State.Enabled)
			{
				if(GUILayout.Button("Disable")) EditorTimeTracker.TrackingState = EditorTimeTracker.State.Disabled;
				if(GUILayout.Button("Disable until Restart")) EditorTimeTracker.TrackingState = EditorTimeTracker.State.DisabledUntilRestart;
			}
			else
			{
				if(GUILayout.Button("Enable")) EditorTimeTracker.TrackingState = EditorTimeTracker.State.Enabled;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(30);
			float total = EditorTimeTracker.users.Sum(kv => kv.Value.GetTotalTime());
			EditorGUILayout.LabelField("All Users Total", ToTimeString(total), EditorStyles.boldLabel);
			float totalActive = EditorTimeTracker.users.Sum(kv => kv.Value.GetTotalTime(TrackedTimeType.AllActive));
			EditorGUILayout.LabelField("All Users Total (active)", ToTimeString(totalActive), EditorStyles.boldLabel);

			GUILayout.Space(10);
			GUILayout.Label("User Times", EditorStyles.boldLabel);
			GUILayout.Space(5);
			using(var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
			{
				scrollPosition = scrollView.scrollPosition;
				GUI.contentColor = new Color(0.3f, 1f, 0.3f);
				var current = EditorTimeTracker.CurrentUser;
				DrawUser(current);
				GUI.contentColor = Color.white;
				foreach(var kv in EditorTimeTracker.users)
				{
					var t = kv.Value;
					if(t == current)
					{
						//Skip current user
						continue;
					}
					GUILayout.Space(10);
					DrawUser(kv.Value);
				}
			}

			if(!Application.isPlaying && EditorTimeTracker.EditorIsFocussed) Repaint();
		}

		private static void DrawUser(TrackedUserTimes times)
		{
			var user = times.user;
			GUILayout.BeginVertical(GUI.skin.box);
			string name = user.IsEmpty ? "(Anonymous)" : user.displayName;
			GUILayout.Label("User: " + name, EditorStyles.boldLabel);
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Total", ToTimeString(times.GetTotalTime()), EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Total (active)", ToTimeString(times.GetTotalTime(TrackedTimeType.AllActive)), EditorStyles.boldLabel);
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Unfocussed Editor Time", ToTimeString(times.GetTotalTime(TrackedTimeType.UnfocusedEditorTime)));
			EditorGUILayout.LabelField("Active Editor Time", ToTimeString(times.GetTotalTime(TrackedTimeType.ActiveEditorTime)));
			EditorGUILayout.LabelField("Playmode Time", ToTimeString(times.GetTotalTime(TrackedTimeType.PlaymodeTime)));
			EditorGUILayout.LabelField("Inactive Time", ToTimeString(times.GetTotalTime(TrackedTimeType.InactiveTime)));
			GUILayout.EndVertical();
		}

		private static string ToTimeString(float t)
		{
			int seconds = Mathf.RoundToInt(t);
			var stringBuilder = new StringBuilder(8);
			bool positive = seconds >= 0;
			stringBuilder.Append(positive ? "" : "-");
			seconds = Mathf.Abs(seconds);
			string sec = (seconds % 60).ToString("D2");
			string min = (seconds / 60 % 60).ToString("D2");
			string hrs = (seconds / 3600).ToString("D2");
			stringBuilder.Append(hrs);
			stringBuilder.Append(":");
			stringBuilder.Append(min);
			stringBuilder.Append(":");
			stringBuilder.Append(sec);
			var output = stringBuilder.ToString();
			stringBuilder.Clear();
			return output;
		}
	}
}
