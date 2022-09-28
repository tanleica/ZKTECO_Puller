using System;
using System.Text;

namespace BioMetrixCore.Info
{
    public static class SimpleScripter
    {
        public static string encode(string text)
        {
            byte[] mybyte = Encoding.UTF8.GetBytes(text);
            string returntext = Convert.ToBase64String(mybyte);
            return returntext;
        }

        public static string decode(string text)
        {
            byte[] mybyte = Convert.FromBase64String(text);
            string returntext = Encoding.UTF8.GetString(mybyte);
            return returntext;
        }
    }
}
