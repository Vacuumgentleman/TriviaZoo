// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TriviaQuizKit
{
	/// <summary>
	/// The screen where the player can select the category to play.
	/// </summary>
	public class CategorySelectionScreen : MonoBehaviour
	{
		public GameConfiguration GameConfiguration;

        public ToggleButtonGroup CategoryToggleGroup;
		public GameObject CategoryTogglePrefab;

        private int selectedCategory;
		private CategoryToggleWrapper anyToggle;

		private void Start()
		{
			if (GameConfiguration != null)
			{
				var i = 0;
				foreach (var category in GameConfiguration.Categories)
				{
					var toggle = CreateCategoryToggle(category.Name, category.Sprite, i);
					CategoryToggleGroup.Buttons.Add(toggle.ToggleButton);
					++i;
				}

				anyToggle = CreateCategoryToggle("Any", GameConfiguration.AnyCategorySprite, -1);
				CategoryToggleGroup.Buttons.Add(anyToggle.ToggleButton);
			}

			SetCategory(PlayerPrefs.GetInt("category"));
            CategoryToggleGroup.SetToggle(selectedCategory != -1 ? selectedCategory : CategoryToggleGroup.Buttons.Count - 1);
		}

        private void SetCategory(int category)
        {
		    selectedCategory = category;
	        PlayerPrefs.SetInt("category", selectedCategory);
        }

		public void OnBackButtonPressed()
		{
			SceneManager.LoadScene("ModeSelection");
		}

		public void OnNextButtonPressed()
		{
			SceneManager.LoadScene("Game");
		}

		private CategoryToggleWrapper CreateCategoryToggle(string categoryName, Sprite categorySprite, int categoryIndex)
		{
			var toggle = Instantiate(CategoryTogglePrefab).GetComponent<CategoryToggleWrapper>();
			toggle.transform.SetParent(CategoryToggleGroup.transform, false);
			toggle.CategoryToggle.CategoryName.text = categoryName;
			toggle.CategoryToggle.CategoryImage.sprite = categorySprite;
			toggle.ToggleButton.ToggleGroup = CategoryToggleGroup;
			toggle.FlatButton.OnPressedEvent.AddListener(() => { SetCategory(categoryIndex); });
			return toggle;
		}
	}
}
