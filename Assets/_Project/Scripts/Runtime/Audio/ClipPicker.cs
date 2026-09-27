using System;

namespace Spaa.Audio
{
    public class ClipPicker
    {
        private readonly Random _random;
        private int _last = -1;

        public ClipPicker(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public int Next(int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            if (count == 1)
            {
                _last = 0;
                return 0;
            }

            int index;
            if (_last < 0 || _last >= count)
            {
                index = _random.Next(count);
            }
            else
            {
                index = _random.Next(count - 1);
                if (index >= _last)
                {
                    index++;
                }
            }

            _last = index;
            return index;
        }
    }
}
