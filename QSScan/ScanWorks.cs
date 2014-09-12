using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Gdk;
using Saraff.Twain;
using NLog;
using Gtk;

namespace QSScan
{
	public class ScanWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private bool WorkWithTwain;
		Twain32 _twain32;

		public event EventHandler<ImageTransferEventArgs> ImageTransfer;
		int TotalImages = -1;

		public List<Pixbuf> Images;
		private bool _isEnable=false;

		public ScanWorks ()
		{
			Images = new List<Pixbuf>();

			if(Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				WorkWithTwain = true;
				SetupTwain();
			}
			else
			{
				WorkWithTwain = false;
				//FIXME Setup Linux scanner
			}
		}

		private void SetupTwain()
		{
			logger.Debug("Setup Twain");
			_twain32 = new Twain32 (new System.ComponentModel.Container());
			_twain32.TwainStateChanged += _twain_TwainStateChanged;
			_twain32.AcquireError += OnTwainAcquireError;

			_twain32.OpenDSM();
		}

		void OnTwainAcquireError (object sender, Twain32.AcquireErrorEventArgs e)
		{
			logger.ErrorException ("Ошибка в процессе сканирования", e.Exception);
			this.Close ();
		}

		private void _twain_TwainStateChanged(object sender,Twain32.TwainStateEventArgs e) {
			if((e.TwainState&Twain32.TwainStateFlag.DSEnabled)==0&&this._isEnable) {
				this._isEnable=false;
				// <<< scaning finished (or closed)
				this.Close();
			}
			this._isEnable=(e.TwainState&Twain32.TwainStateFlag.DSEnabled)!=0;
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}

		private void _twain32_EndXfer(object sender,Twain32.EndXferEventArgs e) 
		{
			if(e.Image!=null) 
			{
				Pixbuf CurImg = WinImageToPixbuf(e.Image);
				e.Image.Dispose();
				if(ImageTransfer == null)
				{// Записываем во внутренний массив
					Images.Add(CurImg);
				}
				else
				{// Передаем через событие
					ImageTransferEventArgs arg = new ImageTransferEventArgs();
					arg.AllImages = TotalImages;
					arg.Image = CurImg;
					ImageTransfer(this, arg);
				}
			}
		}

		private void _twain32_AcquireCompleted(object sender,EventArgs e) 
		{
			logger.Debug("Acquire Completed");
			TotalImages = _twain32.ImageCount;
			for(int i = 0; i < _twain32.ImageCount; i++)
			{
				System.Drawing.Image WinImg = _twain32.GetImage(i);
				Pixbuf CurImg = WinImageToPixbuf(WinImg);
				WinImg.Dispose();
				if(ImageTransfer == null)
				{// Записываем во внутренний массив
					Images.Add(CurImg);
				}
				else
				{// Передаем через событие
					ImageTransferEventArgs arg = new ImageTransferEventArgs();
					arg.AllImages = TotalImages;
					arg.Image = CurImg;
					ImageTransfer(this, arg);
				}
			}
			logger.Debug("Data Transferred");
		}

		private Pixbuf WinImageToPixbuf( System.Drawing.Image img)
		{
			MemoryStream  stream = new MemoryStream();
			img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
			stream.Position = 0;

			Pixbuf PixImage = new Pixbuf(stream);
			stream.Close();

			return PixImage;
		}

		public void GetImages(bool AllAtOnce)
		{
			if(WorkWithTwain)
			{
				if (AllAtOnce) // Получаем изображения или все сразу или по одному.
					_twain32.AcquireCompleted += _twain32_AcquireCompleted;
				else
					_twain32.EndXfer += _twain32_EndXfer;
				RunTwain();
				//FIXME Здесь для повторного использования того же класса нужно почистить события.
			}

		}

		private void RunTwain()
		{
			logger.Debug("Run Twain");
			_twain32.OpenDataSource();
			logger.Debug("DataSource is opened.");
			//Feeder
			if((this._twain32.IsCapSupported(TwCap.Duplex)&TwQC.Get)!=0) {
				var _duplexCapValue=(ushort)this._twain32.GetCap(TwCap.Duplex);
				if(_duplexCapValue>0) {
					// 0 - TWDX_NONE
					// 1 - TWDX_1PASSDUPLEX
					// 2 - TWDX_2PASSDUPLEX

					if((this._twain32.IsCapSupported(TwCap.FeederEnabled)&TwQC.Set)!=0) {
						this._twain32.SetCap(TwCap.FeederEnabled,true);

						if((this._twain32.IsCapSupported(TwCap.XferCount)&TwQC.Set)!=0) {
							this._twain32.SetCap(TwCap.XferCount,-1);

							if((this._twain32.IsCapSupported(TwCap.DuplexEnabled)&TwQC.Set)!=0) {
								this._twain32.SetCap(TwCap.DuplexEnabled,true);
							}
						}
					}
				}
			}
			logger.Debug("Run Acquire");
			_twain32.Acquire();
			logger.Debug("After Acquire");
		}

		public void Close()
		{
			logger.Debug("Close Scanworks");
			_twain32.Dispose();
		}

		public class ImageTransferEventArgs : EventArgs
		{
			public int AllImages { get; set; }
			public Pixbuf Image { get; set; }
		}

	}
}

