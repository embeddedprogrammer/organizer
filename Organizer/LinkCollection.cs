using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
	class LinkCollection
	{
		Link[] links;
		int linkCount;

		public LinkCollection()
		{
			//links = new Link[size];
			linkCount = 0;
		}

		public void addLink(Link link)
		{
			link.ID = linkCount;
			links[linkCount++] = link;
		}

		public void removeLink(int id)
		{
			links[id] = null;
		}

		public Link getLink(int id)
		{
			return links[id];
		}

		public string getLinkRtfText()
		{
			return " ";
		}
	}
}
