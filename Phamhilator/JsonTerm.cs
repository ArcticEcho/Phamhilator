namespace Phamhilator
{
	public class JsonTerm
	{
		public string Regex { get; set; }
		public bool IsAuto { get; set; }
		public string Site { get; set; }
		public float Score { get; set; }
		public int TPCount { get; set; }
		public int FPCount { get; set; }
		public int CaughtCount { get; set; }
	}
}