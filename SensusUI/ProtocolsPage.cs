﻿#region copyright
// Copyright 2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
 
using SensusService;
using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace SensusUI
{
    public class ProtocolsPage : ContentPage
    {
        public static event EventHandler EditProtocol;

        private ListView _protocolsList;

        public ProtocolsPage()
        {
            Title = "Protocols";

            _protocolsList = new ListView();
            _protocolsList.ItemTemplate = new DataTemplate(typeof(TextCell));
            _protocolsList.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");
            _protocolsList.ItemsSource = UiBoundSensusServiceHelper.Get().RegisteredProtocols;

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { _protocolsList }
            };

            ToolbarItems.Add(new ToolbarItem("Open", null, () =>
                {
                    if (_protocolsList.SelectedItem != null)
                        EditProtocol(_protocolsList.SelectedItem, null);
                }));

            ToolbarItems.Add(new ToolbarItem("+", null, () =>
                {
                    UiBoundSensusServiceHelper.Get().RegisterProtocol(new Protocol("New Protocol", true));

                    _protocolsList.ItemsSource = null;
                    _protocolsList.ItemsSource = UiBoundSensusServiceHelper.Get().RegisteredProtocols;
                }));

            ToolbarItems.Add(new ToolbarItem("-", null, async () =>
                {
                    if (_protocolsList.SelectedItem != null)
                    {
                        Protocol protocolToRemove = _protocolsList.SelectedItem as Protocol;

                        if (await DisplayAlert("Delete " + protocolToRemove.Name + "?", "This action cannot be undone.", "Delete", "Cancel"))
                        {
                            await UiBoundSensusServiceHelper.Get().StopProtocolAsync(protocolToRemove, true);

                            try { Directory.Delete(protocolToRemove.StorageDirectory, true); }
                            catch (Exception ex) { UiBoundSensusServiceHelper.Get().Logger.Log("Failed to delete protocol storage directory \"" + protocolToRemove.StorageDirectory + "\":  " + ex.Message, LoggingLevel.Normal); }

                            _protocolsList.ItemsSource = _protocolsList.ItemsSource.Cast<Protocol>().Where(p => p != protocolToRemove);
                            _protocolsList.SelectedItem = null;
                        }
                    }
                }));
        }
    }
}
