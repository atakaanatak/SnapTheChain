# SnapTheChain
An indie game project demonstrating advanced C# backend mechanics, Event-Driven architecture, and memory management within the Unity Engine.

# Snap the Chain - Clean Architecture & System Design Showcase
**Snap the Chain** is an indie game project developed using the Unity Engine and C#, centered around complex state management and an event-driven architecture. 

This repository has been shared publicly to showcase the game's source code and the applied **Clean Architecture**, **SOLID principles**, and **Memory Management** techniques.
---
### Architecture and Engineering Approach
To solve the "Spaghetti Code" and tight coupling problems frequently encountered in the gaming industry and large-scale software, corporate backend standards have been adopted throughout the project:

# 1. Event-Driven Architecture
Inter-system communication is handled via the `EventManager` using the **Observer Pattern**.
* Objects (e.g., Locks, Portals) fire an event (`Action<LockType>`) instead of directly searching for game managers.
* This ensures that modules are completely isolated from each other (decoupled) and memory leaks are prevented.

# 2. Abstraction & Interface Segregation
All interactable objects are derived from the `BaseInteractable` abstract class. 
* Using polymorphism, it is ensured that the Player only calls the `Interact()` method without needing to know what the specific objects in the system are. (Open/Closed Principle)
* Holdable objects (`HoldableObject`) and trigger-only objects are logically segregated, and business rules are directly encapsulated within the core nature of the objects (Encapsulation).
  
# 3. Asynchronous Memory Management
In-game portal transitions and scene loading are designed to be completely asynchronous (`LoadSceneAsync`).
* To prevent unnecessary memory consumption, old scenes are unloaded from memory (`UnloadSceneAsync`), and the garbage collector is triggered (`Resources.UnloadUnusedAssets`) to ensure RAM optimization.

# 4. Custom Localization Engine (i18n)
Instead of Unity's built-in packages, a customized localization (i18n) engine was written from scratch. It reads CSV files, parses data with **Regex**, and stores the data in a **Dictionary (HashMap)** structure in memory to access keys with $O(1)$ complexity.

# 5. Spatial Mathematics and Illusion Systems
To provide seamless image transfer between dynamic portals, Quaternion mathematics and vectorial delta (difference) calculations were used, successfully integrating Linear Algebra principles into the game world.
---
### Technologies and Concepts Used
* **Language & Engine:** C#, Unity Engine
* **Architectural Principles:** Clean Architecture, SOLID, OOP, Dependency Inversion
* **Design Patterns:** Observer Pattern, State Pattern, Singleton
* **Optimization:** Asynchronous Programming, Memory/Resource Management
* **Version Control:** Git / GitHub
