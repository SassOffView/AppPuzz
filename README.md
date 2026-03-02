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

## Roadmap Step-by-Step
- [x] **STEP 1** — Setup & Scena ← *sei qui*
- [ ] STEP 2 — Griglia 5×5 & Lettere Ponderate
- [ ] STEP 3 — Selezione & Costruzione Parola
- [ ] STEP 4 — Dizionari & Validazione
- [ ] STEP 5 — Energia & Bonus
- [ ] STEP 6 — Creatura & Evoluzione
- [ ] STEP 7 — Timer & Flusso Partita
- [ ] STEP 8 — Supporto Base
