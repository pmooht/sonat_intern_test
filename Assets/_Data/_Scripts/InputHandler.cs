using UnityEngine;

// Chịu trách nhiệm: đọc input chuột, chọn chai, yêu cầu chuyển màu
public class InputHandler : MonoBehaviour
{
  [SerializeField] private LevelManager levelManager;

  private BottleController firstBottle;
  private BottleController secondBottle;

  private void Update()
  {
    if (Input.GetMouseButtonDown(0))
      HandleInput();
  }

  // Raycast tìm chai được click → chọn chai đầu hoặc thực hiện chuyển màu
  private void HandleInput()
  {
    Vector3 mousePos   = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

    RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

    if (hit.collider == null) return;

    BottleController clickedBottle = hit.collider.GetComponent<BottleController>();

    if (clickedBottle == null) return;

    if (firstBottle == null)
    {
      // Không cho phép chọn chai rỗng làm chai nguồn
      if (clickedBottle.numberOfColorsInBottle == 0) return;

      firstBottle = clickedBottle;
      firstBottle.SetSelected(true);
    }
    else
    {
      if (firstBottle == clickedBottle)
      {
        // Click lại chai đang chọn: bỏ chọn
        firstBottle.SetSelected(false);
        firstBottle = null;
      }
      else
      {
        // Click chai khác: thử thực hiện chuyển màu
        secondBottle = clickedBottle;
        AttemptTransfer();
      }
    }
  }

  // Kiểm tra hợp lệ và bắt đầu animation đổ màu
  private void AttemptTransfer()
  {
    firstBottle.bottleControllerRef = secondBottle;

    firstBottle.UpdateTopColorValues();
    secondBottle.UpdateTopColorValues();

    if (secondBottle.FillBottleCheck(firstBottle.topColor))
    {
      // Chuyển hợp lệ: subscribe CheckLevelComplete qua LevelManager
      firstBottle.SetSelected(false);
      firstBottle.onTransferComplete += levelManager.CheckLevelComplete;
      firstBottle.StartColorTransfer();
      firstBottle  = null;
      secondBottle = null;
    }
    else
    {
      // Chuyển không hợp lệ: reset lựa chọn
      firstBottle.SetSelected(false);
      firstBottle  = null;
      secondBottle = null;
    }
  }
}
