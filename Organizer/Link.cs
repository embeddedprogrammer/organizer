using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
	class Link
	{
		public int ID;
		public string destinations;
		
		public Link()
		{
			//destinations = new List<string>();
		}

		public void addDestination(string destination)
		{
			destinations += ((destinations.Length > 0) ? "\r\n" : "") + destination;
		}

		public string getLinkRtf()
		{
			return "";
		}
	}
}
