using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spaa.UI
{
    public static class UiMotion
    {
        public static void SetScale(VisualElement element, float value)
        {
            element.style.scale = new Scale(new Vector3(value, value, 1f));
        }

        private static Tween RawScale(VisualElement element, float from, float to, float duration)
        {
            float current = from;
            SetScale(element, from);
            return DOTween.To(() => current, v => { current = v; SetScale(element, v); }, to, duration);
        }

        private static Tween RawFade(VisualElement element, float from, float to, float duration)
        {
            float current = from;
            element.style.opacity = from;
            return DOTween.To(() => current, v => { current = v; element.style.opacity = v; }, to, duration);
        }

        private static Tween RawMoveY(VisualElement element, float from, float to, float duration)
        {
            float current = from;
            element.style.translate = new Translate(0f, from);
            return DOTween.To(() => current, v => { current = v; element.style.translate = new Translate(0f, v); }, to, duration);
        }

        private static T Own<T>(T tween, VisualElement element, GameObject link) where T : Tween
        {
            tween.SetTarget(element).SetLink(link, LinkBehaviour.KillOnDisable);
            return tween;
        }

        public static Sequence PressSpring(VisualElement element, GameObject link)
        {
            DOTween.Kill(element, true);
            var sequence = DOTween.Sequence()
                .Append(RawScale(element, 1f, 0.95f, 0.06f).SetEase(Ease.OutQuad))
                .Append(RawScale(element, 0.95f, 1f, 0.35f).SetEase(Ease.OutElastic, 1.1f, 0.4f));
            return Own(sequence, element, link);
        }

        public static Sequence PopIn(VisualElement element, GameObject link, float delay = 0f)
        {
            DOTween.Kill(element);
            SetScale(element, 0.85f);
            element.style.opacity = 0f;
            var sequence = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(RawScale(element, 0.85f, 1f, 0.35f).SetEase(Ease.OutBack, 1.8f))
                .Join(RawFade(element, 0f, 1f, 0.2f));
            return Own(sequence, element, link);
        }

        public static Sequence SlideUpIn(VisualElement element, GameObject link, float distance, float delay = 0f)
        {
            DOTween.Kill(element);
            element.style.opacity = 0f;
            element.style.translate = new Translate(0f, distance);
            var sequence = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(RawMoveY(element, distance, 0f, 0.4f).SetEase(Ease.OutBack, 1.2f))
                .Join(RawFade(element, 0f, 1f, 0.2f));
            return Own(sequence, element, link);
        }

        public static Tween FadeIn(VisualElement element, GameObject link, float duration = 0.2f)
        {
            DOTween.Kill(element);
            return Own(RawFade(element, 0f, 1f, duration), element, link);
        }

        public static Tween FadeOut(VisualElement element, GameObject link, float duration = 0.3f)
        {
            DOTween.Kill(element);
            var tween = RawFade(element, 1f, 0f, duration).SetEase(Ease.Linear);
            tween.OnComplete(() => element.style.display = DisplayStyle.None);
            return Own(tween, element, link);
        }

        public static Tween TitleBounce(VisualElement element, GameObject link)
        {
            DOTween.Kill(element);
            element.style.translate = new Translate(0f, 0f);
            var pop = RawScale(element, 0.2f, 1f, 0.7f).SetEase(Ease.OutElastic, 1.2f, 0.35f);
            pop.OnComplete(() =>
            {
                var bob = RawMoveY(element, 0f, -4f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                Own(bob, element, link);
            });
            return Own(pop, element, link);
        }

        public static Sequence Callout(VisualElement element, GameObject link, float hold = 0.6f)
        {
            SetScale(element, 0f);
            var sequence = DOTween.Sequence()
                .Append(RawScale(element, 0f, 1f, 0.45f).SetEase(Ease.OutBack, 3f))
                .AppendInterval(hold)
                .Append(RawMoveY(element, 0f, -80f, 0.35f).SetEase(Ease.InQuad))
                .Join(RawFade(element, 1f, 0f, 0.35f));
            sequence.OnKill(() => element.RemoveFromHierarchy());
            return Own(sequence, element, link);
        }

        public static void SetScaleX(VisualElement element, float value)
        {
            element.style.scale = new Scale(new Vector3(value, 1f, 1f));
        }

        public static Tween BarTo(VisualElement fill, GameObject link, float from, float to, float duration, float delay, Ease ease)
        {
            DOTween.Kill(fill);
            float current = from;
            SetScaleX(fill, from);
            var tween = DOTween.To(() => current, v => { current = v; SetScaleX(fill, v); }, to, duration)
                .SetDelay(delay)
                .SetEase(ease);
            return Own(tween, fill, link);
        }

        public static Tween Shake(VisualElement element, GameObject link, float strength = 10f, float duration = 0.25f, int cycles = 3)
        {
            DOTween.Kill(element, true);
            float t = 0f;
            var tween = DOTween.To(() => t, v =>
                {
                    t = v;
                    float x = Mathf.Sin(v * cycles * 2f * Mathf.PI) * strength * (1f - v);
                    element.style.translate = new Translate(x, 0f);
                }, 1f, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => element.style.translate = new Translate(0f, 0f));
            return Own(tween, element, link);
        }

        public static Tween Pulse(VisualElement element, GameObject link, float low = 0.7f, float period = 1.2f)
        {
            DOTween.Kill(element);
            var tween = RawFade(element, 1f, low, period * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            return Own(tween, element, link);
        }

        public static Sequence Punch(VisualElement element, GameObject link, float peak = 1.15f, float duration = 0.2f)
        {
            DOTween.Kill(element, true);
            var sequence = DOTween.Sequence()
                .Append(RawScale(element, 1f, peak, duration * 0.35f).SetEase(Ease.OutQuad))
                .Append(RawScale(element, peak, 1f, duration * 0.65f).SetEase(Ease.OutBack));
            return Own(sequence, element, link);
        }

        public static Sequence PopOut(VisualElement element, GameObject link, System.Action onComplete, float duration = 0.15f)
        {
            DOTween.Kill(element);
            var sequence = DOTween.Sequence()
                .Append(RawScale(element, 1f, 0.95f, duration).SetEase(Ease.InQuad))
                .Join(RawFade(element, 1f, 0f, duration));
            sequence.OnComplete(() =>
            {
                SetScale(element, 1f);
                element.style.opacity = 1f;
                onComplete?.Invoke();
            });
            return Own(sequence, element, link);
        }

        public static Sequence FlyTo(VisualElement element, GameObject link, Vector2 from, Vector2 to, float duration = 0.35f)
        {
            float t = 0f;
            element.style.left = from.x;
            element.style.top = from.y;
            var sequence = DOTween.Sequence()
                .Append(DOTween.To(() => t, v =>
                {
                    t = v;
                    element.style.left = Mathf.Lerp(from.x, to.x, v);
                    element.style.top = Mathf.Lerp(from.y, to.y, v);
                    SetScale(element, Mathf.Lerp(1f, 0.4f, v));
                }, 1f, duration).SetEase(Ease.InQuad));
            sequence.OnKill(() => element.RemoveFromHierarchy());
            return Own(sequence, element, link);
        }

        public static Sequence FloatUp(VisualElement element, GameObject link, float distance = 120f, float duration = 0.7f)
        {
            var sequence = DOTween.Sequence()
                .Append(RawScale(element, 0.6f, 1f, 0.15f).SetEase(Ease.OutBack, 2f))
                .Join(RawMoveY(element, 0f, -distance, duration).SetEase(Ease.OutCubic))
                .Insert(duration * 0.7f, RawFade(element, 1f, 0f, duration * 0.3f));
            sequence.OnKill(() => element.RemoveFromHierarchy());
            return Own(sequence, element, link);
        }

        public static Sequence Flash(VisualElement element, GameObject link, float peak = 1f, float duration = 0.25f)
        {
            DOTween.Kill(element);
            var sequence = DOTween.Sequence()
                .Append(RawFade(element, 0f, peak, duration * 0.4f))
                .Append(RawFade(element, peak, 0f, duration * 0.6f));
            return Own(sequence, element, link);
        }

        public static Sequence SwapText(Label label, GameObject link, string newText, float duration = 0.25f)
        {
            DOTween.Kill(label, true);
            float half = duration * 0.5f;
            var sequence = DOTween.Sequence()
                .Append(RawMoveY(label, 0f, -24f, half).SetEase(Ease.InQuad))
                .Join(RawFade(label, 1f, 0f, half))
                .AppendCallback(() => label.text = newText)
                .Append(RawMoveY(label, 24f, 0f, half).SetEase(Ease.OutQuad))
                .Join(RawFade(label, 0f, 1f, half));
            return Own(sequence, label, link);
        }

        public static Sequence VictoryBanner(VisualElement element, GameObject link, System.Action onComplete)
        {
            DOTween.Kill(element);
            element.style.opacity = 1f;
            element.style.translate = new Translate(0f, 0f);
            var sequence = DOTween.Sequence()
                .Append(RawScale(element, 0.2f, 1f, 0.7f).SetEase(Ease.OutElastic, 1.2f, 0.35f))
                .AppendInterval(0.6f)
                .Append(RawScale(element, 1f, 0.5f, 0.3f).SetEase(Ease.InQuad))
                .Join(RawMoveY(element, 0f, -420f, 0.3f).SetEase(Ease.InQuad))
                .Join(RawFade(element, 1f, 0f, 0.3f));
            sequence.OnComplete(() => onComplete?.Invoke());
            return Own(sequence, element, link);
        }

        public static void Confetti(VisualElement container, Color[] colors, int count, System.Random random, GameObject link)
        {
            float width = container.resolvedStyle.width;
            float height = container.resolvedStyle.height;
            if (float.IsNaN(width) || width <= 0f)
            {
                width = 1080f;
            }

            if (float.IsNaN(height) || height <= 0f)
            {
                height = 1920f;
            }

            for (int i = 0; i < count; i++)
            {
                var piece = new VisualElement { pickingMode = PickingMode.Ignore };
                piece.style.position = Position.Absolute;
                piece.style.left = (float)random.NextDouble() * width;
                piece.style.top = -40f;
                piece.style.width = 14f + (float)random.NextDouble() * 12f;
                piece.style.height = 24f + (float)random.NextDouble() * 16f;
                piece.style.backgroundColor = colors[random.Next(colors.Length)];
                container.Add(piece);

                float fallTime = 1.6f + (float)random.NextDouble() * 1.2f;
                float delay = (float)random.NextDouble() * 0.5f;
                float drift = ((float)random.NextDouble() - 0.5f) * 240f;
                float spin = ((float)random.NextDouble() - 0.5f) * 1440f;
                float t = 0f;

                var fall = DOTween.To(() => t, v =>
                    {
                        t = v;
                        piece.style.translate = new Translate(drift * v, (height + 80f) * v);
                        piece.style.rotate = new Rotate(spin * v);
                        piece.style.opacity = v < 0.8f ? 1f : (1f - v) * 5f;
                    }, 1f, fallTime)
                    .SetDelay(delay)
                    .SetEase(Ease.InQuad)
                    .OnKill(() => piece.RemoveFromHierarchy());
                Own(fall, piece, link);
            }
        }
    }
}
