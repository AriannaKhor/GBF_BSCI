using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace GreatechApp.Services.Utilities
{
    public partial class CodeReaderDisplayControl : Form
    {
        public CodeReaderDisplayControl()
        {
            InitializeComponent();
        }

		public int codeResultCount;
		private DataManSystem m_codereader = null;
		private SynchronizationContext _syncContext = null;
		private object _currentResultInfoSyncLock = new object();

		private string GetReadStringFromResultXml(string resultXml)
		{
			try
			{
				XmlDocument doc = new XmlDocument();

				doc.LoadXml(resultXml);

				XmlNode full_string_node = doc.SelectSingleNode("result/general/full_string");

				if (full_string_node != null && m_codereader != null && m_codereader.State == Cognex.DataMan.SDK.ConnectionState.Connected)
				{
					XmlAttribute encoding = full_string_node.Attributes["encoding"];
					if (encoding != null && encoding.InnerText == "base64")
					{
						if (!string.IsNullOrEmpty(full_string_node.InnerText))
						{
							byte[] code = Convert.FromBase64String(full_string_node.InnerText);
							return m_codereader.Encoding.GetString(code, 0, code.Length);
						}
						else
						{
							return "";
						}
					}

					return full_string_node.InnerText;
				}
			}
			catch
			{
			}

			return "";
		}

		public string ShowResult(ComplexResult complexResult)
		{
			List<Image> images = new List<Image>();
			List<string> image_graphics = new List<string>();
			string read_result = null;
			int result_id = -1;
			ResultTypes collected_results = ResultTypes.None;

			// Take a reference or copy values from the locked result info object. This is done
			// so that the lock is used only for a short period of time.
			lock (_currentResultInfoSyncLock)
			{
				foreach (var simple_result in complexResult.SimpleResults)
				{
						collected_results |= simple_result.Id.Type;

						switch (simple_result.Id.Type)
						{
							case ResultTypes.Image:
								Image image = ImageArrivedEventArgs.GetImageFromImageBytes(simple_result.Data);
								if (image != null)
									images.Add(image);
								break;

							case ResultTypes.ImageGraphics:
								image_graphics.Add(simple_result.GetDataAsString());
								break;

							case ResultTypes.ReadXml:
								read_result = GetReadStringFromResultXml(simple_result.GetDataAsString());
								result_id = simple_result.Id.Id;
								break;

							case ResultTypes.ReadString:
								read_result = simple_result.GetDataAsString();
								result_id = simple_result.Id.Id;
								break;
						}
				}
				return read_result;

			}

			if (images.Count > 0)
			{
				Image first_image = images[0];

				Size image_size = Gui.FitImageInControl(first_image.Size, picResultImage.Size);
				Image fitted_image = Gui.ResizeImageToBitmap(first_image, image_size);

				if (image_graphics.Count > 0)
				{
					using (Graphics g = Graphics.FromImage(fitted_image))
					{
						foreach (var graphics in image_graphics)
						{
							ResultGraphics rg = GraphicsResultParser.Parse(graphics, new Rectangle(0, 0, image_size.Width, image_size.Height));
							ResultGraphicsRenderer.PaintResults(g, rg);
						}
					}
				}

				if (picResultImage.Image != null)
				{
					var image = picResultImage.Image;
					picResultImage.Image = null;
					image.Dispose();
				}

				picResultImage.Image = fitted_image;
				picResultImage.Invalidate();
			}
		}
    }
}
