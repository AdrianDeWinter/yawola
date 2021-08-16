using System.Collections.ObjectModel;

namespace yawola
{
	public class DummyData
	{
		public ObservableCollection<WolTarget> targets { get; set; }
		public DummyData()
		{
			targets = new ObservableCollection<WolTarget>
			{
				new WolTarget("192.168.1.1", "1A:2B:3C:4D:5E:6F", "Test Host", "11"),
				new WolTarget("192.168.2.1", "AA:BB:CC:DD:EE:FF", "Another Host", "33")
			};
		}
	}
}
