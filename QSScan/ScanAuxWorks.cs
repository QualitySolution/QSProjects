using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Gdk;
using Saraff.Twain;
using NLog;

namespace QSScan
{
	/// <summary>
	/// ВНИМАНИЕ!!! Для работы с некоторыми сканерами под виндой, у главного класса приложения должен стоять атрибут [STAThread]
	/// </summary>
	public class ScanWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private bool WorkWithTwain;
		private Twain32 _twain32;
		private MemoryStream stream;
		private Twain32.ColorPalette palette;

		public event EventHandler<ImageTransferEventArgs> ImageTransfer;
		public event EventHandler<ScanWorksPulseEventArgs> Pulse;
		int TotalImages = -1;
		int CurrentImage = 0;

		public List<Pixbuf> Images;
		private bool _isEnable=false;

		public ScanWorks ()
		{
			Images = new List<Pixbuf>();

			if(Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Unix)
			{
				WorkWithTwain = true;
				SetupTwain();
			}
			else
			{
				WorkWithTwain = false;
				//FIXME Setup MacOS scanner
			}
		}

		public int ScannerCount 
		{
			get{return _twain32.SourcesCount;}
		}

		public int CurrentScanner{
			get{
				return _twain32.SourceIndex;
			}
			set{
				_twain32.SourceIndex = value;
				logger.Debug ("Selected Scaner {0}", value);
				logger.Debug ("IsSourceTwain2Compatible = {0}", _twain32.GetIsSourceTwain2Compatible (value));
			}
		}

		public string[] GetScannerList()
		{
			string[] scanners = new string[_twain32.SourcesCount];

			for(int i=0;i<this._twain32.SourcesCount;i++) 
			{
				scanners[i] = this._twain32.GetSourceProductName(i);
			}

			return scanners;
		}

		private void SetupTwain()
		{
			logger.Debug("Setup Twain");
			_twain32 = new Twain32 ();
			_twain32.TwainStateChanged += _twain_TwainStateChanged;
			_twain32.AcquireError += OnTwainAcquireError;

			_twain32.SetupMemXferEvent += OnSetupMemXferEvent;
			_twain32.MemXferEvent += OnMemXferEvent;
			_twain32.AcquireCompleted += _twain32_AcquireCompleted;

			logger.Debug ("IsTwain2Enable = {0}", _twain32.IsTwain2Enable);
			_twain32.OpenDSM();
			logger.Debug ("IsTwain2Supported = {0}", _twain32.IsTwain2Supported);

			logger.Debug ("Exist Sources:");
			for(var i=0; i<_twain32.SourcesCount; i++) {
				logger.Debug("{0}: {1}{2}", i, _twain32.GetSourceProductName(i), _twain32.IsTwain2Supported&&_twain32.GetIsSourceTwain2Compatible(i)?" (TWAIN 2.x)" : string.Empty);
			}

			logger.Debug ("Current Source: {0}", _twain32.GetSourceProductName (_twain32.SourceIndex));
		}

		void OnTwainAcquireError (object sender, Twain32.AcquireErrorEventArgs e)
		{
			logger.Error (e.Exception, "Ошибка в процессе сканирования");
			this.Close ();
		}

		private void _twain_TwainStateChanged(object sender,Twain32.TwainStateEventArgs e) {
			logger.Debug("Handle TwainState");
			if((e.TwainState&Twain32.TwainStateFlag.DSEnabled)==0&&this._isEnable) {
				this._isEnable=false;
				// <<< scaning finished (or closed)
				if (stream != null)
					FinishImageTransfer ();
				logger.Debug ("Сканирование закончено...");
				_twain32.CloseDataSource ();
			}
			this._isEnable=(e.TwainState&Twain32.TwainStateFlag.DSEnabled)!=0;
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}

