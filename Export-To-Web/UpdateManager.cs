using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Export_To_Web
{
	static class UpdateManager
	{
		static bool RequiresUpdate()
		{
			if(!File.Exists("version.txt") || !File.Exists("hearthpwn.txt"))
				return true;
			using (var wc = new WebClient())
			{
				if(File.ReadAllText("version.txt") != wc.DownloadString("https://judge2020.com/hdt/export/version.txt"))
					return true;
			}
			return false;
		}

		static void PerformUpdate()
		{
			if(File.Exists("version.txt"))
				File.Delete("version.txt");
			if(File.Exists("hearthpwn.txt"))
				File.Delete("hearthpwn.txt");
			using (var wc = new WebClient())
			{
				wc.DownloadFile("https://judge2020.com/hdt/export/version.txt", "version.txt");
				wc.DownloadFile("https://judge2020.com/hdt/export/hearthpwn.txt", "hearthpwn.txt");
			}
		}

		public static void Updater()
		{
			if(!RequiresUpdate())
				return;
			PerformUpdate();
		}
	}
}
