using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Factunet
{
	public static class XmlDocHelper
	{
		public static void AppendAttributes(this XmlNode node, Dictionary<string, string> attributes, XmlDocument doc)
		{
			XmlAttribute docAttribute = null;
			foreach (KeyValuePair<string, string> attribute in attributes)
			{
				docAttribute = doc.CreateAttribute(attribute.Key);
				docAttribute.Value = attribute.Value;
				node.Attributes.Append(docAttribute);
			}
		}
	}
}
