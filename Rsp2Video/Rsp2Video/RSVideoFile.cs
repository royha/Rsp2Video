using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RSPro2Video
{
	// using System.Xml.Serialization;
	// XmlSerializer serializer = new XmlSerializer(typeof(ReversalDefinition));
	// using (StringReader reader = new StringReader(xml))
	// {
	//    var test = (ReversalDefinition)serializer.Deserialize(reader);
	// }

	[XmlRoot(ElementName = "Forward")]
	public class Forward
	{

		[XmlAttribute(AttributeName = "ForwardStartSampleNo")]
		public int ForwardStartSampleNo { get; set; }

		[XmlAttribute(AttributeName = "ForwardEndSampleNo")]
		public int ForwardEndSampleNo { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Reversal")]
	public class Reversal
	{

		[XmlAttribute(AttributeName = "ReverseStartSampleNo")]
		public int ReverseStartSampleNo { get; set; }

		[XmlAttribute(AttributeName = "ReverseEndSampleNo")]
		public int ReverseEndSampleNo { get; set; }

		[XmlAttribute(AttributeName = "AddToList")]
		public int AddToList { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "ReversalDefinition")]
	public class ReversalDefinition
	{

		[XmlElement(ElementName = "Forward")]
		public List<Forward> Forward { get; set; }

		[XmlElement(ElementName = "Reversal")]
		public List<Reversal> Reversal { get; set; }

		[XmlAttribute(AttributeName = "ReversalVersion")]
		public double ReversalVersion { get; set; }

		[XmlAttribute(AttributeName = "Count")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "Current")]
		public int Current { get; set; }

		[XmlAttribute(AttributeName = "TranscriptColor")]
		public string TranscriptColor { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

}
