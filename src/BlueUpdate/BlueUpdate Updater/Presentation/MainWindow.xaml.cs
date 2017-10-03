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
using ByteSizeLib;

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

			Action<DownloadProgressChangedEventArgs> updateProgressChanged = (e) =>
			{
				ByteSize received = ByteSize.FromBytes(e.BytesReceived);
				if(total == null) {
					ByteSize totalBS = ByteSize.FromBytes(e.TotalBytesToReceive);
					total = totalBS.ToString();
				}
				_TextBlock_Status.Text = $"Downloading: {received.ToString()} / {total}";
				_ProgressBar.IsIndeterminate = false;
				_ProgressBar.Value = e.ProgressPercentage;
			};

			Action<AsyncCompletedEventArgs> updateCompleted = (e) =>
			{
				if(e.Error != null) {
					Error = e.Error;
				}else if(e.Cancelled) {
					Error = new Exception("The download process was cancelled.");
				}
				isFinished = true;
				Close();
			};

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
