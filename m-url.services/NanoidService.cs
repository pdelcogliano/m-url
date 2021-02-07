using System;

namespace M_url.Services
{
    public class NanoidService
    {
        public static string Create(int length)
        {
            if (length < 5)
                throw new ArgumentOutOfRangeException("Minimum nanoid length is 5");

            return Nanoid.Nanoid.Generate("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", length);
        }
    }
}
