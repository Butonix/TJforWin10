﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using TJ.GetData;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TJ.ViewModels
{
    public class ComboBoxItem
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsSelected { get; set; } 
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;

        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            var link = new Uri("mailto:quobre@gmail.com");
            await Windows.System.Launcher.LaunchUriAsync(link);
        }

        public async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var link = new Uri("https://github.com/trexop/TJforWin10");
            await Windows.System.Launcher.LaunchUriAsync(link);
        }

        private double _OnetimeLoadedItems { get; set; }
        public double OnetimeLoadedItems
        {
            get
            {
                double temp = 30;
                if (localSettings.Values["NumberOfOnetimeLoadedItems"] != null)
                {
                    temp = double.Parse(localSettings.Values["NumberOfOnetimeLoadedItems"].ToString());
                }
                return temp;
            }
            set
            {
                _OnetimeLoadedItems = value;
                localSettings.Values["NumberOfOnetimeLoadedItems"] = value;
            }
        }

        public Boolean? _news_content_visible { get; set; } // Состояние галочки "показывать содержимое новости"
        public Boolean? news_content_visible
        {
            get
            {
                bool temp = false;
                if (localSettings.Values["NewsContentVisible"] != null)
                {
                    Boolean.TryParse(localSettings.Values["NewsContentVisible"].ToString(), out temp);
                }
                return temp;
            }
            set
            {
                _news_content_visible = value;
                this.OnPropertyChanged("news_content_visible");
            }
        }

        public void Checkbox_checked()
        {
            news_content_visible = true;
            localSettings.Values["NewsContentVisible"] = true;
        }
        public void Checkbox_unchecked()
        {
            news_content_visible = false;
            localSettings.Values["NewsContentVisible"] = false;
        }

        private double _FontSize { get; set; }
        public double FontSize
        {
            get
            {
                if (localSettings.Values["FontSize"] != null)
                {
                    return double.Parse(localSettings.Values["FontSize"].ToString());
                }
                else return 18;
            }
            set
            {
                _FontSize = value;
                localSettings.Values["FontSize"] = _FontSize;
                this.OnPropertyChanged("FontSize");
            }
        }

        public void DecreaseFontSize()
        {
            if (localSettings.Values["FontSize"] == null)
            {
                FontSize = 18;
            }
            else
            {
                var FS = double.Parse(localSettings.Values["FontSize"].ToString());
                FontSize = FS - 1;
            }
        }

        public void IncreaseFontSize()
        {
            if (localSettings.Values["FontSize"] == null)
            {
                FontSize = 18;
            }
            else
            {
                var FS = double.Parse(localSettings.Values["FontSize"].ToString());
                FontSize = FS + 1;
            }
        }

        public List<ComboBoxItem> ComboBoxItems { get; set; }

        private ComboBoxItem _DefaultPageSelectedItem = new ComboBoxItem();
        public ComboBoxItem DefaultPageSelectedItem
        {
            get
            {
                return _DefaultPageSelectedItem;
            }
            set
            {
                _DefaultPageSelectedItem = value; OnPropertyChanged("DefaultPageSelectedItem");
            }
        }

        public void PopulateComboboxItems()
        {

            ComboBoxItems = new List<ComboBoxItem>();
            string DefaultPageValue = "News";

            if (localSettings.Values["DefaultPage"] != null)
            {
                DefaultPageValue = localSettings.Values["DefaultPage"].ToString();
            }

            ComboBoxItems.Add(new ComboBoxItem { Name = "News", Content = "Главное"});
            ComboBoxItems.Add(new ComboBoxItem { Name = "Articles", Content = "Статьи" });
            ComboBoxItems.Add(new ComboBoxItem { Name = "Video", Content = "Видео" });
            ComboBoxItems.Add(new ComboBoxItem { Name = "Offtopic", Content = "Оффтоп" });
            ComboBoxItems.Add(new ComboBoxItem { Name = "Tweets", Content = "Твиты" });

            foreach (var Item in ComboBoxItems)
            {
                if (Item.Name == DefaultPageValue)
                {
                    DefaultPageSelectedItem = Item;
                }
            }
        }

        public void DefaultPage_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;
            var item = (ComboBoxItem)combo.SelectedItem;
            localSettings.Values["DefaultPage"] = item.Name.ToString();
        }

        private string _UserId { get; set; }
        public string UserID
        {
            get
            {
                return _UserId;
            }
            set
            {
                _UserId = value;
                this.OnPropertyChanged("UserID");
            }
        }

        public ObservableCollection<GetData.BlackListedAccount> NewsBlackList { get; set; }

        public static async void SaveBlackListCollection(object collection)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync("blacklist.json", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                string json = JsonConvert.SerializeObject(collection);
                await writer.WriteAsync(json);
            }
        }

        public void AddCurrentUserToBlacklist()
        {
            NewsBlackList.Add(new BlackListedAccount { id = int.Parse(UserID) });
            SaveBlackListCollection(NewsBlackList);
            UserID = "";
        }

        public static async void AddUserToBlackList(int userid)
        {
            var blacklist = await Facade.GetBlackListCollection();
            blacklist.Add(new BlackListedAccount { id = userid });
            SaveBlackListCollection(blacklist);
        }

        public void RemoveUserFromBlacklist(object sender, ItemClickEventArgs e)
        {
            var SelectedItem = (BlackListedAccount)e.ClickedItem;
            try
            {
                NewsBlackList.Remove(NewsBlackList.Where(i => i.id == SelectedItem.id).Single());
            } catch (InvalidOperationException)
            {
                NewsBlackList.Clear();
            }
            SaveBlackListCollection(NewsBlackList);
        }
    }
}
