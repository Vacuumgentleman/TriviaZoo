# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TriviaZoo is a Unity-based trivia quiz game built on the Trivia Quiz Kit framework. It's a 2D game developed with Unity 6000.3.8f1 using the Universal Render Pipeline (URP). The project features multiple game modes, question types, and player progression tracking.

**Key Tech Stack:**
- Unity 6000.3.8f1 / C# 11 / URP 17.3.0
- TextMesh Pro for UI text rendering
- Input System 1.18.0

## Architecture & Systems

### 1. Game Configuration System

- `GameConfiguration.cs` - Central ScriptableObject holding categories, question count, scoring rules, time limits, prefab references, and trophy thresholds
- Loaded at runtime via `GameConfigurationLoader.cs` using `Resources.Load("GameConfiguration")` — this resolves to `Assets/TriviaQuizKit/Resources/GameConfiguration.asset`
- `Assets/TriviaZoo/Configuracion/TriviaZoo_GC.asset` is the project-specific configuration asset; `Assets/TriviaZoo/Configuracion/Preguntas/Preguntas_v1.asset` is the active question pack and `Set_P_v1.asset` is its containing `QuestionPackSet`

### 2. Question System

**Type hierarchy (all ScriptableObjects):**
- `BaseQuestion` → `SingleChoiceQuestion`, `MultipleChoiceQuestion`, `TrueFalseQuestion`
- All types support: multiple category membership, optional image, metadata field

**Loading pipeline:**
- `QuestionPack` bundles questions → referenced via `QuestionPackSet.PreloadedQuestionPacks`
- `QuestionPackLoader` filters by selected `QuestionType` enum and category index at runtime

### 3. Scene Flow

Four scenes in `Assets/TriviaZoo/Scenes/`, always loaded in this order:

| Scene | Controller | Purpose |
|---|---|---|
| Home.unity | `HomeScreen.cs` | Avatar/profile selection |
| ModeSelection.unity | `GameModeSelectionScreen.cs` | Question type (Single/Multiple/TrueFalse/Any) |
| CategorySelection.unity | `CategorySelectionScreen.cs` | Category toggle selection |
| Game.unity | `GameScreen.cs` | Core game loop |

**Inter-scene state is passed entirely via PlayerPrefs** (see keys below). Scenes configure themselves by reading these keys on Start.

### 4. Game Screen (Core Logic)

`GameScreen.cs` manages the full gameplay loop:

- **Question selection:** Loads and filters by type/category, randomizes. When all questions are exhausted, the used list is recycled (shuffled and returned to available), enabling infinite play.
- **Question UI:** Dynamically instantiates one of 6 prefabs (3 types × with/without image). The `QuestionOrder` field (Randomized/Test) and `QuestionPackLoad` field (All/Single) are configurable in the Inspector.
- **Result feedback:** `QuestionResultUi.cs` — coroutine-based fade-in/out overlay showing correct/wrong text after each answer (2-second display).
- **Timer:** Limited mode uses a countdown + `Image.fillAmount` indicator. Timeout auto-submits as wrong.
- **Game end:** Trophy (Bronze/Silver/Gold) assigned by comparing `numCorrectAnswers` against `GameConfiguration` thresholds. High score and trophy persisted per `{questionType}_{category}` key.

### 5. UI System

**Base classes:**
- `BaseScreen.cs` — popup stack management, semi-transparent overlay panels, `Resources.LoadAsync` for async popup loading, coroutine-chained fade animations
- `Popup.cs` — Animator-driven open/close with `OnOpen`/`OnClose` UnityEvents; destroyed after close animation delay
- `QuestionUi.cs` — abstract base for answer button layout and result display; extended by `SingleChoiceQuestionUi`, `MultipleChoiceQuestionUi`, `TrueFalseQuestionUi`

**Prefab load paths at runtime:**
- Question UIs: `Resources/QuestionUI/`
- Popups: `Resources/Popups/` (Alert, GameFinished, Profile, QuitGame, Settings)

### 6. Audio System

`SoundManager.cs` — singleton, maps sound names → AudioClips, uses `ObjectPool<SoundFx>` for pooled AudioSources. Respects `sound_enabled` / `music_enabled` PlayerPrefs. `BackgroundMusic.cs` handles the background track separately. `PlaySound.cs` is a UnityEvent-friendly wrapper component.

