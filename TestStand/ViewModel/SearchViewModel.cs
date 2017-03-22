using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TestStand.Controls;
using TestStand.Model;
using TestStand.Services;
using TestStand.View;
using Jupiter.Application;
using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;

namespace TestStand.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private ObservableCollection<Device> _searchResults;
        private string _query;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        #region Commands

        public DelegateCommand<Device> GoToDeviceCommand { get; private set; }

        #endregion

        public ObservableCollection<Device> SearchResults
        {
            get { return _searchResults; }
            private set { Set(ref _searchResults, value); }
        }

        public string Query
        {
            get { return _query; }
            set
            {
                if (Set(ref _query, value))
                {
                    _cts.Cancel();
                    _cts = new CancellationTokenSource();

                    Search(_cts.Token);
                }
            }
        }

        protected override void InitializeCommands()
        {
            GoToDeviceCommand = new DelegateCommand<Device>(device =>
            {
                Popup.Current.Close();

                JupiterApp.Current.NavigationService.Navigate(typeof(DeviceHistoryView), new Dictionary<string, object>
                {
                    ["Device"] = device
                });
            });
        }

        private async void Search(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                SearchResults = null;
                return;
            }

            try
            {
                var service = Ioc.Resolve<DeviceService>();
                var devices = await service.SearchDevicesAsync(Query);

                if (token.IsCancellationRequested)
                    return;

                if (devices != null)
                {
                    if (SearchResults.IsNullOrEmpty())
                        SearchResults = new ObservableCollection<Device>(devices);
                    else
                    {
                        for (int i = 0; i < SearchResults.Count; i++)
                        {
                            if (!devices.Any(d => d.Id == SearchResults[i].Id))
                            {
                                SearchResults.RemoveAt(i);
                                i--;
                            }
                        }

                        for (int i = 0; i < devices.Count; i++)
                        {
                            if (!SearchResults.Any(d => d.Id == devices[i].Id))
                                SearchResults.Add(devices[i]);
                        }
                    }
                }
                else
                    SearchResults = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
