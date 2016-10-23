using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL_1_
{
    class Rule
    {
        public String show;
        private String left;
        private String right;
        private bool isClear;

        public String Left
        {
            set
            {
                left = value;
                show = left + "->" + right;
            }
            get
            {
                return left;
            }
        }

        public String Right
        {
            set
            {
                right = value;
                show = left + "->" + right;
            }
            get
            {
                return right;
            }
        }

        public Rule()
        {
            Left = "";
            Right = "";
            show = "";
            isClear = false;
        }

        public Rule(String left, String right)
        {
            Left = left;
            Right = right;
            isClear = false;
        }

        public void Clear()
        {
            Left = "";
            Right = "";      
            isClear = true;
        }

        public bool IsClear()
        {
            return isClear;
        }

        public override string ToString()
        {
            return show;
        }
    }

    class LL
    {
        private List<Rule> rules;   //文法集
        private List<String> nonterminals;     //非终结符集
        private Dictionary<String, HashSet<String>> first, follow;
        private Dictionary<String, Dictionary<String, Rule>> table;

        public LL(List<Rule> rules)
        {
            this.rules = rules;
            nonterminals = new List<String>();
            foreach (Rule rule in rules)
            {
                if (!nonterminals.Contains(rule.Left))
                {
                    nonterminals.Add(rule.Left);
                }
            }
            first = new Dictionary<String, HashSet<String>>();
            follow = new Dictionary<String, HashSet<String>>();
            table = new Dictionary<String, Dictionary<String, Rule>>();
        }

        /// <summary>
        /// 检索每一个不存在直接左递归的文法，如果存在间接左递归则转换为直接左递归的方式
        /// </summary>
        private void Search()
        {
            for (int i = 0; i < nonterminals.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    for (int k = 0; k < rules.Count; k++)
                    {
                        if (nonterminals[i].Equals(rules[k].Left) && nonterminals[j].Equals(rules[k].Right[0].ToString()))
                        {
                            List<Rule> tmpAdd = new List<Rule>();
                            List<Rule> tmpClear = new List<Rule>();
                            for (int p = 0; p < rules.Count; p++)
                            {
                                if (rules[p].Left.Equals(nonterminals[j]))
                                {
                                    Rule rule = new Rule(rules[k].Left, rules[p].Right + rules[k].Right.Substring(1));
                                    tmpAdd.Add(rule);
                                    tmpClear.Add(rules[k]);
                                }
                            }
                            foreach (Rule rule in tmpAdd)
                            {
                                rules.Add(rule);
                            }
                            foreach (Rule rule in tmpClear)
                            {
                                rules[k].Clear();
                            }
                            if (tmpAdd.Count > 0 && tmpClear.Count > 0)
                            {
                                Search();
                                return;
                            }
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// 消除左递归
        /// </summary>
        public void RemoveLeftRecursion()
        {
            List<Rule> tmp = new List<Rule>();

            Search();

            //存储存在左递归的非终结符,最后加入 T'->ε
            List<String> buffer = new List<String>();

            for (int i = 0; i < nonterminals.Count; i++)
            {
                bool tag = false;   //是否存在左递归
                for (int j = 0; j < rules.Count; j++)
                {
                    if (nonterminals[i].Equals(rules[j].Left))
                    {
                        if (rules[j].Left.Equals(rules[j].Right.Substring(0, rules[j].Left.Length)))
                        {
                            tag = true;
                        }
                    }
                }

                if (tag)
                {
                    for (int j = 0; j < rules.Count; j++)
                    {
                        //检索每一个非终结符，对非终结符相同的每一个文法                   
                        if (nonterminals[i].Equals(rules[j].Left))
                        {
                            //T->TE  ==>  T'->ET'
                            if (rules[j].Left.Equals(rules[j].Right.Substring(0, rules[j].Left.Length)))
                            {
                                String tmp1 = rules[j].Right.Substring(1);
                                String tmp2 = rules[j].Right[0].ToString();
                                rules[j].Left = rules[j].Left + "'";
                                rules[j].Right = tmp1 + tmp2 + "'";
                            }
                            //T->A ==> T->AT'
                            else
                            {
                                rules[j].Right = rules[j].Right + rules[j].Left + "'";
                            }
                        }
                    }
                    buffer.Add(nonterminals[i]);
                }
            }

            foreach (String str in buffer)
            {
                rules.Add(new Rule(str + "'", "ε"));
                nonterminals.Add(str + "'");
            }
            rules.RemoveAll((Rule rule) => { return rule.IsClear(); });
        }

        /// <summary>
        /// 获得First集
        /// </summary>
        public void GetFirst()
        {
            bool tag = true;
            HashSet<String> tmp;
            while (tag)
            {
                tag = false;
                //按非终结符的顺序检索每一个文法
                foreach (String nonterminal in nonterminals)
                {
                    foreach (Rule rule in rules)
                    {
                        //在文法的左侧寻找这个非终结符
                        if (rule.Left.Equals(nonterminal) || rule.Left.Equals(nonterminal + "'"))
                        {
                            if (!first.ContainsKey(rule.Left))
                            {
                                tmp = new HashSet<String>();
                                first[rule.Left] = tmp;
                            }
                            bool isfirst = true;
                            foreach (char c in rule.Right)
                            {
                                if (isfirst)
                                {
                                    //如果右侧第一个为终结符则不用继续在右侧寻找
                                    if (!nonterminals.Contains(c.ToString()))
                                    {
                                        if (first[rule.Left].Add(c.ToString()))
                                        {
                                            tag = true;
                                        }
                                        break;
                                    }
                                }
                                isfirst = false;
                                //如果字符是非终结符
                                if (nonterminals.Contains(c.ToString()))
                                {
                                    //则将它的First集加入当前非终结符的First集中
                                    if (first.TryGetValue(c.ToString(), out tmp))
                                    {
                                        foreach (String str in tmp)
                                        {
                                            if (first[rule.Left].Add(str))
                                            {
                                                tag = true;
                                            }
                                        }
                                        //如果它的First集中不包含空则停止向后检索，否则删去空集继续检索
                                        if (!tmp.Contains("ε"))
                                        {
                                            break;
                                        }
                                        first[rule.Left].Remove("ε");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获得Follow集合
        /// </summary>
        public void GetFollow()
        {
            bool tag = true;
            HashSet<String> tmp;
            bool isfirst = true;
            while (tag)
            {
                tag = false;
                //按非终结符的顺序对文法进行检索
                foreach (String nonterminal in nonterminals)
                {
                    if (!follow.ContainsKey(nonterminal))
                    {
                        tmp = new HashSet<String>();
                        follow[nonterminal] = tmp;
                    }
                    if (isfirst)
                    {
                        follow[nonterminal].Add("#");
                    }
                    isfirst = false;
                    foreach (Rule rule in rules)
                    {
                        //在文法右侧寻找该非终结符                     
                        int i = rule.Right.IndexOf(nonterminal);
                        if (i != -1)
                        {
                            //如果该非终结符位于表达式的最后，则将该文法左侧的非终结符的Follow集加入该非终结符的Follow集中
                            if (i == rule.Right.Length - nonterminal.Length)
                            {
                                if (follow.TryGetValue(rule.Left, out tmp))
                                {
                                    foreach (String str in tmp)
                                    {
                                        if (follow[nonterminal].Add(str))
                                        {
                                            tag = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //如果该非终结符的后一位是非终结符
                                if (nonterminals.Contains(rule.Right[i + 1].ToString()))
                                {
                                    //获得后一个非终结符的First集
                                    if (i + 2 < rule.Right.Length && rule.Right[i + 2].ToString().Equals("'"))
                                    {
                                        tmp = first[rule.Right[i + 1].ToString() + "'"];
                                        i++;
                                    }
                                    else
                                    {
                                        tmp = first[rule.Right[i + 1].ToString()];
                                    }
                                    foreach (String str in tmp)
                                    {
                                        if (follow[nonterminal].Add(str))
                                        {
                                            if (!str.Equals("ε"))
                                            {
                                                tag = true;
                                            }
                                        }

                                    }
                                    int j = i + 1;
                                    //如果其First集中有空集
                                    while (tmp.Contains("ε"))
                                    {
                                        follow[nonterminal].Remove("ε");
                                        if (j + 2 < rule.Right.Length && rule.Right[j + 2].ToString().Equals("'"))
                                        {
                                            tmp = first[rule.Right[j + 1].ToString() + "'"];
                                            j++;
                                        }
                                        else if (j + 1 < rule.Right.Length)
                                        {
                                            tmp = first[rule.Right[j + 1].ToString()];
                                        }
                                        else
                                        {
                                            if (follow.TryGetValue(rule.Left, out tmp))
                                            {
                                                foreach (String str in tmp)
                                                {
                                                    if (follow[nonterminal].Add(str))
                                                    {
                                                        tag = true;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                        j++;
                                        foreach (String str in tmp)
                                        {
                                            if (follow[nonterminal].Add(str))
                                            {
                                                tag = true;
                                            }
                                        }
                                    }
                                }
                                //如果该非终结符的后一位不是非终结符
                                else
                                {
                                    if (rule.Right[i + 1] != '\'')
                                    {
                                        if (follow[nonterminal].Add(rule.Right[i + 1].ToString()))
                                        {
                                            tag = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获得预测表
        /// </summary>
        public void GetTable()
        {
            foreach (Rule rule in rules)
            {
                Dictionary<String, Rule> tmp;
                if (!table.TryGetValue(rule.Left, out tmp))
                {
                    tmp = new Dictionary<String, Rule>();
                    table[rule.Left] = tmp;
                }
                for (int i = 0; i < rule.Right.Length; i++)
                {
                    String str;
                    if (i < rule.Right.Length - 1 && rule.Right[i + 1] == '\'')
                    {
                        str = rule.Right.Substring(i, 2);
                        i++;
                    }
                    else
                    {
                        str = rule.Right[i].ToString();
                    }
                    if (nonterminals.Contains(str))
                    {
                        foreach (String s in first[str])
                        {
                            if (!(s.Equals("ε")))
                            {
                                table[rule.Left][s] = rule;
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (!str.Equals("ε"))
                        {
                            table[rule.Left][str] = rule;
                            break;
                        }
                        else
                        {
                            foreach (String s in follow[rule.Left])
                            {
                                if (!(s.Equals("ε")))
                                {
                                    table[rule.Left][s] = rule;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 打印预测表
        /// </summary>
        public void PrintTable()
        {
            foreach (String key in table.Keys)
            {
                Console.Write(key + ":\t");
                foreach (String key2 in table[key].Keys)
                {
                    Console.Write(key2 + ":  " + table[key][key2] + "\t");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 检查字符串是否符合语法
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>符合或者不符合</returns>
        public bool Check(String str)
        {
            Stack<String> stack = new Stack<String>();
            Dictionary<String, Rule> tmp;
            stack.Push("#");
            stack.Push(nonterminals[0]);
            str = str + "#";
            int i = 0;

            while (true)
            {
                while (str[i].Equals(stack.Peek()[0]))
                {
                    if (str[i] != '#')
                    {
                        if (!nonterminals.Contains(stack.Peek()[0].ToString()))
                        {
                            i++;
                            String s = stack.Pop();
                            if (!s.Substring(1).Equals(""))
                            {
                                stack.Push(s.Substring(1));
                            }
                        }                                             
                    }
                    else
                    {
                        return true;
                    }
                }

                String peek;
                String els;
                if (stack.Peek().Length > 1 && stack.Peek()[1] == '\'')
                {
                    peek = stack.Peek().Substring(0, 2);
                    els = stack.Peek().Substring(2);              
                }
                else
                {
                    peek = stack.Peek()[0].ToString();
                    els = stack.Peek().Substring(1);
                }

                if (table.TryGetValue(peek, out tmp))
                {
                    Rule rule;
                    if (table[peek].TryGetValue(str[i].ToString(), out rule))
                    {
                        stack.Pop();
                        if (!els.Equals(""))
                        {
                            stack.Push(els);
                        }
                        if (!((rule.Right).Equals("ε")))
                        {                            
                            stack.Push(rule.Right);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

