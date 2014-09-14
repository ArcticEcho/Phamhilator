using System.Collections.Generic;



namespace Phamhilator
{
	public class PostTypeInfo
	{
		public PostType Type;
		public bool InaccuracyPossible;
		public List<string> BadTags = new List<string>();
	}
}