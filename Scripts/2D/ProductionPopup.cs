// ============================================================================
// PRODUCTIONPOPUP.CS
// ============================================================================
// PURPOSE:      Floating text popup that shows resource production
// SPAWNED BY:   ProductionPopupPool ONLY (not attached to buildings)
// BEHAVIOR:     Rises upward, fades out, then deactivates for reuse
// ============================================================================

using UnityEngine;
using System.Collections;

public class ProductionPopup : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR SETTINGS
    // ========================================================================

    [Header("Popup Settings")]
    [SerializeField] private float _duration = 1.5f;
    [SerializeField] private float _riseHeight = 1f;
    [SerializeField] private float _randomOffsetX = 0.3f;

    [Header("Visual Settings")]
    [SerializeField] private float _startScale = 0.1f;
    [SerializeField] private float _peakScale = 0.15f;
    [SerializeField] private float _endScale = 0.12f;

    // ========================================================================
    // 🔒 PRIVATE REFERENCES
    // ========================================================================

    private TextMesh _textMesh;
    private Vector3 _startPosition;
    private Color _originalColor = Color.yellow;
    private bool _isSetup = false;

    // ========================================================================
    // 🔧 SETUP
    // ========================================================================

    private void SetupComponents()
    {
        if (_isSetup) return;

        _textMesh = GetComponent<TextMesh>();
        
        if (_textMesh == null)
        {
            _textMesh = gameObject.AddComponent<TextMesh>();
        }

        if (_textMesh != null)
        {
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.fontSize = 32;
            _textMesh.fontStyle = FontStyle.Bold;
            _textMesh.color = Color.yellow;
            _originalColor = Color.yellow;
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "UI";
            mr.sortingOrder = 100;
        }

        _isSetup = true;
    }

    // ========================================================================
    // 🚀 PUBLIC METHODS
    // ========================================================================

    public void Show(string text, Vector3 position, Color color)
    {
        SetupComponents();

        if (_textMesh == null)
        {
            Debug.LogWarning("[ProductionPopup] TextMesh not available, skipping popup.");
            return;
        }

        _textMesh.text = text;
        _textMesh.color = color;
        _originalColor = color;

        float randomX = Random.Range(-_randomOffsetX, _randomOffsetX);
        _startPosition = position + new Vector3(randomX, 0.5f, 0f);
        transform.position = _startPosition;

        transform.localScale = Vector3.one * _startScale;

        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(AnimatePopup());
    }

    // ========================================================================
    // 🎬 ANIMATION
    // ========================================================================

    private IEnumerator AnimatePopup()
    {
        float elapsed = 0f;
        Vector3 endPosition = _startPosition + new Vector3(0f, _riseHeight, 0f);

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _duration;

            transform.position = Vector3.Lerp(_startPosition, endPosition, t);

            float currentScale;
            if (t < 0.2f)
            {
                currentScale = Mathf.Lerp(_startScale, _peakScale, t / 0.2f);
            }
            else if (t < 0.4f)
            {
                currentScale = Mathf.Lerp(_peakScale, _endScale, (t - 0.2f) / 0.2f);
            }
            else
            {
                currentScale = _endScale;
            }
            transform.localScale = Vector3.one * currentScale;

            float alpha = 1f;
            if (t > 0.6f)
            {
                alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            }

            if (_textMesh != null)
            {
                Color c = _originalColor;
                c.a = alpha;
                _textMesh.color = c;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}