﻿using UnityEngine;

namespace EditorTimeTracker
{
	[System.Serializable]
	public class SessionTimers
	{
		public float activeEditTime = 0;
		public float unfocussedEditTime = 0;
		public float playmodeTime = 0;
		public float inactiveTime = 0;

		public float CombinedTime => activeEditTime + unfocussedEditTime + playmodeTime + inactiveTime;

		public void Increase(float delta)
		{
			if(InactivityDetector.UserIsInactive)
			{
				inactiveTime += delta;
			}
			else
			{
				if(Application.isPlaying)
				{
					playmodeTime += delta;
				}
				else
				{
					if(EditorTimeTracker.EditorIsFocussed) activeEditTime += delta;
					else unfocussedEditTime += delta;
				}
			}
		}

		public float GetTotal(TrackedTimeType types)
		{
			float t = 0;
			if(types.HasFlag(TrackedTimeType.ActiveEditorTime)) t += activeEditTime;
			if(types.HasFlag(TrackedTimeType.UnfocusedEditorTime)) t += unfocussedEditTime;
			if(types.HasFlag(TrackedTimeType.PlaymodeTime)) t += playmodeTime;
			if(types.HasFlag(TrackedTimeType.InactiveTime)) t += inactiveTime;
			return t;
		}

		public static SessionTimers Combine(SessionTimers a, SessionTimers b)
		{
			if(b == null)
			{
				return new SessionTimers()
				{
					activeEditTime = a.activeEditTime,
					unfocussedEditTime = a.unfocussedEditTime,
					playmodeTime = a.playmodeTime,
					inactiveTime = a.inactiveTime
				};
			}
			else
			{
				return new SessionTimers()
				{
					activeEditTime = a.activeEditTime + b.activeEditTime,
					unfocussedEditTime = a.unfocussedEditTime + b.unfocussedEditTime,
					playmodeTime = a.playmodeTime + b.playmodeTime,
					inactiveTime = a.inactiveTime + b.inactiveTime
				};
			}
		}
	}
}
