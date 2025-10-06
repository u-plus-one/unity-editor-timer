using System;
using System.Reflection;

namespace EditorTimeTracker
{
	public static class UserUtility
	{
		private static object unityConnect;
		private static Type unityConnectType;
		private static PropertyInfo userInfoProperty;
		private static PropertyInfo userIdProperty;
		private static PropertyInfo userNameProperty;

		public static UserInfo GetCurrentUserInfo()
		{
			if(unityConnect == null)
			{
				var assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
				unityConnect = assembly.CreateInstance("UnityEditor.Connect.UnityConnect", false, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
				unityConnectType = unityConnect?.GetType();
				userInfoProperty = unityConnectType?.GetProperty("userInfo", BindingFlags.Public | BindingFlags.Instance);
				userIdProperty = userInfoProperty?.PropertyType.GetProperty("userId", BindingFlags.Public | BindingFlags.Instance);
				userNameProperty = userInfoProperty?.PropertyType.GetProperty("displayName", BindingFlags.Public | BindingFlags.Instance);
			}
			var info = userInfoProperty.GetValue(unityConnect);
			var id = userIdProperty.GetValue(info) as string;
			var name = userNameProperty.GetValue(info) as string;
			return new UserInfo(id, name);

			/*
			string id = CloudProjectSettings.userId;
			if(!string.IsNullOrWhiteSpace(id)) return id;
			else return $"_{Environment.UserName}";
			*/
		}
	}
}