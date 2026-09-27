using System;
using UnityEngine;

namespace Spaa.Battle
{
    [Serializable]
    public struct MonsterFace
    {
        [SerializeField] private Texture2D eye;
        [SerializeField] private Texture2D mouth;

        public MonsterFace(Texture2D eye, Texture2D mouth)
        {
            this.eye = eye;
            this.mouth = mouth;
        }

        public Texture2D Eye => eye;
        public Texture2D Mouth => mouth;

        public static MonsterFace Layer(MonsterFace baseFace, MonsterFace overrideFace)
        {
            return new MonsterFace(
                overrideFace.eye != null ? overrideFace.eye : baseFace.eye,
                overrideFace.mouth != null ? overrideFace.mouth : baseFace.mouth);
        }
    }
}
