using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
  [SerializeField] private LevelManager levelManager;
  [SerializeField] private InputHandler inputHandler;

  [Header("Uses (default = 3)")]
  [SerializeField] private int maxUndoUses      = 3;
  [SerializeField] private int maxAddBottleUses = 3;
  [SerializeField] private int maxShuffleUses   = 3;

  private int undoUses;
  private int addBottleUses;
  private int shuffleUses;

  private Stack<MoveSnapshot> moveHistory = new Stack<MoveSnapshot>();

  public event Action<int> OnUndoUsesChanged;
  public event Action<int> OnAddBottleUsesChanged;
  public event Action<int> OnShuffleUsesChanged;

  public struct BottleSnapshot
  {
    public Color[] colors;
    public int     numberOfColors;
  }

  private struct MoveSnapshot
  {
    public BottleController source;
    public BottleSnapshot   sourceSnap;
    public BottleController target;
    public BottleSnapshot   targetSnap;
  }

  private void Start()
  {
    ResetAllUses();
  }

  public void RecordMove(BottleController source, BottleController target)
  {
    moveHistory.Push(new MoveSnapshot
    {
      source     = source,
      sourceSnap = source.GetSnapshot(),
      target     = target,
      targetSnap = target.GetSnapshot()
    });
  }

  public void UndoLastMove()
  {
    if (undoUses <= 0)          return;
    if (moveHistory.Count == 0) return;
    if (IsAnyBottleAnimating()) return;

    MoveSnapshot snap = moveHistory.Pop();

    snap.source.RestoreFromSnapshot(snap.sourceSnap);
    snap.target.RestoreFromSnapshot(snap.targetSnap);

    undoUses--;
    OnUndoUsesChanged?.Invoke(undoUses);
  }

  public void AddBottle()
  {
    if (addBottleUses <= 0) return;

    levelManager.AddExtraBottle();

    addBottleUses--;
    OnAddBottleUsesChanged?.Invoke(addBottleUses);
  }

  public void ShuffleSelectedBottle()
  {
    if (shuffleUses <= 0) return;

    BottleController selected = inputHandler.SelectedBottle;
    if (selected == null)     return;
    if (selected.IsAnimating) return;

    selected.ShuffleColors();

    shuffleUses--;
    OnShuffleUsesChanged?.Invoke(shuffleUses);
  }

  public void ResetAllUses()
  {
    undoUses      = maxUndoUses;
    addBottleUses = maxAddBottleUses;
    shuffleUses   = maxShuffleUses;

    moveHistory.Clear();

    OnUndoUsesChanged?.Invoke(undoUses);
    OnAddBottleUsesChanged?.Invoke(addBottleUses);
    OnShuffleUsesChanged?.Invoke(shuffleUses);
  }

  public int UndoUses      => undoUses;
  public int AddBottleUses => addBottleUses;
  public int ShuffleUses   => shuffleUses;

  private bool IsAnyBottleAnimating()
  {
    foreach (BottleController b in levelManager.GetAllBottles())
    {
      if (b != null && b.IsAnimating) return true;
    }
    return false;
  }
}
