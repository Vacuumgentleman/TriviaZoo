using System;
using UnityEngine;

namespace TriviaQuizKit
{
	/// <summary>
	/// Acceso centralizado al nombre del jugador (PlayerPrefs "player_name").
	/// </summary>
	public static class PlayerProfile
	{
		public const string NameKey = "player_name";

		/// <summary>
		/// Devuelve el nombre guardado o genera uno por defecto tipo "Jugador#260628X".
		/// El generado se persiste para que sea estable entre sesiones.
		/// </summary>
		public static string GetOrCreateName()
		{
			var name = PlayerPrefs.GetString(NameKey, string.Empty);
			if (!string.IsNullOrWhiteSpace(name))
			{
				return name;
			}

			name = $"Jugador#{DateTime.Now:yyMMdd}{UnityEngine.Random.Range(0, 10)}";
			PlayerPrefs.SetString(NameKey, name);
			PlayerPrefs.Save();
			return name;
		}

		public static void SetName(string name)
		{
			PlayerPrefs.SetString(NameKey, name == null ? string.Empty : name.Trim());
			PlayerPrefs.Save();
		}
	}
}
