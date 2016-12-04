using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Plugins;
using MahApps.Metro.Controls.Dialogs;
using Clipboard = System.Windows.Clipboard;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Export_To_Web
{
    public class Class1
    {
	    private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
		private static readonly string HearthpwnFile = BaseDir+"\\hearthpwn.txt";
	    private static readonly Dictionary<string, string> CardDictionary = new Dictionary<string, string>();

	    public static void OnLoad()
		{
			if(!File.Exists(HearthpwnFile))
			{
				//make sure file is present
				DownloadHearthpwn();
			}
			//read file
			var fileRead = File.ReadAllLines(HearthpwnFile);

			//initiate dictionary at plugin load
			foreach(var line in fileRead)
			{
				var cardDict = line.Split(' ');
				string cardName = string.Join(" ",cardDict.Take(cardDict.Length-1));
				CardDictionary.Add(cardName,cardDict.Last());
			}

		}

	    private static async void ExportHearthpwn()
		{
			var deck = DeckList.Instance.ActiveDeck;
			var deckUrl = "http://www.hearthpwn.com/deckbuilder/"+deck.Class.ToLower()+"#";
			foreach(var card in deck.Cards)
			{
				var cardId = CardDictionary[card.Name];
				deckUrl = deckUrl + cardId + ":" + card.Count + ";";
			}
			var dialogResult = await OpenCopy();
			if (dialogResult == MessageDialogResult.Affirmative)
			{
				Helper.TryOpenUrl(deckUrl);
			}
			if (dialogResult == MessageDialogResult.Negative)
			{
				Clipboard.SetText(deckUrl);
			}

		}

	    private static async Task<MessageDialogResult> OpenCopy()
	    {
		    MetroDialogSettings messaSettings = new MetroDialogSettings();
		    messaSettings.AffirmativeButtonText = "Open URL";
		    messaSettings.NegativeButtonText = "Copy to clipboard";
			return await Core.MainWindow.ShowMessageAsync("Hearthpwn URL created", "Would you like to Open the URL or copy it to clipboard?", MessageDialogStyle.AffirmativeAndNegative, messaSettings);
	    }

		private static void DownloadHearthpwn()
		{
			var wc = new WebClient();
			wc.DownloadFile("https://judge2020.com/hearthpwn.txt",HearthpwnFile);
		}

	    public static void menuItem_click(object sender, RoutedEventArgs e) => ExportHearthpwn();

	    public static void OnUnload()
	    {
		    CardDictionary.Clear();
	    }
    }

	public class ExportWebPlugin : IPlugin
	{
		public string Author => "Author name";

		public string ButtonText => "Settings";

		public string Description => "example text";

		public MenuItem MenuItem
		{
			get
			{
				var menuItem = new MenuItem()
				{
					Header = "Export to Hearthpwn".ToUpper()
				};
				
				
				menuItem.Click += Class1.menuItem_click;
				return menuItem;
			}
		}

		public string Name => "Export to web";

		public void OnButtonPress()
		{
		}

		public void OnLoad() => Class1.OnLoad();

		public void OnUnload()
		{
			Class1.OnUnload();
		}

		public void OnUpdate()
		{
		}

		public Version Version => new Version(1,0,0);
	}
}
