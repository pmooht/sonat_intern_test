using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
  [SerializeField] private BottleController FirstBottle;
  [SerializeField] private BottleController SecondBottle;

  private void Update()
  {
    if (Input.GetMouseButtonDown(0))
      HandleInput();
  }

  private void HandleInput()
  {
    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

    RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

    if (hit.collider == null) return;

    BottleController clickedBottle = hit.collider.GetComponent<BottleController>();

    if (clickedBottle == null) return;

    if (FirstBottle == null)
    {
      // Chưa chọn chai nào: chọn chai đầu tiên
      FirstBottle = clickedBottle;
      FirstBottle.SetSelected(true);
    }
    else
    {
      if (FirstBottle == clickedBottle)
      {
        // Click lại chai đang chọn: bỏ chọn
        FirstBottle.SetSelected(false);
        FirstBottle = null;
      }
      else
      {
        // Click chai khác: thử thực hiện chuyển màu
        SecondBottle = clickedBottle;
        AttemptTransfer();
      }
    }
  }

  // Kiểm tra điều kiện hợp lệ và thực hiện chuyển màu từ FirstBottle sang SecondBottle
  private void AttemptTransfer()
  {
    FirstBottle.bottleControllerRef = SecondBottle;

    FirstBottle.UpdateTopColorValues();
    SecondBottle.UpdateTopColorValues();

    if (SecondBottle.FillBottleCheck(FirstBottle.topColor))
    {
      // Chuyển hợp lệ: bắt đầu animation đổ màu
      FirstBottle.SetSelected(false);
      FirstBottle.onTransferComplete += CheckLevelComplete;
      FirstBottle.StartColorTransfer();
      FirstBottle = null;
      SecondBottle = null;
    }
    else
    {
      // Chuyển không hợp lệ: reset lựa chọn
      FirstBottle.SetSelected(false);
      FirstBottle = null;
      SecondBottle = null;
    }
  }

  // Kiểm tra tất cả chai sau mỗi lần đổ xong:
  // nếu tất cả đã được giải → kết thúc màn chơi
  public void CheckLevelComplete()
  {
    BottleController[] allBottles = FindObjectsByType<BottleController>(FindObjectsSortMode.None);
    bool allSolved = true;
    foreach (BottleController bottle in allBottles)
    {
      if (!bottle.IsSolved())
        return;
    }
    if (allSolved)
    {
      foreach (BottleController bottle in allBottles)
      {
        Destroy(bottle.gameObject);
      }
    }
  }
}
