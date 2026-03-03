using System.Collections;
using System.ComponentModel;
using UnityEngine;
using System;

public class BottleController : MonoBehaviour
{
  // Callback được gọi khi animation đổ màu hoàn tất
  public Action onTransferComplete;

  private const int maxLayers = 4;

  // Mảng màu của từng lớp chất lỏng trong chai (index 0 = đáy, 3 = trên cùng)
  public Color[] bottleColors;
  public SpriteRenderer bottleMaskSR;

  public float rotationDuration = 2f;
  // Curve điều khiển giá trị _SARM (Scale And Rotation Multiplier) theo góc xoay
  public AnimationCurve SARMCurve;
  // Curve ánh xạ góc xoay → mức fill chất lỏng (_FillAmout)
  public AnimationCurve FillAmountCurve;
  // Curve điều chỉnh tốc độ xoay tại từng góc
  public AnimationCurve RotationSpeedMultiplier;

  public float[] fillAmounts;
  public float[] rotationValues;

  private int rotationIndex = 0;

  [Range(0, maxLayers)]
  public int numberOfColorsInBottle = maxLayers;

  public Color topColor;
  public int numberOfTopColorLayers = 1;

  private bool isRotating = false;

  public BottleController bottleControllerRef;
  private int numberOfColorsToTransfer = 0;

  public Transform leftRotationPoint;
  public Transform rightRotationPoint;
  private Transform chosenRotationPoint;
  // +1 xoay phải, -1 xoay trái
  private float directionMultiplier = 1f;

  Vector3 originalPosition;
  Vector3 startPosition;
  Vector3 endPosition;

  public float selectionHeightOffset = 0.5f;
  public float selectionMoveSpeed = 5f;
  private bool isSelected = false;
  private Vector3 targetPosition;

  public LineRenderer lineRenderer;

  // Khởi tạo: set fill shader, lưu vị trí gốc, cập nhật màu và topColor
  void Start()
  {
    lineRenderer.enabled = false;
    bottleMaskSR.material.SetFloat("_FillAmout", fillAmounts[numberOfColorsInBottle]);

    originalPosition = transform.position;
    targetPosition = originalPosition;

    UpdateColorsOnShader();
    UpdateTopColorValues();
  }