### 7. Utility Scripts

- `SceneTransition.cs` — `SceneManager.LoadScene()` wrapper for buttons
- `Initialization.cs` — sets PlayerPrefs defaults on Awake (first-run setup)
- `ListShuffle.cs` — question randomization
- `ToggleButtonGroup.cs` / `ToggleButton.cs` — mutually exclusive toggle controls
- `FlatButton.cs` — button with `OnPressedEvent` UnityEvent
- `SpriteSwapper.cs` — toggles a UI `Image` between two predefined sprites

### 8. Editor Tools

Access via **Tools > Trivia Quiz Kit** menu:
- **Editor** — `GameConfigurationTab` (categories, scoring, prefab refs), `QuestionsTab` (CRUD for questions)
- **Delete PlayerPrefs** — clears all PlayerPrefs; useful when testing fresh-start flows
- **Delete EditorPrefs** — clears editor preferences
- `GameScreenInspector.cs` — custom Inspector for GameScreen

## PlayerPrefs Keys

| Key | Type | Description |
|---|---|---|
| `sound_enabled` | 0/1 | Sound effects toggle |
| `music_enabled` | 0/1 | Background music toggle |
| `player_avatar` | int | Avatar index |
| `question_type` | int | `QuestionType` enum value |
| `category` | int | Category index; -1 = Any |
| `time_mode` | int | `TimeMode` enum value |
| `trophy_{questionType}_{category}` | int | 0=none, 1=bronze, 2=silver, 3=gold |
| `score_{questionType}_{category}` | int | High score per type+category combination |

## Development Workflow

**Opening the project:** Unity 6000.3.8f1. Solution file is `TriviaZoo.slnx`. `.vscode/settings.json` is pre-configured to treat `.asset`, `.prefab`, `.unity`, and `.meta` files as YAML.

**Build scene order:** Home → ModeSelection → CategorySelection → Game (File > Build Settings).

**Third-party tools:** `Assets/ThirdParty/PlayModeComponentSaver/` — saves and restores component values modified during Play Mode back to the scene, preventing work loss.

### Adding a New Question
1. Create a ScriptableObject of the desired question type
2. Fill in Question, Categories, Answers (and optional Image)
3. Add to `Preguntas_v1.asset` (the active QuestionPack)
4. Verify `Set_P_v1.asset` references the pack (or use the QuestionsTab editor)

### Modifying Game Configuration
Edit `Assets/TriviaZoo/Configuracion/TriviaZoo_GC.asset` directly, or use **Tools > Trivia Quiz Kit > Editor > GameConfigurationTab**.

### Creating Screens / Popups
- **Screen:** New MonoBehaviour extending `BaseScreen`; attach to scene Canvas
- **Popup:** New MonoBehaviour extending `Popup`; create prefab with Animator in `Resources/Popups/`; call `BaseScreen.OpenPopup("PrefabName")`

### Modifying Question UI
Edit prefabs in `Assets/TriviaQuizKit/Resources/QuestionUI/`. All 6 variants (3 types × with/without image) must stay in sync. If adding new prefab types, update the `GameObject` references in `GameConfiguration`.

## Debugging Tips

- **Questions not appearing:** Confirm `QuestionPackSet` is assigned in `GameConfiguration.PreloadedQuestionPacks`; verify question categories match the selected filter
- **UI buttons not responding:** Check `CanvasGroup.blocksRaycasts` on any popup overlay panel
- **Sound not playing:** Confirm `sound_enabled = 1` in PlayerPrefs; verify `SoundManager.Instance` is not null
- **Wrong trophy awarded:** Check `NumQuestionsNeededFor{Bronze,Silver,Gold}` thresholds in `GameConfiguration`
- **Reset game state during testing:** Use **Tools > Trivia Quiz Kit > Delete PlayerPrefs**

**Key debug entry points:**
- `GameScreen.OnPlayerAnswered()` — answer validation
- `GameScreen.SelectRandomQuestion()` — question selection/filtering
- `SoundManager.PlaySound()` — audio system

## Key Packages

- `com.unity.render-pipelines.universal` (17.3.0) — URP graphics
- `com.unity.ugui` (2.0.0) — Canvas/UI
- `com.unity.textmeshpro` — text rendering
- `com.unity.inputsystem` (1.18.0) — input handling
- `com.unity.timeline` (1.8.10) — animation support
