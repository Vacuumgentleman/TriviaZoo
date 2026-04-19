# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TriviaZoo is a Unity-based trivia quiz game built on the Trivia Quiz Kit framework. It's a 2D game developed with Unity 6000.3.8f1 using the Universal Render Pipeline (URP). The project features multiple game modes, question types, and player progression tracking.

**Key Tech Stack:**
- Unity 6000.3.8f1
- C# 11
- Universal Render Pipeline (URP)
- TextMesh Pro for UI text rendering
- Input System 1.18.0

## Architecture & Systems

### Core Systems

#### 1. Game Configuration System
- **ScriptableObject-based configuration** stored in `Assets/TriviaQuizKit/Resources/GameConfiguration.asset`
- `GameConfiguration.cs` - Central configuration holding:
  - Game categories and sprites
  - Question count, scoring rules, time limits
  - Prefab references for different question UI types
  - Trophy thresholds (Bronze/Silver/Gold)
- Loaded via `GameConfigurationLoader.cs` at runtime using `Resources.Load()`

#### 2. Question System
**Base hierarchy:**
- `BaseQuestion` - Abstract base for all question types (ScriptableObjects)
- `SingleChoiceQuestion` - Single correct answer
- `MultipleChoiceQuestion` - Multiple correct answers from a set
- `TrueFalseQuestion` - Binary true/false questions

All question types support:
- Categories (can belong to multiple)
- Optional images
- Metadata field for additional info

**Question Management:**
- Questions bundled into `QuestionPack.cs` (ScriptableObjects in Resources/)
- `QuestionPackLoader.cs` loads either single or all question packs
- Questions are filtered by:
  - Selected question type (Single/Multiple/TrueFalse/Any)
  - Selected category (or "Any" for all categories)

#### 3. Game Flow (Scene Architecture)

Four main scenes in `Assets/TriviaZoo/Scenes/`:
1. **Home.unity** - Main menu with player profile/avatar selection
   - `HomeScreen.cs` - Manages avatar display and settings popups
2. **ModeSelection.unity** - Game mode/question type selection
   - `GameModeSelectionScreen.cs` - Selects question type (Single/Multiple/TrueFalse/Any)
3. **CategorySelection.unity** - Category selection via toggle buttons
   - `CategorySelectionScreen.cs` - Builds category toggles from GameConfiguration
4. **Game.unity** - Main gameplay scene
   - `GameScreen.cs` - Core game loop controller

**Data Flow via PlayerPrefs:**
- `player_avatar` - Selected avatar index
- `question_type` - Selected question type (enum)
- `category` - Selected category index (-1 = Any)
- `time_mode` - Limited or Unlimited time (enum)
- Trophy and score persistence: `trophy_{questionType}_{category}`, `score_{questionType}_{category}`

#### 4. Game Screen (Core Logic)

`GameScreen.cs` orchestrates gameplay:
- **Question Selection:** Loads available questions, filters by type/category, randomizes order
- **Question UI Management:** Dynamically instantiates appropriate question UI variant:
  - 6 question UI prefabs (3 types × with/without image)
  - Selected based on question type and presence of image
- **Scoring:** Tracks correct/wrong answers, calculates score per question
- **Time Management:**
  - Limited mode: Countdown timer with visual fill indicator, auto-fails on timeout
  - Unlimited mode: No timer UI shown
- **Answer Flow:**
  - Player submits answer → `OnPlayerAnswered()` validates
  - Shows result animation (2 seconds)
  - Advances to next question or finishes game
- **Game End:** Trophy assignment (Bronze/Silver/Gold) based on correct answer count

#### 5. UI System

**Base Classes:**
- `BaseScreen.cs` - Abstract base for all screens
  - Popup management (stack-based with semi-transparent panels)
  - Fade-in/fade-out animations for popups
  - Resources.LoadAsync for async popup loading

- `Popup.cs` - Abstract base for popups (Inspector-driven animations)
  - OnOpen/OnClose UnityEvents
  - Animator-driven close animation with destroy delay

- `QuestionUi.cs` - Abstract base for question UI variants
  - Implementations: `SingleChoiceQuestionUi`, `MultipleChoiceQuestionUi`, `TrueFalseQuestionUi`
  - Each variant manages answer button layout and result display

**UI Prefab Structure:**
- Question UI prefabs loaded dynamically at runtime from Resources/QuestionUI/
- Popup prefabs loaded from Resources/Popups/ (AlertPopup, GameFinishedPopup, etc.)
- All UI uses Canvas and RectTransform for responsive layout

#### 6. Audio System

`SoundManager.cs` - Singleton pattern:
- Maps sound names to AudioClips via dictionary
- Uses `ObjectPool.cs` for SoundFx instances (pooled AudioSources)
- Respects PlayerPrefs sound/music enabled flags
- Called by GameScreen for feedback: "Correct", "Incorrect" sounds
- `BackgroundMusic.cs` - Separate component for background track

**Object Pooling:**
`ObjectPool.cs` - Generic pooling system:
- Configurable initial pool size
- Lazy creation if pool exhausted
- Used for: SoundFx prefabs

#### 7. Utility Systems

- **SceneTransition.cs** - Scene loading via `SceneManager.LoadScene()`
- **Initialization.cs** - Awake-time setup for PlayerPrefs defaults
- **ListShuffle.cs** - Question randomization utility
- **ToggleButtonGroup.cs/ToggleButton.cs** - Category/mode selection UI controls
- **FlatButton.cs** - Button wrapper with OnPressedEvent UnityEvent

### Editor Tools

`Tools/Trivia Quiz Kit/Editor` menu window provides:
- **GameConfigurationTab** - Configure game settings (categories, scoring, UI prefabs)
- **QuestionsTab** - CRUD operations for questions (category assignment, answers)
- **AboutTab** - Kit information
- Inspector custom editor: `GameScreenInspector.cs`

