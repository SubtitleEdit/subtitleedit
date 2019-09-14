using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class SeJsonParser
    {
        public class StateElement
        {
            public int State { get; set; }
            public string Name { get; set; }
        }

        private const int StateObjectStart = 0;
        private const int StateObjectValue = 1;
        private const int StateObjectEnd = 2;
        private const int StateArrayStart = 3;
        private const int StateArrayEnd = 4;

        /// <summary>
        /// Get values as strings from tags
        /// </summary>
        public List<string> GetAllTagsByNameAsStrings(string content, string name)
        {
            var list = new List<string>();
            int i = 0;
            int max = content.Length;
            var state = new Stack<int>();
            var objectName = string.Empty;
            while (i < max)
            {
                var ch = content[i];
                if (state.Count == 0)
                {
                    if (ch == '{')
                    {
                        state.Push(StateObjectStart);
                    }
                    else if (ch == '[')
                    {
                        state.Push(StateArrayStart);
                    }
                    i++;
                }
                else if (state.Peek() == StateObjectEnd)
                {
                    if (ch == ',')
                    {
                        state.Pop();
                        state.Push(StateObjectStart);
                    }
                    else if (ch == '}')
                    {
                        state.Pop();
                    }
                    i++;
                }
                else if (state.Peek() == StateArrayEnd)
                {
                    if (ch == ',')
                    {
                        state.Pop();
                        state.Push(StateArrayStart);
                    }
                    else if (ch == ']')
                    {
                        state.Pop();
                    }
                    i++;
                }
                else if (state.Peek() == StateObjectValue)
                {
                    if (ch == '{')
                    {
                        state.Push(StateObjectStart);
                    }
                    else if (ch == '[')
                    {
                        state.Push(StateArrayStart);
                    }
                    else if (ch == '"')
                    {
                        i++;
                        var skip = true;
                        int end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                return list;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                return list;
                            }
                        }
                        var objectValue = content.Substring(i, end - i).Trim();
                        if (objectName == name)
                        {
                            list.Add(objectValue);
                        }
                        i += end - i;
                        state.Pop();
                        state.Push(StateObjectEnd);
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        if (objectName == name)
                        {
                            list.Add("null");
                        }
                        i += 3;
                        state.Pop();
                        state.Push(StateObjectEnd);
                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        if (objectName == name)
                        {
                            list.Add("true");
                        }
                        i += 3;
                        state.Pop();
                        state.Push(StateObjectEnd);
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        if (objectName == name)
                        {
                            list.Add("false");
                        }
                        i += 3;
                        state.Pop();
                        state.Push(StateObjectEnd);
                    }
                    else if ("+-0123456789.Ee".Contains(ch))
                    {
                        var sb = new StringBuilder();
                        while ("+-0123456789.Ee".Contains(content[i]) && i < max)
                        {
                            sb.Append(content[i]);
                            i++;
                        }
                        if (objectName == name)
                        {
                            list.Add(sb.ToString());
                        }
                        state.Pop();
                        state.Push(StateObjectEnd);
                        i--;
                    }
                    i++;
                }
                else if (state.Peek() == StateObjectStart)
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        if (end < 0)
                        {
                            return list;
                        }
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            return list;
                        }
                        i += colon - i + 1;
                        state.Pop();
                        state.Push(StateObjectValue);
                    }
                    else
                    {
                        i++;
                    }
                }
                else if (state.Peek() == StateArrayStart)
                {
                    if (ch == '{')
                    {
                        state.Push(StateObjectStart);
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(StateArrayStart);
                        i++;
                    }
                    else if (ch == ']')
                    {
                        state.Pop();
                        i++;
                    }
                    else if (ch == '"')
                    {
                        i++;
                        var skip = true;
                        int end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                return list;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                return list;
                            }
                        }
                        i += end - i;
                        state.Pop();
                        state.Push(StateArrayEnd);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get array elements by a tag name - returns json
        /// </summary>
        public List<string> GetArrayElementsByName(string content, string name)
        {
            var list = new List<string>();
            int i = 0;
            int max = content.Length;
            var state = new Stack<StateElement>();
            var start = -1;
            var objectName = string.Empty;
            while (i < max)
            {
                var ch = content[i];
                if (state.Count == 0)
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement { State = StateObjectStart, Name = objectName });
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement { State = StateArrayStart, Name = objectName });
                    }
                    i++;
                }
                else if (state.Peek().State == StateObjectEnd)
                {
                    if (ch == ',')
                    {
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectStart, Name = objectName });
                    }
                    else if (ch == '}')
                    {
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Name == name && start >= 0)
                        {
                            list.Add(content.Substring(start, i - start + 1));
                            start = -1;
                        }
                    }
                    i++;
                }
                else if (state.Peek().State == StateArrayEnd)
                {
                    if (ch == ',')
                    {
                        if (objectName == name)
                        {
                            if (start >= 0)
                            {
                                list.Add(content.Substring(start, i - start - 1));
                            }
                            start = i + 1;
                        }
                        state.Pop();
                        state.Push(new StateElement { State = StateArrayStart, Name = objectName });
                    }
                    else if (ch == ']')
                    {
                        if (objectName == name)
                        {
                            if (start >= 0)
                            {
                                list.Add(content.Substring(start, i - start - 1));
                            }
                        }
                        state.Pop();
                    }
                    i++;
                }
                else if (state.Peek().State == StateObjectValue)
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement { State = StateObjectStart, Name = objectName });
                    }
                    else if (ch == ',')
                    {
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectStart, Name = objectName });
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement { State = StateArrayStart, Name = objectName });
                        if (objectName == name)
                        {
                            start = i + 1;
                        }
                    }
                    else if (ch == ']')
                    {
                        state.Pop();
                    }
                    else if (ch == '"')
                    {
                        i++;
                        var skip = true;
                        int end = 0;
                        while (skip)
                        {
                            end = content.IndexOf('"', i);
                            if (end < 0)
                            {
                                return list;
                            }
                            skip = content[i - 1] == '\\';
                            if (skip)
                            {
                                i++;
                            }
                            if (i >= max)
                            {
                                return list;
                            }
                        }
                        i += end - i;
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectEnd, Name = objectName });
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        i += 3;
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectEnd, Name = objectName });
                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 3;
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectEnd, Name = objectName });
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 3;
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectEnd, Name = objectName });
                    }
                    else if ("+-0123456789.Ee".Contains(ch))
                    {
                        while ("+-0123456789.Ee".Contains(content[i]) && i < max)
                        {
                            i++;
                        }
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectEnd, Name = objectName });
                        i--;
                    }
                    i++;
                }
                else if (state.Peek().State == StateObjectStart)
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        if (end < 0)
                        {
                            return list;
                        }
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            return list;
                        }
                        i += colon - i + 1;
                        state.Pop();
                        state.Push(new StateElement { State = StateObjectValue, Name = objectName });
                        if (objectName == name)
                        {
                            start = i;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
                else if (state.Peek().State == StateArrayStart)
                {
                    if (ch == '{')
                    {
                        if (state.Peek().Name == name)
                        {
                            start = i;
                        }
                        state.Push(new StateElement { State = StateObjectStart, Name = objectName });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement { State = StateObjectStart, Name = objectName });
                        i++;
                    }
                    else if (ch == ']')
                    {
                        if (objectName == name)
                        {
                            if (start >= 0)
                            {
                                list.Add(content.Substring(start, i - start - 1));
                            }
                        }
                        state.Pop();
                        i++;
                    }
                    else if (ch == '"')
                    {
                        i++;
                        var skip = true;
                        int end = 0;
                        while (skip)
                        {
                            end = content.IndexOf('"', i);
                            if (end < 0)
                            {
                                return list;
                            }
                            skip = content[i - 1] == '\\';
                            if (skip)
                            {
                                i++;
                            }
                            if (i >= max)
                            {
                                return list;
                            }
                        }
                        i += end - i;
                        state.Pop();
                        state.Push(new StateElement { State = StateArrayEnd, Name = objectName });
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            return list;
        }
    }
}
