using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TriviaQuizKit
{
	/// <summary>
	/// Servicio de ranking contra Supabase via REST (UnityWebRequest, sin SDK).
	/// Singleton auto-inicializado; no requiere colocarlo en ninguna escena.
	/// </summary>
	public class SupabaseRankingService : MonoBehaviour
	{
		public static SupabaseRankingService Instance { get; private set; }

		private SupabaseConfig config;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Bootstrap()
		{
			if (Instance != null)
			{
				return;
			}

			var go = new GameObject("SupabaseRankingService");
			DontDestroyOnLoad(go);
			Instance = go.AddComponent<SupabaseRankingService>();
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			config = Resources.Load<SupabaseConfig>("SupabaseConfig");
			if (config == null || !config.IsValid)
			{
				Debug.LogWarning("[Supabase] SupabaseConfig ausente o incompleto en Resources. El ranking quedara deshabilitado.");
			}
		}

		public bool IsConfigured => config != null && config.IsValid;

		/// <summary>
		/// Inserta un puntaje. onDone(true) si fue exitoso.
		/// </summary>
		public void SubmitScore(string playerName, int questionType, int category, int score, Action<bool> onDone = null)
		{
			if (!IsConfigured)
			{
				onDone?.Invoke(false);
				return;
			}

			StartCoroutine(SubmitScoreRoutine(playerName, questionType, category, score, onDone));
		}

		/// <summary>
		/// Trae el top de la categoria/tipo dados, ordenado por puntaje desc.
		/// </summary>
		public void GetTop(int questionType, int category, int limit, Action<List<RankingEntry>> onResult)
		{
			if (!IsConfigured)
			{
				onResult?.Invoke(new List<RankingEntry>());
				return;
			}

			StartCoroutine(GetTopRoutine(questionType, category, limit, onResult));
		}

		private IEnumerator SubmitScoreRoutine(string playerName, int questionType, int category, int score, Action<bool> onDone)
		{
			var entry = new RankingEntry
			{
				player_name = playerName,
				question_type = questionType,
				category = category,
				score = score
			};
			var body = JsonUtility.ToJson(entry);

			var url = $"{config.Url}/rest/v1/{config.TableName}";
			using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
			{
				var raw = System.Text.Encoding.UTF8.GetBytes(body);
				request.uploadHandler = new UploadHandlerRaw(raw);
				request.downloadHandler = new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");
				request.SetRequestHeader("apikey", config.AnonKey);
				request.SetRequestHeader("Authorization", $"Bearer {config.AnonKey}");
				request.SetRequestHeader("Prefer", "return=minimal");

				yield return request.SendWebRequest();

				var ok = request.result == UnityWebRequest.Result.Success;
				if (!ok)
				{
					Debug.LogWarning($"[Supabase] SubmitScore fallo: {request.responseCode} {request.error} {request.downloadHandler.text}");
				}
				onDone?.Invoke(ok);
			}
		}

		private IEnumerator GetTopRoutine(int questionType, int category, int limit, Action<List<RankingEntry>> onResult)
		{
			var url = $"{config.Url}/rest/v1/{config.TableName}" +
				$"?select=player_name,question_type,category,score" +
				$"&question_type=eq.{questionType}" +
				$"&category=eq.{category}" +
				$"&order=score.desc&limit={limit}";

			using (var request = UnityWebRequest.Get(url))
			{
				request.SetRequestHeader("apikey", config.AnonKey);
				request.SetRequestHeader("Authorization", $"Bearer {config.AnonKey}");

				yield return request.SendWebRequest();

				var list = new List<RankingEntry>();
				if (request.result == UnityWebRequest.Result.Success)
				{
					var json = request.downloadHandler.text;
					var wrapped = "{\"items\":" + json + "}";
					var parsed = JsonUtility.FromJson<RankingList>(wrapped);
					if (parsed != null && parsed.items != null)
					{
						list.AddRange(parsed.items);
					}
				}
				else
				{
					Debug.LogWarning($"[Supabase] GetTop fallo: {request.responseCode} {request.error}");
				}
				onResult?.Invoke(list);
			}
		}
	}

	[Serializable]
	public class RankingEntry
	{
		public string player_name;
		public int question_type;
		public int category;
		public int score;
	}

	[Serializable]
	public class RankingList
	{
		public RankingEntry[] items;
	}
}
