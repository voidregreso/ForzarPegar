using System;
using System.Windows.Forms;

namespace ForzarPegar
{
	// Token: 0x02000004 RID: 4
	internal static class Program
	{
		// Token: 0x0600001F RID: 31 RVA: 0x00002CF6 File Offset: 0x00000EF6
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
