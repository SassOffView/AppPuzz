# AppPuzz

Gioco puzzle mobile 2D in Unity — Griglia 5×5, parole in italiano/inglese, creatura evolutiva.

## Setup (STEP 1)

### Requisiti
- [Unity Hub](https://unity.com/download)
- Unity **2022.3 LTS** o **6000.0 LTS** (template: **2D**)

### Installazione
1. Clona questo repository:
   ```
   git clone <url-repository>
   cd AppPuzz
   ```
2. Apri **Unity Hub** → **New Project**
   - Template: `2D`
   - Nome: `AppPuzz`
   - Posizione: la cartella clonata (oppure crea il progetto e poi copia la cartella `Assets/`)
3. Imposta orientamento **Portrait**:
   - `Edit` → `Project Settings` → `Player`
   - `Default Orientation` → **Portrait**
4. Installa **TextMeshPro**:
   - `Window` → `Package Manager` → cerca "TextMeshPro" → **Install**
   - Clicca **Import TMP Essentials** quando richiesto
5. Crea la scena principale:
   - Nella finestra `Project`, apri `Assets/Scenes/`
   - Tasto destro → `Create` → `Scene` → rinomina `Gameplay`
   - Doppio clic per aprirla
6. Aggiungi il Canvas UI:
   - Nella `Hierarchy`: tasto destro → `UI` → `Canvas`
   - Seleziona il Canvas → Inspector: `UI Scale Mode` → `Scale With Screen Size`
   - Reference Resolution: `1080 × 1920`
   - `Screen Match Mode` → `Match Width Or Height` → slider a `0.5`

## Struttura Cartelle
```
Assets/
├── Scripts/
│   ├── Grid/          ← GridManager.cs, LetterCell.cs
│   ├── Gameplay/      ← WordValidator.cs, EnergyManager.cs, GameManager.cs
│   ├── Creatures/     ← CreatureController.cs, EvolutionConfig.cs
│   ├── Localization/  ← LanguageManager.cs
│   └── Utils/         ← WeightedRandom.cs
├── Prefabs/           ← LetterCell.prefab, Creature.prefab
├── UI/                ← HUD.prefab
│   └── Fonts/
├── Dictionaries/      ← italian.json, english.json, fantasy_shared.json
├── Creatures/         ← sprite creatura
├── Materials/
└── Scenes/            ← Gameplay.unity
```

## Setup (STEP 3) — Scena Unity

> **Prerequisito**: TMP installato (Window → Package Manager → TextMeshPro → Import TMP Essentials + Examples).
> Dopo l'import, aspetta che Unity finisca di ricompilare (indicatore in basso a destra).

### 1. Crea la scena
- `Assets/Scenes/` → tasto destro → Create → Scene → rinomina `Gameplay`
- Doppio clic per aprirla

### 2. Crea la gerarchia nel Canvas
```
[Canvas]
  ├── [GridPanel]         ← UI › Panel, poi aggiungi GridLayoutGroup
  ├── [UI_Overlay]
  │     ├── CurrentWordText    ← UI › Text - TextMeshPro
  │     ├── EnergyBar          ← UI › Slider
  │     └── WordSelectorOverlay ← UI › Image (alpha = 0)
  │                                 + Add Component: WordSelector
  └── [Managers]          ← GameObject vuoto (Ctrl+Shift+N)
        ├── GameManager        ← Add Component: GameManager
        ├── GridManager        ← Add Component: GridManager
        ├── EnergyManager      ← Add Component: EnergyManager
        └── LanguageManager    ← Add Component: LanguageManager
```

### 3. Configura GridPanel
- Inspector → `GridLayoutGroup`:
  - Cell Size: `180 × 180`
  - Spacing: `10 × 10`
  - Constraint: `Fixed Column Count` = 5
- `RectTransform`: centra il panel nella scena

### 4. Crea il Prefab LetterCell
1. Hierarchy → tasto destro → UI → Image → rinomina `LetterCell`
2. Aggiungi figlio: UI → Text - TextMeshPro → rinomina `LetterText`
3. Seleziona `LetterCell` → Add Component → `LetterCell` (script)
4. Nell'Inspector assegna:
   - `Letter Text` → trascina `LetterText` (TextMeshPro)
   - `Background Image` → trascina l'Image del `LetterCell` stesso
5. Trascina `LetterCell` dalla Hierarchy in `Assets/Prefabs/` → salva come Prefab
6. Cancella l'oggetto dalla Hierarchy (ora è solo un prefab)

### 5. Assegna i riferimenti Inspector

| Seleziona | Campo | Trascina |
|---|---|---|
| `GridManager` | `letterCellPrefab` | Prefab `LetterCell` |
| `GridManager` | `gridContainer` | GameObject `GridPanel` |
| `GameManager` | `gridManager` | GameObject `GridManager` |
| `GameManager` | `energyManager` | GameObject `EnergyManager` |
| `WordSelector` | `currentWordText` | `CurrentWordText` (TMP) |
| `EnergyManager` | `energyBar` | `EnergyBar` (Slider) |

### 6. Verifica WordSelectorOverlay
- `WordSelectorOverlay` deve coprire **tutto il GridPanel** (usa RectTransform = stretch)
- `Image` → `Color` → alpha a `0` (trasparente ma intercetta i tocchi)
- `Raycast Target` → deve essere **abilitato** (spunta ✅)

---

## Struttura Cartelle
```
Assets/
├── Scripts/
│   ├── Grid/          ← GridManager.cs, LetterCell.cs
│   ├── Gameplay/      ← WordValidator.cs, EnergyManager.cs, GameManager.cs, WordSelector.cs
│   ├── Creatures/     ← CreatureController.cs, EvolutionConfig.cs
│   ├── Localization/  ← LanguageManager.cs
│   └── Utils/         ← WeightedRandom.cs
├── Prefabs/           ← LetterCell.prefab, Creature.prefab
├── UI/
│   └── Fonts/
├── Dictionaries/      ← (copia di backup)
├── Resources/
│   └── Dictionaries/  ← italian.json, english.json, fantasy_shared.json  ← usati da Resources.Load
├── Creatures/         ← sprite creatura
├── Materials/
└── Scenes/            ← Gameplay.unity
```

## Roadmap Step-by-Step
- [x] **STEP 1** — Setup & Scena
- [x] **STEP 2** — Griglia 5×5 & Lettere Ponderate
- [x] **STEP 3** — Selezione & Costruzione Parola ← *sei qui*
- [ ] STEP 4 — Dizionari & Validazione
- [ ] STEP 5 — Energia & Bonus
- [ ] STEP 6 — Creatura & Evoluzione
- [ ] STEP 7 — Timer & Flusso Partita
- [ ] STEP 8 — Supporto Base
