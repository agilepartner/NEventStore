using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NEventStore.Api.Syndication.Atom
{
	public class AtomPoller
	{
		private Uri _feedUri;
		private NetworkCredential _credentials;

		public AtomPoller()
		{
			//TODO : check how to make this work
		}

		public void Start(Uri feedUri, NetworkCredential credentials)
		{
			_feedUri = feedUri;
			_credentials = credentials;

			Uri last = null;
			var stop = false;
			while(last == null && !stop)
			{
				last = GetLast(_feedUri);
				if(last == null)
					Thread.Sleep(1000);
			}

			while(!stop)
			{
				var current = ReadPrevious(last);
				if(last == current)
				{
					Thread.Sleep(1000);
				}
				last = current;
			}
		}

		private SyndicationLink GetNamedLink(IEnumerable<SyndicationLink> links, string name)
		{
			return links.FirstOrDefault(link => link.RelationshipType == name);
		}

		private Uri GetLast(Uri head)
		{
			var request = (HttpWebRequest) WebRequest.Create(head);
			request.Credentials = _credentials;
			request.Accept = "application/atom+xml";
			try
			{
				using(var response = (HttpWebResponse) request.GetResponse())
				{
					if(response.StatusCode == HttpStatusCode.NotFound)
						return null;
					using(var xmlreader = XmlReader.Create(response.GetResponseStream()))
					{
						var feed = SyndicationFeed.Load(xmlreader);
						var last = GetNamedLink(feed.Links, "last");
						return (last != null) ? last.Uri : GetNamedLink(feed.Links, "self").Uri;
					}
				}
			}
			catch(WebException ex)
			{
				if(((HttpWebResponse) ex.Response).StatusCode == HttpStatusCode.NotFound)
					return null;
				throw;
			}
		}

		private Uri ReadPrevious(Uri uri)
		{
			var request = (HttpWebRequest) WebRequest.Create(uri);
			request.Credentials = _credentials;
			request.Accept = "application/atom+xml";
			using(var response = request.GetResponse())
			{
				using(var xmlreader = XmlReader.Create(response.GetResponseStream()))
				{
					var feed = SyndicationFeed.Load(xmlreader);
					foreach(var item in feed.Items.Reverse())
					{
						ProcessItem(item);
					}
					var prev = GetNamedLink(feed.Links, "previous");
					return prev == null ? uri : prev.Uri;
				}
			}
		}

		private void ProcessItem(SyndicationItem item)
		{
			//TODO : replace this stuff
			Console.WriteLine(item.Title.Text);
			//get events
			var request = (HttpWebRequest) WebRequest.Create(GetNamedLink(item.Links, "alternate").Uri);
			request.Credentials = _credentials;
			request.Accept = "application/json";
			using(var response = request.GetResponse())
			{
				var streamReader = new StreamReader(response.GetResponseStream());
				//TODO : replace this stuff
				Console.WriteLine(streamReader.ReadToEnd());
			}
		}

	}
}