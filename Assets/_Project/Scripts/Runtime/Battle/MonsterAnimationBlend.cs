namespace Spaa.Battle
{
    public static class MonsterAnimationBlend
    {
        public static float OneShotWeight(float time, float length, float fade, bool holdLastFrame)
        {
            if (time < 0f)
            {
                return 0f;
            }

            if (fade <= 0f)
            {
                return holdLastFrame || time < length ? 1f : 0f;
            }

            float fadeIn = time / fade;
            if (holdLastFrame)
            {
                return fadeIn < 1f ? fadeIn : 1f;
            }

            float fadeOut = (length - time) / fade;
            float weight = fadeIn < fadeOut ? fadeIn : fadeOut;
            if (weight < 0f)
            {
                return 0f;
            }

            return weight > 1f ? 1f : weight;
        }

        public static bool IsFinished(float time, float length, bool holdLastFrame)
        {
            return !holdLastFrame && time >= length;
        }
    }
}
