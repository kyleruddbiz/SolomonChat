// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SolomonChat.Extensions;
using SolomonChat.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SolomonChat.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<ChatModel> History { get; } = new ObservableCollection<ChatModel>();

        private const string Me = "Me";
        private const string Solomon = "Solomon";

        public MainPage()
        {
            CurrentUser = Me;

            this.InitializeComponent();

            _ = SetFocusDefault();
        }

        public string CurrentTextOther
        {
            get { return (string)GetValue(CurrentTextOtherProperty); }
            set { this.SetIfDifferent(CurrentTextOtherProperty, value); }
        }

        public static readonly DependencyProperty CurrentTextOtherProperty = DependencyProperty.Register(
            nameof(CurrentTextOther), typeof(string), typeof(MainPage), new PropertyMetadata(default(string)));

        public string CurrentUser
        {
            get { return (string)GetValue(CurrentUserProperty); }
            set { this.SetIfDifferent(CurrentUserProperty, value); }
        }

        public static readonly DependencyProperty CurrentUserProperty = DependencyProperty.Register(
            nameof(CurrentUser), typeof(string), typeof(MainPage), new PropertyMetadata(default(string)));

        private async Task SetFocusDefault()
        {
            await Task.Delay(100);

            CurrentText.Focus(FocusState.Programmatic);
        }

        private void ToggleUser()
        {
            if (CurrentUser == Me)
            {
                CurrentUser = Solomon;
            }
            else if (CurrentUser == Solomon)
            {
                CurrentUser = Me;
            }
            else
            {
                throw new InvalidOperationException("Well fuck...");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendChat();
        }

        private void SendChat()
        {
            var chat = new ChatModel(CurrentUser, CurrentTextOther);

            History.Add(chat);

            // Write chat to a file writer

            CurrentTextOther = "";

            ToggleUser();

            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.ScrollableHeight);
        }

        private async void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SendButton.Focus(FocusState.Programmatic);

            await Task.Delay(10);

            Button_Click(sender, new RoutedEventArgs());

            CurrentText.Focus(FocusState.Programmatic);
        }
    }
}