# The Adventures of Curry Man  
### Vertical Slice

This repository contains the **original gameplay vertical slice** for *The Adventures of Curry Man*, implemented in **C#**.

This slice represents the point at which the game’s **core combat philosophy, player freedom, and systemic interactions were proven to work in practice**. Everything here exists to answer one question:

> Does the game feel right when played?

The answer to that question determined whether the project moved forward.

---

## What This Vertical Slice Represents

This is not a tutorial project, a tech demo, or a collection of experiments.

This vertical slice was built to validate:
- Free-form combo-driven combat
- Player-first control and responsiveness
- Enemy pressure without restricting player expression
- Readable combat state (hit, stun, KO, super)
- A complete gameplay loop from spawn → fight → win/lose → reset

Once these systems worked together coherently, the slice was considered successful.

---

## Design Focus

The vertical slice prioritizes **behavior over abstraction**.

Key principles:
- Player expression comes from **timing and flow**, not rigid combo trees
- Combat difficulty comes from **enemies**, not input restrictions
- Systems are explicit and readable
- Minimal automation; logic is spelled out
- Animation and gameplay remain tightly coupled

The code reflects a willingness to be direct where clarity matters.

---

## Systems Overview

### Core Gameplay
- Player movement and rotation
- Free-form punch/kick combo system
- Input buffering and anti-spam logic
- Combat state locking and release

### Combat & Damage
- Animation-driven hit detection
- Health and KO handling
- Hit reactions and death flow
- Super activation and damage scaling

### AI
- Distance-based chase logic
- Attack timing and facing control
- Clean handoff between movement and combat
- Fail-safes to prevent animation deadlocks

### Feedback & Presentation
- Camera follow and bounds
- Camera shake for impact
- Dynamic focus / depth control
- Centralized audio playback

### Game Flow
- Splash screen flow
- Win/lose detection
- Scene reset and restart handling

---

## Scope & Limitations

This vertical slice:
- Focuses on **one gameplay scenario**
- Uses placeholder assets where appropriate
- Optimizes for feel, not extensibility
- Does not attempt to solve long-term content scaling

Those concerns were intentionally deferred until the core experience proved itself.

---

## What This Is (and Isn’t)

**This is**:
- A validated gameplay foundation
- Proof of combat feel and player freedom
- A complete, playable slice of the game

**This is not**:
- A finished product
- A general-purpose framework
- A content-complete build

---

## Notes for Reviewers

- Code favors clarity over cleverness
- Some systems are intentionally verbose to keep behavior visible
- Decisions here reflect the needs of a vertical slice, not final architecture

---

## Author

Fish  
Game Developer / Gameplay Programmer  

Vertical Slice – *The Adventures of Curry Man*
