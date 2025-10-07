namespace EditorTimeTracker
{
	public struct UserInfo
	{
		public readonly string id;
		public readonly string displayName;

		public bool IsEmpty => string.IsNullOrEmpty(id);

		public UserInfo(string id, string displayName)
		{
			this.id = id;
			this.displayName = displayName;
		}

		public UserInfo(string id)
		{
			this.id = id;
			displayName = id;
		}

		public override int GetHashCode()
		{
			return !IsEmpty ? id.GetHashCode() : 0;
		}
	}
}