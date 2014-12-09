﻿using SensusService;
using SensusUI.UiProperties;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SensusUI
{
    public class OptionsPage : ContentPage
    {
        public static event EventHandler StopSensusTapped;

        public OptionsPage(SensusServiceHelper service)
        {
            Title = "Options";

            List<StackLayout> stacks = UiProperty.GetPropertyStacks(service);

            StackLayout contentLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            foreach (StackLayout stack in stacks)
                contentLayout.Children.Add(stack);

            Button stopSensusButton = new Button
            {
                Text = "Stop Sensus",
                Font = Font.SystemFontOfSize(20)
            };

            stopSensusButton.Clicked += (o, e) =>
                {
                    if (StopSensusTapped != null)
                        StopSensusTapped(o, e);
                };

            contentLayout.Children.Add(stopSensusButton);

            Content = contentLayout;
        }
    }
}