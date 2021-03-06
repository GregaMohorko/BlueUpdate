﻿/*
MIT License

Copyright (c) 2018 Grega Mohorko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Project: BlueUpdate Updater
Created: 2017-10-29
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlueUpdate;
using CodeBits;

namespace BlueUpdate_Updater.Presentation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Gets the exception that was thrown in case of a unsuccessful update.
		/// </summary>
		public Exception Error { get; private set; }

		private readonly UpdatableApp appToUpdate;
		private readonly ICredentials credentials;

		private bool isFinished;

		public MainWindow(UpdatableApp appToUpdate,ICredentials credentials)
		{
			InitializeComponent();

			this.appToUpdate = appToUpdate;
			this.credentials = credentials;

			_Label_Title.Content = $"Updating {appToUpdate.Name}";
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			Update();
		}

		private void Update()
		{
			string total = null;

			void updateProgressChanged(DownloadProgressChangedEventArgs e)
			{
				string received = ByteSizeFriendlyName.Build(e.BytesReceived);
				if(total == null) {
					total = ByteSizeFriendlyName.Build(e.TotalBytesToReceive);
				}
				_TextBlock_Status.Text = $"Downloading: {received} / {total}";
				_ProgressBar.IsIndeterminate = false;
				_ProgressBar.Value = e.ProgressPercentage;
			}

			void updateCompleted(AsyncCompletedEventArgs e)
			{
				if(e.Error != null) {
					Error = e.Error;
				} else if(e.Cancelled) {
					Error = new Exception("The download process was cancelled.");
				}
				isFinished = true;
				Close();
			}

			try {
				UpdateUtility.Update(appToUpdate, credentials, updateCompleted, updateProgressChanged);
			}catch(Exception e) {
				updateCompleted(new AsyncCompletedEventArgs(e, false, null));
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(!isFinished) {
				// do not allow the user to close this window if the download is not finished yet ...
				e.Cancel = true;
			}else {
				base.OnClosing(e);
			}
		}
	}
}
