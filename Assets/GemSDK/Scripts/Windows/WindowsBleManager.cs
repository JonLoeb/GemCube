using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GemSDK.Unity.Windows
{
    public class WindowsBleManager
    {
        public static string[] GetPairedGems() {
            uint gemsCount = NativeWrapper.gemGetDescriptionListCount();

            var gems = new GemDescription[gemsCount];
            NativeWrapper.gemGetDescriptionList(gems, gemsCount);

            var addresses = new string[gems.Length];
            for(int i = 0; i < gems.Length; i++)
            {
                addresses[i] = convertAddress(gems[i].address);
            }

            return addresses;
        }

        private static string convertAddress(byte[] addr)
        {
            string res = "";

            for (int j = 0; j < addr.Length; j++)
            {
                if (j > 0)
                {
                    res += ":";
                }
                res += addr[j].ToString("X2");
            }

            return res;
        }
    }
}
