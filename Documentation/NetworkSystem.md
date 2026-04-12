# Network System

## How it works

Every controllable entity (character, future vehicle, etc.) runs its simulation twice: once on the client immediately when input happens, and once on the server authoritatively. The server periodically sends its result back to the client. If they differ, the client rewinds to the diverged tick and replays its buffered inputs forward. From the player's perspective, movement feels instant with no input lag.

---

## The tick loop

`WorldSimulation` is a MonoBehaviour that owns a list of entities and calls `SimulateTick(dt)` on all of them every `FixedUpdate`. Nothing else should have a `FixedUpdate` — everything goes through this single driver.

`NetworkWorldSimulation` extends it with reconciliation. Before each tick it snapshots state, after the tick it checks for server corrections, and if any entity diverged it rewinds and replays.

```
FixedUpdate (NetworkWorldSimulation)
  ├── SaveState(tick)         for each reconcilable
  ├── Tick()                  SimulateTick on every entity
  ├── NeedsReconciliation?    for each reconcilable
  │     yes → RestoreState(rewindTick)
  │           replay ticks rewindTick..currentTick
  └── currentTick++
```

The two interfaces that plug into this system:
- `ISimulatableEntity` — anything that has a `SimulateTick(dt)`. Registered with `WorldSimulation`.
- `IReconcilableEntity` — adds `SaveState`, `RestoreState`, `NeedsReconciliation`. Registered with `NetworkWorldSimulation` on the owning client only.

---

## ClientPrediction

`ClientPrediction<TInput, TState>` implements both interfaces. It wraps any `ISimulatable<TInput, TState>` and adds all the network bookkeeping:

- **Input ring buffer** — 128 slots indexed by `tick % 128`. Stores the input used at each tick so replay can reproduce the same result.
- **State ring buffer** — same size. Stores the simulated state after each tick. Each slot records its own tick number so stale entries can be detected.
- **Server input queue** — server side only. Stores inputs received from the client indexed by tick. Falls back to the last known input on miss.
- **Correction handling** — client side only. Compares the server's state against the local buffer. If `CheckDivergence` returns true, triggers a rewind.

The `NetworkBehaviour` wrapper owns a `ClientPrediction` instance, sets the two transport callbacks (`OnSendInput`, `OnSendStateCorrection`), and calls `RegisterWithReconciliation` / `Register` in `OnNetworkSpawn`.

---

## File and folder separation

Each controllable entity type has two folders:

```
FeatureName/
  Offline/          # Pure C#, no NGO dependency
    Scripts/
      Brain/        # *BrainCore — pure C# class, owns StateMachine + Motor
      ...
  Online/           # NGO wrapper, minimal code
    Scripts/
      Brain/        # *NetworkBrain (NetworkBehaviour) + *NetworkBrainCore
```

The rule: **all simulation logic lives Offline**. The Online folder only contains the `NetworkBehaviour` shell, the RPC methods, and the `ClientPrediction` wiring. If you find yourself writing movement or physics code in an Online file, it belongs Offline instead.

---

## Developing an offline feature (do this first)

Before thinking about networking, implement the feature entirely offline. Follow this structure:

**1. Define the input payload**
A plain struct. Fields are raw input values — no logic, no decisions.
```csharp
public struct MyInputPayload
{
    public Vector2 Move;
    public bool Jump;
}
```

**2. Define the state snapshot**
A plain struct containing everything needed to fully restore the entity's state.
```csharp
public struct MyStateSnapshot
{
    public Vector3 Position;
    public Vector3 Velocity;
}
```

