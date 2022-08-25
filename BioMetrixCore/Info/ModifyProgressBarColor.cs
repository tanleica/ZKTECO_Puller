using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BioMetrixCore.Info
{
    public static class ModifyProgressBarColor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        public static void SetState(this ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, Constants.PBM_SETSTATE, (IntPtr)state, IntPtr.Zero);
        }
    }
}
