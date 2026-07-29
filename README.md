MindScape — A Journey Inside the Mind

Gameplay scripts from MindScape, a Unity/C# educational game about mental health. Each level turns a psychological condition into gameplay: symptoms become environmental challenges, and coping strategies become tools the player actively uses. Two levels are fully playable — Generalized Anxiety Disorder and Major Depressive Disorder.

Graduation project, Faculty of Computer Science & Engineering, New Mansoura University, 2026.

> This repository contains the gameplay scripts I wrote. Art, audio, and scene files are excluded — they were produced by teammates or licensed from the Unity Asset Store.

 My contribution

I led a 6-person team and owned the gameplay programming layer. Every script here is mine.

- Designed the three-tier architecture and the persistent / level-specific split
- Wrote the state systems, zone and scene management, coping mechanics, pickups, enemy interactions, and pause/restart
- Defined the extensibility requirement and built the manager layer around it
- Ran the test pass (15 cases) and the playtesting sessions

 Start here

If you only want to read two things:

GameManager.CheckWinLoseConditions() — the design decision, in code. Winning isn't reaching the exit; it's reaching it *in an acceptable psychological state*. Four conditions must hold at once: the player reached the safe spot, Stress ≤ 40, Confidence ≥ 30, and enough Positive Cards were collected. Losing triggers on Stress hitting its maximum **or** the collision count exceeding its limit — two different ways to fail, mapped to two different player mistakes.

GameManager.UpdateHighStressAudio() — runs every frame but fires only on threshold crossings. A highStressAudioOn flag guards the transition so the heartbeat and breathing audio start once when Stress passes 60 and stop once when it drops back, instead of restarting 60 times a second.

 Architecture

Three tiers, so level content never touches core systems.

Persistent (DontDestroyOnLoad)
  GameManager · AudioManager · ZoneManager · PlayerController
        ↕
Level Managers (fresh per scene)
  UIManager (L1) · Level2Manager · Level2UIManager
        ↕
World Objects (prefabs)
  Triggers · Pickups · NPCs · Hazards


The interesting constraint: the two levels are structured differently on purpose.

| | Level 1 — Rush Hour of Worry | Level 2 — The Grey Loop |
| Condition | Generalized Anxiety Disorder | Major Depressive Disorder |
| Structure | Multi-scene — 3 scenes loaded via `ZoneManager` | Single-scene — 3 trigger-based zones |
| Meters | Stress, Confidence (0–100) | Mood, Energy (0–100) |
| Fail states | Stress at max, or 5 collisions | Mood or Energy bottoming out |
| Signature mechanic | Calm Bomb — stress drop plus `Time.timeScale` slow | Passive energy drain, simulating anergia |

The persistent tier has no knowledge of which pattern a level uses; the manager layer absorbs the difference. Adding a level means adding a Scene and a Manager — no changes to existing code. This was set as a requirement at design time and validated by designing Levels 3 and 4 against the same contract.

 Scripts

Persistent

| Script | Responsibility |
| GameManager.cs | Level 1 state — Stress, Confidence, cards, collisions; win/lose evaluation; per-zone card gating |
| AudioManager.cs | Music, SFX, voice lines, heartbeat fade |
| ZoneManager.cs | Scene loading — LoadZone(string sceneName) |
| PlayerController.cs | Movement, jump, card use — Unity New Input System |

Level managers

| Script | Responsibility |
| UIManager.cs | Level 1 HUD, toast messages, win/lose panels |
| Level2Manager.cs | Level 2 state — Mood, Energy, Memory Tokens, Hope Spark |
| Level2UIManager.cs | Level 2 HUD — vignette, bars, gem icons |

World & UI

| Script | Responsibility |
| ZoneTrigger.cs | Level 1 safe-spot detection + scene load |
| ZoneAreaTrigger.cs | Level 2 zone entry — adjusts darkness and spawn settings |
| HeavyFogZone.cs | Energy drain per second while the player is inside |
| BeaconOfHope.cs | Win trigger — evaluates Level 2 stats |
| PositiveCardPickup.cs / MemoryTokenPickup.cs | Collection + voice line + toast + FX |
| NegativeText.cs / ShadowNPC.cs / VehicleController.cs | Enemy collision → meter impact |
| PauseMenuUI.cs | Pause / resume / restart — Time.timeScale management |
| LoseScreenManager.cs | Lose-reason display, retry and menu |

 Results

- 15 test cases— component and integrated gameplay testing, all passing, no critical runtime errors
- Stable above 35 FPS on mid-range hardware; target was 30
- 75–80% level-completion rate, **11–13 min** average sessions in playtesting

Testing sample was small — the team plus two external testers.

 Known limitations

UI reads state by polling rather than events.** The HUD checks meter values each frame instead of subscribing to change notifications. It works at this scale, but an event-driven layer is the first thing I'd refactor.
- The emotion-classification prototype (RandomForest + Flask) is **not connected to the build**. It was trained on synthetic data, so its accuracy figure isn't meaningful — it needs real player data first.
- Levels 3 (OCD) and 4 (ADHD) are designed and documented, not implemented.
- No clinical review by a mental health professional. Mechanics are grounded in DSM-5 descriptions, which isn't the same as validation.
- MindScape is an awareness tool. It is not diagnostic or therapeutic.

 Built with

Unity · C# · Blender · Visual Studio · Git

**Rana Tamer Hamada** — [LinkedIn](https://www.linkedin.com/in/ranat00426) · ranatamer251@gmail.com