		private void _twain32_EndXfer(object sender,Twain32.EndXferEventArgs e) 
		{
			logger.Debug("EndXfer fired");
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
			//FIXME Если не будет использоваться нативный режим событие не нужно.
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

		public void GetImages()
		{
			if(WorkWithTwain)
			{
				RunTwain();
			}

		}

		void OnMemXferEvent (object sender, Twain32.MemXferEventArgs e)
		{
			try 
			{
				logger.Debug("On MemXfer Event {0}", this.stream.Position);
				//FIXME Некорректно работает с черно белыми изображениями на нашем сканере(нужно проверить)
				int _bytesPerPixel=e.ImageInfo.BitsPerPixel>>3;
				for(int i=0,_rowOffset=0; i<e.ImageMemXfer.Rows; i++,_rowOffset+=(int)e.ImageMemXfer.BytesPerRow) {
					for(int ii=0,_colOffset=0; ii<e.ImageMemXfer.Columns; ii++,_colOffset+=_bytesPerPixel) {
						switch(e.ImageInfo.BitsPerPixel) {
						case 1:
							for(int _mask=1; (_mask&0xff)!=0&&ii<e.ImageMemXfer.Columns; _mask<<=1,ii++) {
								this.stream.WriteByte((e.ImageMemXfer.ImageData[_rowOffset+_colOffset]&_mask)!=0?byte.MaxValue:byte.MinValue);
							}
							_colOffset++;
							ii--;
							break;
						case 8:
						case 24:
							if(e.ImageInfo.PixelType==TwPixelType.Palette) {
								System.Drawing.Color _color=this.palette.Colors[e.ImageMemXfer.ImageData[_rowOffset+_colOffset]];
								this.stream.Write(new byte[] { _color.R,_color.G,_color.B },0,3);
							} else {
								this.stream.Write(e.ImageMemXfer.ImageData,_rowOffset+_colOffset,_bytesPerPixel);
							}
							break;
						}
					}
				}
				OnPulse(e.ImageInfo.BitsPerPixel * e.ImageInfo.ImageLength * e.ImageInfo.ImageWidth / 8, (int)stream.Position);
			} 
			catch(Exception ex) 
			{
				logger.Error (ex, "Ошибка при получении изображения.");
				throw ex;
			}

		}

		private void OnPulse(int imageSize, int imagePosition)
		{
			if(Pulse != null)
			{
				var args = new ScanWorksPulseEventArgs ();
				args.CurrentImage = CurrentImage;
				args.ImageByteSize = imageSize;
				args.LoadedByteSize = imagePosition;
				args.ProgressText = String.Format("Получение {0}-го изображения...", CurrentImage + 1);
				Pulse (this, args);
			}
		}

		private void RunTwain()
		{
			logger.Debug("Run Twain");

			logger.Debug ("IsTwain2Supported = {0}", _twain32.IsTwain2Supported);
			logger.Debug ("IsTwain2Enable = {0}", _twain32.IsTwain2Enable);
			if( _twain32.OpenDataSource() == false)
			{
				string text = "Не удалось открыть источник.";
				logger.Error (text);
				throw new InvalidOperationException (text);
			}
			logger.Debug("DataSource is opened.");

			#if DEBUG
			logger.Debug ("XferMech.IsSupported=", _twain32.Capabilities.XferMech.IsSupported ());
			logger.Debug ("XferMech Support:");
			Twain32.Enumeration xferMech=_twain32.Capabilities.XferMech.Get();
			for(int i=0; i<xferMech.Count; i++) {
				logger.Debug ("[{0}] {1}", i, xferMech[i].ToString());
			}

			logger.Debug ("All Capabilities");

			foreach (TwCap cap in Enum.GetValues(typeof(TwCap)))
			{
				if((_twain32.IsCapSupported(cap)&TwQC.GetCurrent)!=0)
					logger.Debug ("{0}(Current) = {1}", cap, _twain32.GetCurrentCap (cap));
				else if((_twain32.IsCapSupported(cap)&TwQC.Get)!=0)
					logger.Debug ("{0} = {1}", cap, _twain32.GetCap (cap));
				else
					logger.Debug ("{0} = skiped", cap);
			}
			#endif

			_twain32.Capabilities.XferMech.Set (TwSX.Memory);
			//Feeder
			if(this._twain32.Capabilities.FeederEnabled.IsSupported(TwQC.Set)) {
				this._twain32.Capabilities.FeederEnabled.Set(true);
				logger.Debug ("TwCap.FeederEnabled = Enabled");

				if(this._twain32.Capabilities.AutoFeed.IsSupported(TwQC.Set)) {
					this._twain32.Capabilities.AutoFeed.Set(true);
					logger.Debug ("TwCap.AutoFeed = Enabled");
				}

				if(this._twain32.Capabilities.XferCount.IsSupported(TwQC.Set)) {
					this._twain32.Capabilities.XferCount.Set(-1);
					logger.Debug ("TwCap.XferCount = Enabled");
				}
			}

			#if DEBUG
			//Отслеживаем все события
			_twain32.AcquireCompleted += delegate {
				logger.Debug ("AcquireCompleted fired");
			};

			_twain32.AcquireError+= (sender, e) => logger.Debug ("AcquireError fired");

			_twain32.DeviceEvent += delegate {
				logger.Debug ("DeviceEvent fired");
			};

			_twain32.EndXfer += delegate {
				logger.Debug ("EndXfer fired");
			};

			_twain32.FileXferEvent += delegate {
				logger.Debug ("FileXferEvent fired");
			};

			_twain32.MemXferEvent += delegate {
				logger.Debug ("MemXferEvent fired");
			};

			_twain32.SetupFileXferEvent += delegate {
				logger.Debug ("SetupFileXferEvent fired");
			};

			_twain32.SetupMemXferEvent += delegate {
				logger.Debug ("SetupMemXferEvent fired");
			};

			_twain32.TwainStateChanged += (object sender, Twain32.TwainStateEventArgs e) =>  {
				logger.Debug ("TwainStateChanged fired");
				logger.Debug ("TwainState = {0}", e.TwainState);
			};

			_twain32.XferDone += delegate {
				logger.Debug ("XferDone fired");
			};

			#endif

			logger.Debug("Run Acquire");
			_twain32.Acquire();
			logger.Debug("After Acquire");
		}

		void OnSetupMemXferEvent (object sender, Twain32.SetupMemXferEventArgs e)
		{
			if (stream != null)
				FinishImageTransfer ();
			logger.Debug("SetupMemXfer size={0}B", e.ImageInfo.BitsPerPixel * e.ImageInfo.ImageLength * e.ImageInfo.ImageWidth / 8);
			try {
				stream = new MemoryStream((int)e.BufferSize);
				var _writer=new BinaryWriter(this.stream);
				switch(e.ImageInfo.BitsPerPixel) {
				case 1:
					if(e.ImageInfo.PixelType==TwPixelType.Palette) {
						_writer.Write(Encoding.ASCII.GetBytes("P6\n"));
						this.palette=this._twain32.Palette.Get();
					} else {
						_writer.Write(Encoding.ASCII.GetBytes("P5\n"));
					}
					break;
				case 3:
					_writer.Write(Encoding.ASCII.GetBytes("P6\n"));
					break;
				default:
					_writer.Write(Encoding.ASCII.GetBytes("PX\n"));
					break;
				}
				_writer.Write(Encoding.ASCII.GetBytes(string.Format("# (C) SARAFF SOFTWARE 2013.\n{0} {1}\n{2}\n",e.ImageInfo.ImageWidth,e.ImageInfo.ImageLength,byte.MaxValue)));
				OnPulse(e.ImageInfo.BitsPerPixel * e.ImageInfo.ImageLength * e.ImageInfo.ImageWidth / 8, (int)stream.Position);
			} catch(Exception ex) 
			{
				logger.Error (ex, "Ошибка при настройке буфера приема.");
				throw ex;
			}

		}

		private void FinishImageTransfer()
		{
			if(stream != null) 
			{
				stream.Position = 0;
				Pixbuf CurImg = new Pixbuf(stream);
				stream.Close ();
				stream = null;
				if(ImageTransfer == null)
				{// Записываем во внутренний массив
					Images.Add(CurImg);
				}
				else
				{// Передаем через событие
					ImageTransferEventArgs arg = new ImageTransferEventArgs();
					arg.AllImages = TotalImages;
					arg.Image = CurImg;
					CurrentImage++;
					ImageTransfer(this, arg);
				}
			}

		}

		public void Close()
		{
			logger.Debug("Close Scanworks");
			_twain32.Dispose();
		}
	}
}

