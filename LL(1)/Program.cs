using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL_1_
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Rule> rules = new List<Rule>();
            rules.Add(new Rule("S", "a"));
            rules.Add(new Rule("S", "(T)"));
            rules.Add(new Rule("T", "T,S"));
            rules.Add(new Rule("T", "S"));
            foreach (Rule rule in rules)
            {
                Console.WriteLine(rule);
            }
            Console.WriteLine();
            LL ll = new LL(rules);
            ll.RemoveLeftRecursion();
            ll.GetFirst();
            ll.GetFollow();
            ll.GetTable();
            ll.PrintTable();
            Console.WriteLine();
            Console.WriteLine("(a,a)" + ":" + ll.Check("(a,a)"));
            Console.WriteLine("(a,a,a)" + ":" + ll.Check("(a,a,a)"));
            Console.WriteLine("(a)" + ":" + ll.Check("(a)"));
            Console.WriteLine("a" + ":" + ll.Check("a"));
        }
    }
}