## File Organization

```
Assets/
├── TriviaQuizKit/              # Core framework (licensed Asset Store code)
│   ├── Scripts/
│   │   ├── Core/               # Base classes, managers, utilities
│   │   ├── Game/
│   │   │   ├── Logic/          # Question types, game configuration, loaders
│   │   │   └── UI/
│   │   │       ├── Screens/    # Scene controllers
│   │   │       ├── Widgets/    # Question UI, category toggles
│   │   │       └── Popups/     # Popup implementations
│   │   └── Editor/             # Editor window and tools
│   ├── Resources/              # Runtime loadable assets
│   │   ├── Popups/
│   │   ├── QuestionUI/
│   │   ├── GameConfiguration.asset
│   │   └── QuestionPackSet.asset
│   ├── Prefabs/                # Reusable UI components
│   ├── Sounds/                 # Audio clips
│   └── Sprites/                # UI sprites
└── TriviaZoo/                  # Project-specific content
    ├── Scenes/                 # Game scenes (Home, ModeSelection, CategorySelection, Game)
    ├── Configuracion/
    │   ├── Preguntas/          # QuestionPack and GameConfiguration assets
    │   └── TriviaZoo_GC.asset  # Main game configuration
    ├── Sprites/                # Game-specific sprites
    └── Music/                  # Background music
```

## Development Workflow

### Building & Testing

**Unity Editor Build:**
- Open project in Unity 6000.3.8f1
- Build via File > Build Settings (configure scenes in order: Home → ModeSelection → CategorySelection → Game)
- Project uses Universal Render Pipeline, ensure URP asset is assigned in Graphics settings

**Assembly Files:**
- `Assembly-CSharp.csproj` - Main game code
- `Assembly-CSharp-Editor.csproj` - Editor tools
- Can be opened in Visual Studio Code/Visual Studio (configured in .vscode/settings.json)

### Key Workflows

**Adding a New Question:**
1. Create ScriptableObject instance of desired question type
2. Fill in Question, Categories, Answers (and optional Image)
3. Add to QuestionPack asset
4. Pack referenced in GameConfiguration.PreloadedQuestionPacks

**Modifying Game Configuration:**
1. Edit TriviaZoo_GC.asset directly, or
2. Use Tools > Trivia Quiz Kit > Editor window (GameConfigurationTab)
3. Configure categories, scoring rules, time limits, trophy thresholds

**Creating Custom Screens/Popups:**
1. Create new MonoBehaviour inheriting from `BaseScreen` or `Popup`
2. For screens: Implement scene and attach to main Canvas
3. For popups: Create prefab in Resources/Popups/, prefab must have Popup component with Animator

**Modifying Question UI:**
1. Edit prefabs in Assets/TriviaQuizKit/Resources/QuestionUI/
2. Ensure all 6 variants (3 types × with/without image) are kept in sync
3. Update GameConfiguration references if adding new UI prefabs

### PlayerPrefs Keys (Gameplay State)

Persistent player data stored via PlayerPrefs:
- `sound_enabled` (0/1) - Sound effects toggle
- `music_enabled` (0/1) - Background music toggle
- `player_avatar` (int) - Avatar index selection
- `question_type` (int) - QuestionType enum value
- `category` (int) - Category index (-1 = Any)
- `time_mode` (int) - TimeMode enum value
- `trophy_{questionType}_{category}` (int) - Trophy level (0=none, 1=bronze, 2=silver, 3=gold)
- `score_{questionType}_{category}` (int) - High score for combination

## Important Design Patterns

### Dynamic UI Instantiation
GameScreen dynamically instantiates UI prefabs at runtime rather than pre-populating scenes. This allows flexible question type handling without cluttering scenes.

### PlayerPrefs as Session State
Game state between scenes is passed via PlayerPrefs. Scenes reference these keys to configure themselves (e.g., CategorySelectionScreen reads `category` to set initial toggle).

### Question Randomization with Renewal
When available questions exhaust, the used list is recycled (shuffle). This allows infinite play without question repetition until all questions seen.

### Async Resource Loading
Popups and scenes use Resources.LoadAsync to avoid frame hiccups. BaseScreen chains coroutines for popup fade animations.

### ScriptableObject-Driven Configuration
All game content (questions, categories, settings) is in ScriptableObjects, allowing designers to configure without code changes (via Inspector or Editor window).

## Dependencies

**Core Unity Packages:**
- com.unity.inputsystem (1.18.0) - Input handling
- com.unity.render-pipelines.universal (17.3.0) - URP graphics
- com.unity.ugui (2.0.0) - Canvas/UI system
- com.unity.textmeshpro - Text rendering
- com.unity.timeline (1.8.10) - Animation support

**Optional/Development:**
- com.unity.ide.rider, com.unity.ide.visualstudio - IDE integration
- com.unity.test-framework (1.6.0) - Unit testing (if needed)

## Debugging Tips

**Common Issues:**
- Questions not appearing: Check QuestionPackSet is referenced in GameConfiguration, and question categories match selected filters
- UI buttons not responding: Verify Canvas is present and PopupPanel doesn't block raycasts (check CanvasGroup.blocksRaycasts)
- Sound not playing: Check PlayerPrefs `sound_enabled` is 1, and SoundManager Instance is found
- Incorrect trophy awarded: Verify NumQuestionsNeededFor{Bronze,Silver,Gold} thresholds in GameConfiguration

**Useful Debug Points:**
- GameScreen.OnPlayerAnswered() - Answer validation logic
- GameScreen.SelectRandomQuestion() - Question selection/filtering
- SoundManager.PlaySound() - Audio system entry point
