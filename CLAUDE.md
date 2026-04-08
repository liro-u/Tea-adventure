# Tea Adventure — Project Guide

## What is this project?

Tea Adventure is a cooperative multiplayer game (non-competitive) built in Unity 6. Players explore an open world, engage in combat, pilot vehicles, and interact with the environment (moving objects, etc.). The emphasis is on fun co-op play, not ranked competition.

**Tech stack:**
- Unity 6
- Unity CharacterController, Animator, Unity Input System
- Unity Netcode for GameObjects (NGO) with Relay for networking
- Client-side prediction + server reconciliation for all controllable entities (movement, vehicles, etc.)
- HFSM (Hierarchical Finite State Machine) for all entity logic

---

## Architecture Overview

### Folder philosophy

```
Assets/
  CharacterController/       # Reusable package: offline + online character system
    Offline/                 # Pure local controller (no networking)
    Online/                  # Network wrapper around the offline controller
  Scripts/                   # Game-specific code
    Network/                 # Relay, session management
    UI/                      # UI logic
    Debug/                   # Debug utilities
```

Code that could be reused in another project lives in its own **package folder** (e.g. `CharacterController/`, future `VehicleController/`).  
Game-specific code lives under `Scripts/`.  
Split by concern — one concept per file, small files are the goal.

### The Brain / Motor / State Machine pattern

Every controllable entity follows this layered structure:

```
InputProvider  →  Brain  →  StateMachine (HFSM)  →  Motor
```

| Layer | Responsibility |
|---|---|
| **InputProvider** | Reads raw device or network input. Produces an `IInputPayload`. Never contains logic. |
| **Brain** | Wires everything together (MonoBehaviour). Owns the Motor, StateMachine, InputProvider. |
| **StateMachine / States** | Pure logic. Reads the payload, writes to `IBrainStatePayload`. No Unity API calls except through Motor. |
| **Motor** | Executes physics. Owns `CharacterController` / Rigidbody. Exposes `Velocity`, `Position`, `IsGrounded`, etc. |
| **Data (ScriptableObjects)** | Tuning values only. States read from SO data, never write to it. |

---

## Coding Rules

### 1. Input NEVER touches logic

`IInputProvider` collects raw values and exposes a payload. It does **not** decide what the character does. Transition conditions live in states, not in the input layer.

```csharp
// CORRECT — input just stores the value
private void OnJumpInput(InputAction.CallbackContext context)
    => inputPayload.IsJumping = context.ReadValueAsButton();

// WRONG — input deciding behaviour
private void OnJumpInput(InputAction.CallbackContext context)
{
    if (context.ReadValueAsButton()) stateMachine.Transition(JumpState);
}
```

### 2. HFSM states are pure

States receive context through the `IBrainStatePayload` and the `IBrain`. They must not cache Unity component references directly — use the Brain or Motor interfaces.  
A state must be fully testable without a running scene.

```csharp
// CORRECT
public override void Enter() => brain.Motor.ResetVerticalVelocity();

// WRONG
public override void Enter() => GetComponent<CharacterController>().Move(...);
```

### 3. Networking wraps, does not own logic

The Online layer (`NetworkCharacterBrain`, `MovementClientPrediction`) wraps the offline brain. All movement/combat/vehicle simulation logic lives in the offline system. The network layer only adds:
- Tick management
- Input serialization / deserialization
- Client prediction + server reconciliation

Never duplicate simulation logic in the network layer.

### 4. Client prediction is required for all controllable entities

Every entity the local player controls (character, vehicle, carried object) must use the `ClientPrediction<TInput, TState>` base class:
- The client simulates immediately on input
- The server runs the same simulation authoritatively
- On receiving a server state, the client checks for divergence and reconciles (replays inputs) if needed
- Reconciliation threshold is defined per-feature (e.g. position error > 0.001f for movement)
- Input and state payloads must be structs and network-serializable

### 5. Separate concerns in folders and files

One concept = one file. If a class grows beyond ~150 lines, consider splitting it.  
Group by feature, then by role within the feature:

```
FeatureName/
  Scripts/
    Input/
    Brain/
    Movement/States/
    Data/ScriptableObjects/
    Animation/
```

### 6. Small files by default

Prefer many small, focused files over large monolithic ones. Exceptions are allowed only when splitting would create meaningless fragmentation (e.g., a single-method helper that truly belongs with its owner).

### 7. Reusable code lives in package folders

If a system could ship as a standalone package (character controller, vehicle controller, state machine base, client prediction base), put it in its own top-level folder under `Assets/`. Keep it free of game-specific references.

---

## Planned Features (context for future work)

- **Combat system** — melee and/or ranged, follows the same Brain/State pattern, with client prediction
- **Vehicle controller** — separate package folder, same Input→Brain→Motor pattern, online wrapper with client prediction
- **Object interaction** — picking up / moving objects in the world
- **Exploration** — open world traversal, no loading screens between areas if possible

---

## Networking Notes

- Unity 6 + Unity Netcode for GameObjects
- Transport: Unity Relay (via `RelayManager`)
- Authority model: server-authoritative with client prediction
- `ClientPrediction<TInput, TState>` is the generic base — extend it for each controllable type
- Input payloads must be structs and network-serializable (INetworkSerializable or unmanaged)
- Reconciliation: client replays all buffered inputs after a mismatch with server state

---

## Naming Conventions

| Thing | Convention | Example |
|---|---|---|
| Interfaces | `I` prefix | `IInputProvider<T>`, `IMotor` |
| State classes | `*State` suffix | `JumpingState`, `SprintingState` |
| Brain classes | `*Brain` suffix | `CharacterBrain`, `NetworkCharacterBrain` |
| Data SO fields | PascalCase | `GroundedData.GroundLayer` |
| Input payloads | `*InputPayload` | `MovementInputPayload` |
| State payloads | `*BrainStatePayload` | `MovementBrainStatePayload` |

---

## What to avoid

- Do not add `[SerializeField]` fields to states — pass data through the Brain/Motor interfaces.
- Do not put `MonoBehaviour` in Motor or State classes.
- Do not let network code call physics directly — route through the offline Brain.
- Do not create helper utilities for one-off operations — inline them.
- Do not add comments that restate what the code already says clearly.
- Do not skip client prediction for any player-controlled entity — latency hiding is a core requirement.
