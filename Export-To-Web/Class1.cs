using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
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
	    private static Deck _selectedDeck;
	    
	    public static void OnLoad()
		{
			Hearthstone_Deck_Tracker.API.DeckManagerEvents.OnDeckSelected.Add(DeckSelected);

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

	    private static void DeckSelected(Deck deck)
	    {
		    _selectedDeck = deck;
	    }

	    private static async void ExportHearthpwn()
		{
			try
			{
				var deckUrl = "http://www.hearthpwn.com/deckbuilder/" + _selectedDeck.Class.ToLower() + "#";
				foreach (var card in _selectedDeck.Cards)
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
			catch (NullReferenceException)
			{
				await Reselect();
			}
			catch(Exception e)
			{
				var dialogResult = await Error();
				if (dialogResult == MessageDialogResult.Affirmative)
				{
					var url = "https://github.com/judge2020/Export-To-Web-HDT/issues/new?title=Crash Report&body=" + e.Message + " | Stacktrace: " + e.StackTrace;
					Helper.TryOpenUrl(url);
				}
			}

		}

	    private static async Task<MessageDialogResult> OpenCopy()
	    {
		    MetroDialogSettings messaSettings = new MetroDialogSettings
		    {
			    AffirmativeButtonText = "Open URL",
			    NegativeButtonText = "Copy to clipboard"
		    };
		    return await Core.MainWindow.ShowMessageAsync("Hearthpwn URL created", "Would you like to Open the URL or copy it to clipboard?", MessageDialogStyle.AffirmativeAndNegative, messaSettings);
	    }
		private static async Task<MessageDialogResult> Error()
		{
			MetroDialogSettings messaSettings = new MetroDialogSettings
			{
				AffirmativeButtonText = "Send report",
				NegativeButtonText = "don't send"
			};
			return await Core.MainWindow.ShowMessageAsync("Unable to create URL","Would you like to send the crash report?",MessageDialogStyle.AffirmativeAndNegative,messaSettings);
		}
		private static async Task<MessageDialogResult> Reselect()
		{
			return await Core.MainWindow.ShowMessageAsync("Please select a different deck then reselect the current one.", "This is due to HDT not really selecting a deck on startup.");
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

		public string ButtonText => "View github/readme";

		public string Description => "Exports selected deck to Hearthpwn. More deck websites to be added in the future. ";

		public MenuItem MenuItem
		{
			get
			{
				var menuItem = new MenuItem()
				{
					Header = "Export deck to Hearthpwn".ToUpper()
				};
				
				
				menuItem.Click += Class1.menuItem_click;
				return menuItem;
			}
		}

		public string Name => "Export to web";

		public void OnButtonPress()
		{
		Helper.TryOpenUrl("https://github.com/judge2020/Export-To-Web-HDT");
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
