using UnityEngine;

namespace TriviaQuizKit
{
	[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "TriviaZoo/Supabase Config")]
	public class SupabaseConfig : ScriptableObject
	{
		[Tooltip("https://<project>.supabase.co")]
		public string Url = "https://eynyocldtcruwxaynrpo.supabase.co";

		[Tooltip("Settings > API > Project API keys > anon public")]
		public string AnonKey = "";

		public string TableName = "ranking";

		public bool IsValid => !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(AnonKey);
	}
}
