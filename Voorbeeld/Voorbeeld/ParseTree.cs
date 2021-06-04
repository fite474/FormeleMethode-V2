using System;
using System.Collections.Generic;
using System.Text;

namespace Voorbeeld
{
    using input = System.Char;

    class ParseTree
    {
        public enum NodeType
        {
            Chr,
            Star,
            Question,
            Alter,
            Concat
        }

        public NodeType type;
        public input? data;
        public ParseTree left;
        public ParseTree right;

        public ParseTree(NodeType type_, input? data_, ParseTree left_, ParseTree right_)
        {
            type = type_;
            data = data_;
            left = left_;
            right = right_;
        }

		public List<string> GiveLanguage(int length)
		{
			List<string> result = new List<string>();

			if (length <= 0)
			{
				Console.WriteLine($"length is {length}");
				return result;
			}

			switch (type)
			{
				case NodeType.Chr:
					result.Add(data.ToString());
					break;

				case NodeType.Star:
					List<string> languageLeft = left.GiveLanguage(length - 1);
					result.AddRange(languageLeft);

					for(int i = 1; i < length; i++)
					{
						HashSet<string> languageTemp = new HashSet<string>(result);

						foreach(string s1 in languageLeft)
						{
							foreach(string s2 in languageTemp)
							{
								result.Add(s1 + s2);
							}
						}
					}

					// Because of the star operator a word doesnt have to be included
					result.Add("");
					break;

				case NodeType.Question:
					result.AddRange(left.GiveLanguage(length - 1));
					result.Add("");
					break;

				case NodeType.Alter:
					result.AddRange(left.GiveLanguage(length - 1));
					result.AddRange(right.GiveLanguage(length - 1));
					break;

				case NodeType.Concat:
					List<string> languageLeft2 = left.GiveLanguage(length - 1);
					List<string> languageRight = right.GiveLanguage(length - 1);
					
					foreach(string leftWord in languageLeft2)
					{
						foreach(string rightWord in languageRight)
						{
							result.Add(leftWord + rightWord);
						}
					}

					break;
			}

			return result;
		}
    }
}

