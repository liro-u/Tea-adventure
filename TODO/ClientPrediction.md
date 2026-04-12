# Client Prediction — Known Missing Points

## 1. Remote player interpolation
**File:** `NetworkCharacterBrain.cs` — `ReceiveStateCorrectionClientRpc`

Non-owners currently receive `brain.ApplyState(state)` every tick — a hard snap to the latest server state. This causes visible stutter.

**Fix:** introduce a `RemoteEntityInterpolator<TState>` class (separate from `ClientPrediction`, which is owner/server only). It maintains a small circular buffer of `(TState state, float receivedTime)` entries. In `Update`, it interpolates between the two states bracketing `Time.time - delay` (1–2 ticks of delay). `NetworkCharacterBrain` owns one instance per non-owned character and feeds it from the RPC.

---

## 2. Reconciliation visual smoothing
**File:** `ClientPrediction.cs` — `RestoreState`

When reconciliation fires, `RestoreState` hard-snaps the owner's character to the corrected position before replaying inputs. For small corrections this is invisible, but a larger mismatch will pop visually.

**Fix:** decouple the rendered position from the simulated position. Keep a smoothed visual transform that chases the simulation result each frame (simple lerp or spring). The simulation stays exact — only the visual layer blends the correction in over a few frames.

---

## 3. Input redundancy depth (server-side loss resilience)
**File:** `ClientPrediction.cs` — `OnSendInput` / `NetworkCharacterBrain.cs` — `SubmitInputServerRpc`

Currently the client sends current + previous input (N=2) per packet. Under a spike of 3+ consecutive dropped packets the server falls back to `lastServerInput` for multiple ticks, which increases divergence.

**Fix:** raise N to 3 or 4 — send the last N inputs per packet. The `OnSendInput` signature and `SubmitInputServerRpc` would need to carry a small fixed-size array instead of a single previous input. Cost is negligible in bandwidth.
