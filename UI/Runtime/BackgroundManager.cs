using UnityEngine;
using UnityEngine.UI;
using StoryEngine.Core;

namespace StoryEngine.UI
{
    [DisallowMultipleComponent]
    public class BackgroundManager : MonoBehaviour
    {
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image overlayImage;
        [SerializeField] private BackgroundCatalogAsset catalog;
        [SerializeField] private RectTransform parallaxTarget;

        private Coroutine _fadeRoutine;
        private string _currentId = string.Empty;

        private void OnEnable()
        {
            if (runner == null) runner = GetComponent<DialogueRunner>();
            if (runner == null) return;

            runner.LinePresented += OnPresented;
            runner.ChoicesPresented += OnPresented;
        }

        private void OnDisable()
        {
            if (runner == null) return;
            runner.LinePresented -= OnPresented;
            runner.ChoicesPresented -= OnPresented;
        }

        private void Update()
        {
            if (parallaxTarget == null || backgroundImage == null || !backgroundImage.enabled) return;

            var background = catalog != null ? catalog.Get(_currentId) : null;
            if (background == null || background.parallaxStrength <= 0f) return;

            var mouse = Input.mousePosition;
            float x = (mouse.x / Screen.width - 0.5f) * background.parallaxStrength;
            float y = (mouse.y / Screen.height - 0.5f) * background.parallaxStrength;
            parallaxTarget.anchoredPosition = new Vector2(-x, -y);
        }

        private void OnPresented(LinePresentedArgs args) => RequestBackground(args.context.backgroundId);
        private void OnPresented(ChoicesPresentedArgs args) => RequestBackground(args.context.backgroundId);

        private void RequestBackground(string backgroundId)
        {
            if (catalog == null || backgroundImage == null) return;
            if (string.IsNullOrEmpty(backgroundId) || backgroundId == _currentId) return;

            var background = catalog.Get(backgroundId);
            if (background == null) return;

            _currentId = backgroundId;

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeTo(background));
        }

        private System.Collections.IEnumerator FadeTo(BackgroundAsset background)
        {
            float duration = Mathf.Max(0.05f, background.fadeDuration);
            float half = duration * 0.5f;
            float timer = 0f;
            var startColor = backgroundImage.color;

            while (timer < half)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / half);
                backgroundImage.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }

            backgroundImage.sprite = background.sprite;
            backgroundImage.gameObject.SetActive(background.sprite != null);

            if (overlayImage != null)
            {
                var overlay = background.overlayColor;
                overlay.a = background.overlayAlpha;
                overlayImage.color = overlay;
                overlayImage.gameObject.SetActive(background.overlayAlpha > 0f);
            }

            timer = 0f;
            while (timer < half)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / half);
                backgroundImage.color = new Color(1f, 1f, 1f, t);
                yield return null;
            }

            backgroundImage.color = Color.white;
            _fadeRoutine = null;
        }
    }
}