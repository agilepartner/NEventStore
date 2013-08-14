using System;
namespace NEventStore.Example
{
	[Serializable]
	public class SomeDomainEvent
	{
		public SomeDomainEvent(string value)
		{
			Value = value;
		}

		public readonly string Value;
	}
}