**3. Implement ISimulatable**
The core class (pure C#, no MonoBehaviour) implements `ISimulatable<TInput, TState>`:
```csharp
public class MyBrainCore : ISimulatable<MyInputPayload, MyStateSnapshot>
{
    public MyStateSnapshot SimulateTick(float dt, MyInputPayload input) { ... }
    public void ApplyState(MyStateSnapshot state) { ... }
}
```

**4. Wrap in a MonoBehaviour**
The `*Brain` MonoBehaviour creates the core, registers with `WorldSimulation`, and drives `OnUpdate` for per-frame non-simulation work (camera, animation smoothing, etc.).
```csharp
protected virtual void Start() => Brain.Register(WorldSimulation.Instance);
private void OnDestroy()       => Brain.Unregister(WorldSimulation.Instance);
```

**Tips for easy future conversion:**
- Keep `SimulateTick` deterministic — same inputs must always produce the same output.
- Never read `Time.time` or `Time.deltaTime` inside `SimulateTick` — use the `dt` parameter.
- Make the state snapshot complete. If restoring it doesn't fully reproduce the entity's behaviour on the next tick, the reconciliation replay will drift.
- Keep the input payload flat and small — it will be sent over the network every tick.

---

## Converting an offline feature to online

Assume the offline feature is already working and follows the structure above.

**1. Define network-serializable versions of the payloads**
Add `INetworkSerializable` to both structs (or make them unmanaged if they contain only blittable fields).
```csharp
public struct MyInputPayload : INetworkSerializable
{
    public Vector2 Move;
    public bool Jump;
    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref Move);
        s.SerializeValue(ref Jump);
    }
}
```

**2. Create the NetworkBrainCore**
Extend the offline `*BrainCore`, create a `ClientPrediction` instance, and set `CheckDivergence`.
```csharp
public class MyNetworkBrainCore : MyBrainCore
{
    public readonly ClientPrediction<MyInputPayload, MyStateSnapshot> Prediction;

    public MyNetworkBrainCore(NetworkBehaviour owner, /* ...same args as base... */)
        : base(/* ...args... */)
    {
        Prediction = new ClientPrediction<MyInputPayload, MyStateSnapshot>(
            this,
            () => CurrentInputPayload,
            owner)
        {
            CheckDivergence = (server, local) =>
                Vector3.Distance(server.Position, local.Position) > 0.01f
        };
    }
}
```

**3. Create the NetworkBehaviour wrapper**
Mirrors the offline `*Brain` MonoBehaviour but uses `NetworkBehaviour` and wires RPCs.

```csharp
public class MyNetworkBrain : NetworkBehaviour
{
    private MyNetworkBrainCore brain;

    private void Awake()
    {
        brain = new MyNetworkBrainCore(this, /* ...serialized refs... */);

        brain.Prediction.OnSendInput = (input, tick, prevInput, prevTick) =>
            SubmitInputServerRpc(input, tick, prevInput, prevTick);

        brain.Prediction.OnSendStateCorrection = (state, tick) =>
            ReceiveStateCorrectionClientRpc(state, tick);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && !IsServer)
            brain.Prediction.RegisterWithReconciliation(NetworkWorldSimulation.Instance);
        else if (IsServer)
        {
            brain.Prediction.InitializeTick(NetworkWorldSimulation.Instance.CurrentTick);
            brain.Prediction.Register(WorldSimulation.Instance);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && !IsServer)
            brain.Prediction.UnregisterWithReconciliation(NetworkWorldSimulation.Instance);
        else if (IsServer)
            brain.Prediction.Unregister(WorldSimulation.Instance);
    }

    // Server receives input from owning client
    [ServerRpc]
    private void SubmitInputServerRpc(MyInputPayload input, int tick,
        MyInputPayload prevInput, int prevTick, ServerRpcParams rpc = default)
    {
        if (rpc.Receive.SenderClientId != OwnerClientId) return;
        brain.Prediction.EnqueueServerInput(input, tick);
        brain.Prediction.EnqueueServerInput(prevInput, prevTick);
    }

    // Server sends authoritative state to all clients
    [ClientRpc]
    private void ReceiveStateCorrectionClientRpc(MyStateSnapshot state, int tick,
        ClientRpcParams rpc = default)
    {
        if (IsOwner && !IsServer)
            brain.Prediction.ReceiveCorrection(state, tick);
        else if (!IsOwner)
            brain.ApplyState(state);   // TODO: interpolate instead of snap (see TODO/ClientPrediction.md)
    }
}
```

**4. Checklist before testing**
- [ ] Both payload structs implement `INetworkSerializable`
- [ ] `SimulateTick` is deterministic (no random, no `Time.*`, no external state reads)
- [ ] `ApplyState` fully restores the entity (position, velocity, and any state machine state)
- [ ] `CheckDivergence` threshold is tuned for the feature (loose enough to avoid constant reconciliation, tight enough to catch real divergence)
- [ ] `OnNetworkDespawn` unregisters correctly to avoid ghost entities ticking after despawn
