using UnityEngine;

public class InputHandler : MonoBehaviour
{
  [SerializeField] private LevelManager levelManager;
  [SerializeField] private PowerUpManager powerUpManager;

  private BottleController firstBottle;
  private BottleController secondBottle;

  public BottleController SelectedBottle => firstBottle;

  private void Update()
    {
      if (Input.GetMouseButtonDown(0))
        {
          HandleInput();
        }
    }

  private void HandleInput()
    {
      foreach (BottleController b in levelManager.GetAllBottles())
        {
          if (b != null && b.IsAnimating) return;
        }

      Vector3 mousePos   = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

      RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

      if (hit.collider == null) return;

      BottleController clickedBottle = hit.collider.GetComponent<BottleController>();

      if (clickedBottle == null) return;

      if (firstBottle == null)
        {
          if (clickedBottle.numberOfColorsInBottle == 0) return;
          if (clickedBottle.IsSolved()) return;

          firstBottle = clickedBottle;
          firstBottle.SetSelected(true);
        }
      else
        {
          if (firstBottle == clickedBottle)
            {
              firstBottle.SetSelected(false);
              firstBottle = null;
            }
          else
            {
              secondBottle = clickedBottle;
              AttemptTransfer();
            }
        }
    }

  private void AttemptTransfer()
    {
      firstBottle.bottleControllerRef = secondBottle;

      firstBottle.UpdateTopColorValues();
      secondBottle.UpdateTopColorValues();

      if (secondBottle.FillBottleCheck(firstBottle.topColor))
        {
          firstBottle.SetSelected(false);
          powerUpManager?.RecordMove(firstBottle, secondBottle);
          firstBottle.onTransferComplete += levelManager.CheckLevelComplete;
          firstBottle.StartColorTransfer();
          firstBottle  = null;
          secondBottle = null;
        }
      else
        {
          firstBottle.SetSelected(false);
          firstBottle  = null;
          secondBottle = null;
        }
    }
}
