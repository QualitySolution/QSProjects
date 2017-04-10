using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Gdk;
using NLog;
using Saraff.Twain;
using Saraff.Twain.Aux;

namespace QSScan
{
	/// <summary>
	/// ВНИМАНИЕ!!! Для работы с некоторыми сканерами под виндой, у главного класса приложения должен стоять атрибут [STAThread]
	/// </summary>
	public class ScanAuxWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private MemoryStream stream;

		public event EventHandler<ImageTransferEventArgs> ImageTransfer;
		public event EventHandler<ScanWorksPulseEventArgs> Pulse;
		int TotalImages = -1;
		int CurrentImage = 0;

		public List<Pixbuf> Images;
		private bool _isEnable=false;

		public ScanAuxWorks ()
		{
			Images = new List<Pixbuf>();
			SetupTwain();
		}

		public int ScannerCount 
		{
			get{return scanners.Count;}
		}

		int currentScanner;

		public int CurrentScanner{
			get{
				return currentScanner;
			}
			set{
				currentScanner = value;
				logger.Debug ("Selected Scaner [{0}]{1}", value, scanners[currentScanner].Name);
			}
		}

		Collection<Source> scanners;

		public string[] GetScannerList()
		{
			return scanners.Select(x => x.Name).ToArray ();
		}

