using System.Linq;
using System.Windows.Forms;

namespace ProgramBlocker
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			if (args.Length == 1)
			{
				MessageBox.Show(string.Format("Anticrastinate is currently blocking\n{0}\nShouldn't you be working?", args[0]),
					"Program Blocked", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
		}
	}
}


