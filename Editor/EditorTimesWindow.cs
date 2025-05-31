using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorTimeTracker
{
	internal class EditorTimesWindow : EditorWindow
	{
		private Vector2 scrollPosition;

		[MenuItem("Window/Editor Times")]
		public static void ShowWindow()
		{
			var window = GetWindow<EditorTimesWindow>("Editor Times");
			window.Show();
		}

		private void OnGUI()
		{
			//GUILayout.Label("Editor Time Tracker", EditorStyles.largeLabel);

			GUILayout.Space(10);
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
				foreach(var kv in EditorTimeTracker.users)
				{
					bool isLocalUser = CloudProjectSettings.userId == kv.Key;
					GUI.contentColor = isLocalUser ? new Color(0.3f, 1f, 0.3f) : Color.white;
					GUILayout.Space(10);
					GUILayout.BeginVertical(GUI.skin.box);
					var user = kv.Value;
					GUILayout.Label("User: " + kv.Key, EditorStyles.boldLabel);
					GUILayout.Space(5);
					EditorGUILayout.LabelField("Total", ToTimeString(user.GetTotalTime()), EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Total (active)", ToTimeString(user.GetTotalTime(TrackedTimeType.AllActive)), EditorStyles.boldLabel);
					GUILayout.Space(5);
					EditorGUILayout.LabelField("Unfocussed Editor Time", ToTimeString(user.GetTotalTime(TrackedTimeType.UnfocusedEditorTime)));
					EditorGUILayout.LabelField("Playmode Time", ToTimeString(user.GetTotalTime(TrackedTimeType.PlaymodeTime)));
					EditorGUILayout.LabelField("Inactive Time", ToTimeString(user.GetTotalTime(TrackedTimeType.InactiveTime)));
					GUILayout.EndVertical();
				}
			}
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
