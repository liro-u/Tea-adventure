using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives every registered ISimulatableEntity at a fixed tick rate.
/// This is the single FixedUpdate driver — entities must not have their own.
///
/// Offline usage:
///   Attach to a scene GameObject. Each entity (CharacterBrain, etc.) calls
///   WorldSimulation.Instance.Register(this) on Awake and Unregister(this) on OnDestroy.
///
/// Online:
///   NetworkWorldSimulation extends this class and adds reconciliation after the tick loop.
/// </summary>
public class WorldSimulation : MonoBehaviour
{
    public static WorldSimulation Instance { get; private set; }

    private readonly List<ISimulatableEntity> entities = new();

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Register(ISimulatableEntity entity)
    {
        if (!entities.Contains(entity))
            entities.Add(entity);
    }

    public void Unregister(ISimulatableEntity entity) => entities.Remove(entity);

    protected virtual void FixedUpdate()
    {
        Tick(Time.fixedDeltaTime);
    }

    protected void Tick(float dt)
    {
        foreach (var entity in entities)
            entity.SimulateTick(dt);
    }
}