		private Collection<Source> _GetSources() {
			var _result=new Collection<Source>();
			foreach(var _host in new string[] { Source.x86Aux, Source.msilAux }) {
				TwainExternalProcess.Execute(
					System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), _host),
					twain => {
						try {
							if(_host==Source.msilAux&&!twain.IsTwain2Supported) {
								return;
							}
							for(var i=0; i<twain.SourcesCount; i++) {
								_result.Add(new Source {
									Id=i,
									Name=twain.GetSourceProductName(i),
									IsX64Platform=_host==Source.x86Aux?false:Environment.Is64BitOperatingSystem,
									IsTwain2=twain.IsTwain2Supported,
									IsDefault=twain.SourceIndex==i
								});
							}
							currentScanner = twain.SourceIndex;
						} catch {
						}
					});
			}
			return _result;
		}

		private void SetupTwain()
		{
			logger.Debug("Setup Twain");
			scanners = _GetSources ();
			logger.Debug ("Exist Sources:");
			for(var i=0; i < scanners.Count; i++) {
				logger.Debug("{0}: {1}{2}", i, scanners[i].Name, scanners[i].IsTwain2  ? " (TWAIN 2.x)" : string.Empty);
			}

			logger.Debug ("Current Source: {0}", scanners[currentScanner].Name );
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
			_Acquire ();
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

		private Source _CurrentDataSource {
			get {
				return scanners[currentScanner];
			}
		}

		private void _Acquire() {
			TwainExternalProcess.Execute(
				System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), this._CurrentDataSource.ExecFileName),
				twain => {

					#region Native

					twain.EndXfer+= (sender, e) => Gtk.Application.Invoke (delegate {
						logger.Debug ("EndXfer fired");
						if (e.Image != null) {
							Pixbuf CurImg = WinImageToPixbuf (e.Image);
							e.Image.Dispose ();
							if (ImageTransfer == null) {
								// Записываем во внутренний массив
								Images.Add (CurImg);
							} else {
								// Передаем через событие
								ImageTransferEventArgs arg = new ImageTransferEventArgs ();
								arg.AllImages = TotalImages;
								arg.Image = CurImg;
								ImageTransfer (this, arg);
							}
						}
					});

					#endregion

					#region File

/*					twain.SetupFileXferEvent+=(sender, e) => {
						try {
							var _dlg=new Microsoft.Win32.SaveFileDialog {
								Filter=String.Format("{0}-files|*.{0}", (this.imageFileFormExpander.Content as ListBox).SelectedValue.ToString().ToLower()),
								OverwritePrompt=true
							};
							if((bool)_dlg.ShowDialog()) {
								e.FileName=_dlg.FileName;
							} else {
								e.Cancel=true;
							}
						} catch {
						}
					};

					twain.FileXferEvent+=(sender, e) => {
						try {
							if(System.IO.Path.GetExtension(e.ImageFileXfer.FileName)==".tmp") {
								Win32.MoveFileEx(e.ImageFileXfer.FileName, null, Win32.MoveFileFlags.DelayUntilReboot);
							}
							var _img=new BitmapImage(new Uri(e.ImageFileXfer.FileName));
							_img.Freeze();
							this.Dispatcher.BeginInvoke(
								new Action(() => {
									try {
										this.scanImage.Source=_img;
									} catch(Exception ex) {
										ex.ErrorMessageBox();
									}
								})
							);
						} catch {
						}
					};
*/
					#endregion

					#region Memory
					/*
					#region SetupMemXferEvent

					twain.SetupMemXferEvent+=(sender, e) => {
						try {
							System.Windows.Media.PixelFormat _format=PixelFormats.Default;
							BitmapPalette _pallete=null;
							switch(e.ImageInfo.PixelType) {
							case TwPixelType.BW:
								_format=PixelFormats.BlackWhite;
								break;
							case TwPixelType.Gray:
								_format=new Dictionary<short, System.Windows.Media.PixelFormat> {
									{2,PixelFormats.Gray2},
									{4,PixelFormats.Gray4},
									{8,PixelFormats.Gray8},
									{16,PixelFormats.Gray16}
								}[e.ImageInfo.BitsPerPixel];
								break;
							case TwPixelType.Palette:
								_pallete=new BitmapPalette(new Func<IList<Color>>(() => {
									var _res=new Collection<Color>();
									var _colors=twain.Palette.Get().Colors;
									for(int i=0; i<_colors.Length; i++) {
										_res.Add(Color.FromArgb(_colors[i].A, _colors[i].R, _colors[i].G, _colors[i].B));
									}
									return _res;
								})());
								_format=new Dictionary<short, System.Windows.Media.PixelFormat> {
									{2,PixelFormats.Indexed1},
									{4,PixelFormats.Indexed2},
									{8,PixelFormats.Indexed4},
									{16,PixelFormats.Indexed8}
								}[e.ImageInfo.BitsPerPixel];
								break;
							case TwPixelType.RGB:
								_format=new Dictionary<short, System.Windows.Media.PixelFormat> {
									{8,PixelFormats.Rgb24},
									{24,PixelFormats.Rgb24},
									{16,PixelFormats.Rgb48},
									{48,PixelFormats.Rgb48}
								}[e.ImageInfo.BitsPerPixel];
								break;
							default:
								throw new InvalidOperationException("Данный формат пикселей не поддерживается.");
							}

							this.Dispatcher.BeginInvoke(
								new Action(() => {
									try {
										this.scanImage.Source=new WriteableBitmap(
											e.ImageInfo.ImageWidth,
											e.ImageInfo.ImageLength,
											e.ImageInfo.XResolution,
											e.ImageInfo.YResolution,
											_format,
											_pallete);
									} catch(Exception ex) {
										ex.ErrorMessageBox();
									}
								})
							);

						} catch {
						}
					};

					#endregion

					twain.MemXferEvent+=(sender, e) => {
						try {
							this.Dispatcher.BeginInvoke(
								new Action(() => {
									try {
										(this.scanImage.Source as WriteableBitmap).WritePixels(
											new Int32Rect(0, 0, (int)e.ImageMemXfer.Columns, (int)e.ImageMemXfer.Rows),
											e.ImageMemXfer.ImageData,
											(int)e.ImageMemXfer.BytesPerRow,
											(int)e.ImageMemXfer.XOffset,
											(int)e.ImageMemXfer.YOffset);
									} catch(Exception ex) {
										ex.ErrorMessageBox();
									}
								})
							);
						} catch {
						}
					};
					*/

					#endregion
					#region Set Capabilities

					twain.SourceIndex=currentScanner;
					twain.ShowUI = true;
					twain.OpenDataSource();
					twain.Capabilities.XferMech.Set (TwSX.Native);

/*					try {
						twain.SetCap(TwCap.XResolution, (float)(this.resolutionExpander.Content as ListBox).SelectedValue);
					} catch {
					}

					try {
						twain.SetCap(TwCap.YResolution, (float)(this.resolutionExpander.Content as ListBox).SelectedValue);
					} catch {
					}

					try {
						twain.SetCap(TwCap.IPixelType, (TwPixelType)(this.pixelTypeExpander.Content as ListBox).SelectedValue);
					} catch {
					}

					try {
						twain.SetCap(TwCap.IXferMech, (TwSX)(this.xferMechExpander.Content as ListBox).SelectedValue);
					} catch {
					}

					try {
						twain.SetCap(TwCap.ImageFileFormat, (TwFF)(this.imageFileFormExpander.Content as ListBox).SelectedValue);
					} catch {
					}

					try {
						twain.Capabilities.Indicators.Set(false);
					} catch {
					}
*/
					twain.Capabilities.XferCount.Set(1);


					#endregion

					twain.Acquire();
				});
		}

		public void Close()
		{
			logger.Debug("Close ScanAuxWorks");
		}

		#region class Source

		private class Source {
			public const string x86Aux = "Saraff.Twain.Aux_x86.exe";
			public const string msilAux = "Saraff.Twain.Aux_MSIL.exe";

			//public static readonly DependencyProperty CurrentProperty;

			static Source() {
			//	Source.CurrentProperty=DependencyProperty.RegisterAttached("Current",typeof(Source),typeof(Source),new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender|FrameworkPropertyMetadataOptions.AffectsMeasure));
			}

			public string Visual {
				get {
					return this.ToString();
				}
			}

			public int Id {
				get;
				set;
			}

			public string ExecFileName {
				get {
					return !this.IsX64Platform&&!this.IsTwain2 ? Source.x86Aux : Source.msilAux;
				}
			}

			public string Name {
				get;
				set;
			}

			public bool IsX64Platform {
				get;
				set;
			}

			public bool IsTwain2 {
				get;
				set;
			}

			public bool IsDefault {
				get;
				set;
			}

			public override bool Equals(object obj) {
				for(var _val = obj as Source; _val!=null;) {
					return _val.IsX64Platform==this.IsX64Platform&&_val.IsTwain2==this.IsTwain2&&_val.Id==this.Id;
				}
				return false;
			}

			public override int GetHashCode() {
				return this.Id.GetHashCode();
			}

			public override string ToString() {
				return String.Format("[{0}.x; {1}]: {2}",this.IsTwain2 ? "2" : "1",this.IsX64Platform ? "x64" : "x86",this.Name);
			}
		}

		#endregion
	}
}