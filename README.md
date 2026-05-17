# 🏆 pick-me-up

Welcome to the official repository for **pick-me-up**, a premium, beautifully crafted, and highly polished idle/gacha mobile RPG built in **Unity (6.0 / 6000.0.74f1)**.

This game is designed to offer a fluid, gorgeous, and warning-free landscape gameplay experience optimized for modern mobile targets, specifically profile-mapped for the **Samsung Galaxy A34 (19.5:9 aspect ratio, 2340x1080 resolution)**.

---

## 📂 Repository Structure

The project has evolved through three iterative developmental phases:

* **[prototype-0.1](./prototype-0.1)** — The foundational scaffolding, event systems, and initial roster mockups.
* **[prototype-0.2](./prototype-0.2)** — Advanced roster system integrations, Gacha pulls, and initial battle systems.
* **[prototype-0.3](./prototype-0.3) (Latest & Current)** — The definitive polished version featuring high-contrast bold layouts, cross-scene fade transitions, zero-warning font optimizations, and dynamic navigation buttons.

---

## 📝 Developer Handoff Documents

For a complete and highly detailed breakdown of the latest systems, fixes, and architecture, refer to the handover documentation:

* **[Root Handoff Document](./handover.md)** — Core architectures, dynamic button wiring fallbacks, persistent canvases, and TMPro warning resolutions.

---

## ⚡ Key Highlights of Prototype 0.3

1. **🎨 High-Visibility Landscape layouts:** All headers, stats, levels, and button texts are scale-optimized with premium weights (`FontStyles.Bold`) and increased sizing (`fontSize = 20` for button labels) for maximum clarity.
2. **🌟 Zero-Warning Star Rating System:** We resolved the TextMeshPro unicode star rating trap by using universal ASCII standard indicators (`*` for active, `-` for empty). **Your console is now completely free of millions of missing-character warnings!**
3. **🔒 Bulletproof Dynamic Navigation:** Programmatically wired Back buttons and utility buttons in each script's `Start()` method to bypass Unity Editor serialization dropping, ensuring a 100% bug-free user journey.
4. **🔄 Seamless Fade Transitions:** Persistent `BootstrapCanvas` parenting under `_GameManager` ensures smooth, cross-scene fade overlays.

---

## 🕹️ Quick Start Guide

To launch and run the project in the Unity Editor:

1. Open the project folder in **prototype-0.3** in Unity 6.
2. Allow Unity to complete script compilation.
3. Select **`Tools` ➔ `Generate UI Prefabs`** from the top menu to construct the UI cards.
4. Select **`Tools` ➔ `Build and Wire All Scenes`** from the top menu to automatically generate and wire all 8 gameplay scenes.
5. Press **Play**! The game is completely warning-free, crisp, and fully functional!
