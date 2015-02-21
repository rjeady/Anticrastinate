using System.Linq;
using System.Windows.Forms;

namespace ProgramBlocker
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			if (args.Length == 1 && args[0].EndsWith(".exe"))
			{
				MessageBox.Show(string.Format("Anticrastinate is currently blocking {0} \nShouldn't you be working?", args[0].Split('\\').Last()),
					"Program Blocked", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
		}
	}
}


