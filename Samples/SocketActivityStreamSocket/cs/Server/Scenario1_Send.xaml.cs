//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SDKTemplate;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Text;
using Windows.Foundation;
using System.Collections.Generic;

namespace StreamSocketListenerServer
{
    public sealed partial class Scenario1_Send : Page
    {
        private MainPage rootPage;
        private StreamSocketListener tcpListener;
        private List<StreamSocket> connectedSockets = null;
        private const string port = "40404";

        public Scenario1_Send()
        {
            this.InitializeComponent();
            SendMessageTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private async void OnConnected(
           StreamSocketListener sender,
           StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if (connectedSockets == null)
            {
                //connectedSocket.Dispose();
                //connectedSocket = null;
                connectedSockets = new List<StreamSocket>();
            }
            connectedSockets.Add(args.Socket);
            await SendMessageTextBox.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                rootPage.NotifyUser("Client has connected, go ahead and send a message...", NotifyType.StatusMessage);
                SendMessageTextBox.IsEnabled = true;
                SendButton.IsEnabled = true;
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            rootPage.NotifyUser("Waiting for client to connect...", NotifyType.StatusMessage);
            tcpListener = new StreamSocketListener();
            tcpListener.ConnectionReceived += OnConnected;
            await tcpListener.BindEndpointAsync(null, port);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var connectedSocket in connectedSockets)
            {
                try
                {
                    DataWriter writer = new DataWriter(connectedSocket.OutputStream);
                    writer.WriteBytes(Encoding.UTF8.GetBytes(SendMessageTextBox.Text));
                    await writer.StoreAsync();
                    writer.DetachStream();
                    writer.Dispose();
                }
                catch (Exception exception)
                {
                    rootPage.NotifyUser(exception.Message, NotifyType.ErrorMessage);
                    connectedSockets.Remove(connectedSocket);
                }
            }

            SendMessageTextBox.Text = "";
        }

    }
}