  void Update()
  {
    if (!isRotating && transform.position != targetPosition)
      {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * selectionMoveSpeed);
      }
    }

  // Đẩy toàn bộ mảng bottleColors lên shader
  private void UpdateColorsOnShader()
  {
    for (int i = 0; i < maxLayers; i++)
    {
      bottleMaskSR.material.SetColor($"_C{i + 1}", bottleColors[i]);
    }
  }

  // Coroutine: trượt chai nguồn đến vị trí pivot của chai đích, rồi bắt đầu RotateBottle
  IEnumerator MoveBottle()
  {
    startPosition = transform.position;
    if (chosenRotationPoint == rightRotationPoint)
      endPosition = bottleControllerRef.leftRotationPoint.position;
    else
      endPosition = bottleControllerRef.rightRotationPoint.position;

    float t = 0f;
    while (t <= 1)
    {
      transform.position = Vector3.Lerp(startPosition, endPosition, t);
      t += Time.deltaTime * 2f;
      yield return new WaitForEndOfFrame();
    }
    transform.position = endPosition;
    StartCoroutine(RotateBottle());
  }

  // Coroutine: xoay chai nghiêng về phía chai đích, đồng thời cập nhật shader
  // fill chất lỏng chảy sang chai đích theo thời gian thực
  IEnumerator RotateBottle()
  {
    float t = 0f;
    float lerpValue = 0f;
    float angleValue = 0f;
    float lastAngleValue = 0f;

    isRotating = true;

    while (t < rotationDuration)
    {
      t += Time.deltaTime;

      lerpValue = t / rotationDuration;
      angleValue = Mathf.Lerp(0f, directionMultiplier * rotationValues[rotationIndex], lerpValue);
      transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

      // Cập nhật độ nghiêng của texture chất lỏng
      bottleMaskSR.material.SetFloat("_SARM", SARMCurve.Evaluate(angleValue));

      // Chỉ đổ nếu chai nguồn còn đủ chất lỏng hơn ngưỡng dừng
      if (fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleValue) + 0.005f)
      {
        // Bật line và set màu — luôn set màu khi bắt đầu rót (tránh màu trắng default)  
        if (lineRenderer.enabled == false)
        {
          // Ép alpha = 1 vì bottleColors lưu với alpha = 0 (shader dùng _FillAmout thay cho alpha)
          Color lineColor = new Color(topColor.r, topColor.g, topColor.b, 1f);
          lineRenderer.startColor = lineColor;
          lineRenderer.endColor = lineColor;
          lineRenderer.enabled = true;
        }

        lineRenderer.SetPosition(0, chosenRotationPoint.position);

        float currentFill = bottleControllerRef.bottleMaskSR.material.GetFloat("_FillAmout");
        Bounds targetBounds = bottleControllerRef.bottleMaskSR.bounds;
        float liquidBottom = targetBounds.min.y;
        float liquidTop    = targetBounds.max.y;
        float normalizedFill = Mathf.InverseLerp(fillAmounts[0], fillAmounts[maxLayers], currentFill);
        float surfaceY = Mathf.Lerp(liquidBottom, liquidTop, normalizedFill);
        Vector3 surfacePoint = new Vector3(bottleControllerRef.transform.position.x, surfaceY, 0f);
        lineRenderer.SetPosition(1, surfacePoint);

        bottleMaskSR.material.SetFloat("_FillAmout", FillAmountCurve.Evaluate(angleValue));
        bottleControllerRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue));
      }

      t += Time.deltaTime * RotationSpeedMultiplier.Evaluate(angleValue);
      lastAngleValue = angleValue;
      yield return new WaitForEndOfFrame();
    }

    // Snap về giá trị cuối chính xác
    angleValue = directionMultiplier * rotationValues[rotationIndex];
    bottleMaskSR.material.SetFloat("_SARM", SARMCurve.Evaluate(SARMCurve.Evaluate(angleValue)));
    bottleMaskSR.material.SetFloat("_FillAmout", FillAmountCurve.Evaluate(angleValue));

    // Cập nhật số lớp màu sau khi đổ
    numberOfColorsInBottle -= numberOfColorsToTransfer;
    bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTransfer;

    lineRenderer.enabled = false;

    // Clear slot màu đã trống để shader không hiển thị màu cũ
    for (int i = numberOfColorsInBottle; i < maxLayers; i++)
    {
      bottleColors[i] = Color.clear;
    }
    UpdateColorsOnShader();

    StartCoroutine(RotateBottleBack());
  }

  // Coroutine: xoay chai trở về thẳng đứng sau khi đổ xong
  IEnumerator RotateBottleBack()
  {
    float t = 0;
    float lerpValue;
    float angleValue;

    float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];
    while (t < rotationDuration)
    {
      t += Time.deltaTime;
      lerpValue = t / rotationDuration;
      angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0f, lerpValue);

      transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
      bottleMaskSR.material.SetFloat("_SARM", SARMCurve.Evaluate(angleValue));

      lastAngleValue = angleValue;
      t += Time.deltaTime;
      yield return new WaitForEndOfFrame();
    }

    // Khi về thẳng: cập nhật màu top, sync shader, reset góc
    UpdateTopColorValues();
    UpdateColorsOnShader();
    bottleMaskSR.material.SetFloat("_FillAmout", fillAmounts[numberOfColorsInBottle]);
    angleValue = 0f;
    transform.eulerAngles = new Vector3(0f, 0f, angleValue);
    bottleMaskSR.material.SetFloat("_SARM", SARMCurve.Evaluate(SARMCurve.Evaluate(angleValue)));
    isRotating = false;

    StartCoroutine(MoveBottleBack());
  }

  // Coroutine: trượt chai nguồn trở về vị trí ban đầu, rồi gọi callback hoàn tất
  IEnumerator MoveBottleBack()
  {
    startPosition = transform.position;
    endPosition = originalPosition;

    float t = 0f;
    while (t <= 1)
    {
      transform.position = Vector3.Lerp(startPosition, endPosition, t);
      t += Time.deltaTime * 2f;
      yield return new WaitForEndOfFrame();
    }
    transform.position = endPosition;

    transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
    bottleMaskSR.sortingOrder -= 2;

    onTransferComplete?.Invoke();
    onTransferComplete = null;
  }

  // Điểm khởi đầu của toàn bộ quá trình đổ màu:
  public void StartColorTransfer()
  {
    ChooseRotationPointAndDirection();

    int emptySpaceInTarget = maxLayers - bottleControllerRef.numberOfColorsInBottle;
    numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, emptySpaceInTarget);

    for (int i = 0; i < numberOfColorsToTransfer; i++)
    {
      bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
    }
    bottleControllerRef.UpdateColorsOnShader();

    CalculateRotationIndex(emptySpaceInTarget);

    transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
    bottleMaskSR.sortingOrder += 2;

    StartCoroutine(MoveBottle());
  }

  public void UpdateTopColorValues()
  {
    if (numberOfColorsInBottle <= 0)
    {
      numberOfTopColorLayers = 0;
      return;
    }

    int topIndex = Mathf.Clamp(numberOfColorsInBottle - 1, 0, bottleColors.Length - 1);
    topColor = bottleColors[topIndex];

    numberOfTopColorLayers = 1;
    for (int i = topIndex - 1; i >= 0; i--)
    {
      if (bottleColors[i].Equals(topColor))
        numberOfTopColorLayers++;
      else
        break;
    }

    rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
  }

  public bool FillBottleCheck(Color colorToCheck)
  {
    if (numberOfColorsInBottle == 0) return true;
    if (numberOfColorsInBottle >= maxLayers) return false;
    return topColor.Equals(colorToCheck);
  }

  private void CalculateRotationIndex(int numberOfEmptySpacesInSecondBottle)
  {
    rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(numberOfEmptySpacesInSecondBottle, numberOfTopColorLayers));
  }
  private void FillUp(float fillAmountToAdd)
  {
    bottleMaskSR.material.SetFloat("_FillAmout", bottleMaskSR.material.GetFloat("_FillAmout") + fillAmountToAdd);
  }

  private void ChooseRotationPointAndDirection()
  {
    if (transform.position.x < bottleControllerRef.transform.position.x)
    {
      chosenRotationPoint = rightRotationPoint;
      directionMultiplier = 1f;
    }
    else
    {
      chosenRotationPoint = leftRotationPoint;
      directionMultiplier = -1f;
    }
  }

  public bool IsSolved()
  {
    if (numberOfColorsInBottle == 0) return true;
    if (numberOfColorsInBottle < maxLayers) return false;
    Color baseColor = bottleColors[0];
    for (int i = 1; i < numberOfColorsInBottle; i++)
    {
      if (bottleColors[i] != baseColor)
        return false;
    }
    return true;
  }

  public void SetSelected(bool selected)
  {
    if (isRotating) return;

    isSelected = selected;

    if (isSelected)
      targetPosition = originalPosition + Vector3.up * selectionHeightOffset;
    else
      targetPosition = originalPosition;
  }
}
