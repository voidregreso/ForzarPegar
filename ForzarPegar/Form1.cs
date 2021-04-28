using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ForzarPegar
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			this.InitializeComponent();
			StartHook();
		}

		public void frmAdminMain_FormClosing_1(object sender, FormClosingEventArgs e)
		{
			if (DialogResult.OK == MessageBox.Show("Are you sure you want to quit?", "Closing tips", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
			{
				this.FormClosing -= new FormClosingEventHandler(this.frmAdminMain_FormClosing_1);
				CloseHook();
				Application.Exit(); 
			}
			else
			{
				e.Cancel = true;
			}
		}

		public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        private static int hHook;
        public const int WH_KEYBOARD_LL = 13;
        private HookProc KeyBoardHookProcedure;

        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void StartHook()
        {
            if (hHook == 0)
            {
                KeyBoardHookProcedure = KeyBoardHookProc;
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyBoardHookProcedure,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (hHook == 0) CloseHook();
                else
                {
                    var key = Registry.CurrentUser.OpenSubKey(
                        @"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    if (key == null)
                        key = Registry.CurrentUser.CreateSubKey(
                            @"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                    key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
                    key.Close();
                }
            }
        }

        public void CloseHook()
        {
            var retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }

            if (!retKeyboard) MessageBox.Show("UnhookWindowsHookEx failed.");
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                true);
            if (key != null)
            {
                key.DeleteValue("DisableTaskMgr", false);
                key.Close();
            }
        }

        public int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));
				if (kbh.vkCode == this.hotkey) {
					this.paste();
					return 1;
				}
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        private void Form1_Load(object sender, EventArgs e)
		{
			this.comboBox1.SelectedIndex = 1;
		}

		private void paste()
		{
			string text = Clipboard.GetText();
			for (int i = 0; i < 10000; i++)
			{
			}
			if ((text.Contains('+') || text.Contains('^') || text.Contains('%') || text.Contains('~') || text.Contains('(') || text.Contains(')') || text.Contains('{') || text.Contains('}') || text.Contains('[') || text.Contains(']')))
			{
				text = this.replace(text);
			}
			if (this.disenter)
			{
				text = text.Replace("\r", "");
			}
			try
			{
				SendKeys.Send(text);
			}
			catch (ArgumentException)
			{
				text = "The content you have pasted contains a combination of characters that this program cannot handle";
				SendKeys.Send(text);
			}
		}

		private string replace(string str)
		{
			str = str.Replace("{", "__temp_123left");
			str = str.Replace("}", "__temp_456right");
			str = str.Replace("__temp_123left", "{{}");
			str = str.Replace("__temp_456right", "{}}");
			str = str.Replace("(", "{(}");
			str = str.Replace(")", "{)}");
			str = str.Replace("[", "{[}");
			str = str.Replace("]", "{]}");
			str = str.Replace("+", "{+}");
			str = str.Replace("^", "{^}");
			str = str.Replace("%", "{%}");
			str = str.Replace("~", "{~}");
			return str;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (this.comboBox1.SelectedIndex)
			{
			case 0:
				this.hotkey = (int)Keys.F1;
				return;
			case 1:
				this.hotkey = (int)Keys.F2;
				return;
			case 2:
				this.hotkey = (int)Keys.F3;
				return;
			case 3:
				this.hotkey = (int)Keys.F4;
				return;
			default:
				return;
			}
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			this.disenter = this.checkBox1.Checked;
		}

		private int hotkey = 113;

		private bool disenter;

	}
}
