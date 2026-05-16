using UnityEngine;
using System;

public enum LockType
{
    Maze,
    Fish,
    Platform,
    Slave
}

public class LockItem : InholdableObject
{
    [Header("Lock Settings")] [Tooltip("Which room does this lock belong to? (Select from Inspector)")]
    public LockType lockType;


    protected override void Interact()
    {
        EventManager.TriggerLockCollected(lockType);
        Destroy(gameObject);
    }
}