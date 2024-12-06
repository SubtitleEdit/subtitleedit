using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class SeJsonParser
    {
        public enum SeJsonState
        {
            Object,
            Array,
            Value,
        }

        public List<string> Errors { get; private set; } = new List<string>();

        public class StateElement
        {
            public SeJsonState State { get; private set; }
            public string Name { get; private set; }
            public int Count { get; set; }

            public StateElement(string name, SeJsonState state)
            {
                Name = name;
                State = state;
                Count = 0;
            }
        }

        private readonly HashSet<char> _whiteSpace = new HashSet<char> { ' ', '\r', '\n', '\t' };

        public List<string> GetAllTagsByNameAsStrings(string content, string name)
        {
            Errors = new List<string>();
            var list = new List<string>();
            var i = 0;
            var max = content.Length;
            var state = new Stack<StateElement>();
            var objectName = string.Empty;
            while (i < max)
            {
                var ch = content[i];
                if (_whiteSpace.Contains(ch)) // ignore white space
                {
                    i++;
                }

                else if (state.Count == 0) // root
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Object) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        var end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        var colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement(objectName, SeJsonState.Value));
                    }
                    else if (ch == '}')
                    {
                        i++;
                        state.Pop();
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == ']') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Pop();
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Value) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
                    {
                        i++;
                        var skip = true;
                        var end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                Errors.Add($"Fatal - expected char \" after position {endSeek}");
                                return list;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                Errors.Add($"Fatal - expected end tag after position {endSeek}");
                                return list;
                            }
                        }
                        var objectValue = content.Substring(i, end - i);
                        if (objectName == name)
                        {
                            list.Add(objectValue);
                        }
                        i += end - i + 1;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '}') // empty value
                    {
                        i++;
                        var value = state.Pop();
                        if (state.Count > 0)
                        {
                            if (value.State == SeJsonState.Value)
                            {
                                state.Pop();
                            }
                            else
                            {
                                state.Peek().Count++;
                            }
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add(null);
                        }

                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add("true");
                        }
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 5;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add("false");
                        }
                    }
                    else if ("+-0123456789".IndexOf(ch) >= 0)
                    {
                        var sb = new StringBuilder();
                        while ("+-0123456789.Ee".IndexOf(content[i]) >= 0 && i < max)
                        {
                            sb.Append(content[i]);
                            i++;
                        }
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add(sb.ToString());
                        }
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Array) // array, after '['
                {
                    if (ch == ']')
                    {
                        state.Pop();
                        i++;
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                        i++;
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName + "_array_value", SeJsonState.Value));
                    }
                }

            }

            return list;
        }

        public List<string> GetArrayElementsByName(string content, string name)
        {
            Errors = new List<string>();
            var list = new List<string>();
            var i = 0;
            var max = content.Length;
            var state = new Stack<StateElement>();
            var objectName = string.Empty;
            var start = -1;
            while (i < max)
            {
                var ch = content[i];
                if (_whiteSpace.Contains(ch)) // ignore white space
                {
                    i++;
                }

                else if (state.Count == 0) // root
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Object) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        var end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        var colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement(objectName, SeJsonState.Value));
                    }
                    else if (ch == '}')
                    {
                        i++;
                        state.Pop();
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == ']') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Pop();
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Value) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
                    {
                        i++;
                        var skip = true;
                        var end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                Errors.Add($"Fatal - expected char \" after position {endSeek}");
                                return list;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                Errors.Add($"Fatal - expected end tag after position {endSeek}");
                                return list;
                            }
                        }
                        var objectValue = content.Substring(i, end - i).Trim();
                        if (objectName == name)
                        {
                            list.Add(objectValue);
                            start = -1;
                        }
                        i += end - i + 1;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '}') // empty value
                    {
                        i++;
                        var value = state.Pop();
                        if (state.Count > 0)
                        {
                            if (value.State == SeJsonState.Value)
                            {
                                state.Pop();
                            }
                            else
                            {
                                state.Peek().Count++;
                            }
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add(null);
                        }

                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add("true");
                        }
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 5;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add("false");
                        }
                    }
                    else if ("+-0123456789".IndexOf(ch) >= 0)
                    {
                        var sb = new StringBuilder();
                        while (i < max && "+-0123456789.Ee".IndexOf(content[i]) >= 0)
                        {
                            sb.Append(content[i]);
                            i++;
                        }
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        if (objectName == name)
                        {
                            list.Add(sb.ToString());
                        }
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Array));
                        i++;
                        if (start < 0 && objectName == name)
                        {
                            start = i;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Array) // array, after '['
                {
                    if (ch == ']')
                    {
                        state.Pop();
                        i++;
                        if (state.Count > 0 && state.Peek().Name == name && start > -1)
                        {
                            list.Add(content.Substring(start, i - start - 1).Trim());
                            start = -1;
                        }
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        if (start >= 0 && state.Peek().State == SeJsonState.Array && state.Peek().Name == name)
                        {
                            list.Add(content.Substring(start, i - start).Trim());
                            start = i + 1;
                        }
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                        i++;
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName + "_array_value", SeJsonState.Value));
                    }
                }
            }

            return list;
        }

        public List<string> GetArrayElements(string content)
        {
            Errors = new List<string>();
            var list = new List<string>();
            var i = 0;
            var max = content.Length;
            var state = new Stack<StateElement>();
            var objectName = string.Empty;
            var start = -1;
            while (i < max)
            {
                var ch = content[i];
                if (_whiteSpace.Contains(ch)) // ignore white space
                {
                    i++;
                }

                else if (state.Count == 0) // root
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Array));
                        i++;
                        start = i;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Object) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        var end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        var colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement(objectName, SeJsonState.Value));
                    }
                    else if (ch == '}')
                    {
                        i++;
                        state.Pop();
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == ']') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Pop();
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Value) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
                    {
                        i++;
                        var skip = true;
                        var end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                Errors.Add($"Fatal - expected char \" after position {endSeek}");
                                return list;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                Errors.Add($"Fatal - expected end tag after position {endSeek}");
                                return list;
                            }
                        }
                        i += end - i + 1;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '}') // empty value
                    {
                        i++;
                        var value = state.Pop();
                        if (state.Count > 0)
                        {
                            if (value.State == SeJsonState.Value)
                            {
                                state.Pop();
                            }
                            else
                            {
                                state.Peek().Count++;
                            }
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 5;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if ("+-0123456789".IndexOf(ch) >= 0)
                    {
                        var sb = new StringBuilder();
                        while ("+-0123456789.Ee".IndexOf(content[i]) >= 0 && i < max)
                        {
                            sb.Append(content[i]);
                            i++;
                        }
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Array) // array, after '['
                {
                    if (ch == ']')
                    {
                        if (state.Count > 0 && state.Peek().Name == "Root" && start > -1)
                        {
                            list.Add(content.Substring(start, i - start));
                            start = -1;
                        }
                        state.Pop();
                        i++;
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        if (start >= 0 && state.Count == 1 && state.Peek().State == SeJsonState.Array && state.Peek().Name == "Root")
                        {
                            list.Add(content.Substring(start, i - start));
                            start = i + 1;
                        }
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                        i++;
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName + "_array_value", SeJsonState.Value));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Get first object or value
        /// </summary>
        public string GetFirstObject(string content, string name)
        {
            Errors = new List<string>();
            var i = 0;
            var max = content.Length;
            var state = new Stack<StateElement>();
            var objectName = string.Empty;
            var start = -1;
            var startSateCount = -1;
            while (i < max)
            {
                var ch = content[i];
                if (_whiteSpace.Contains(ch)) // ignore white space
                {
                    i++;
                }

                else if (state.Count == 0) // root
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return string.Empty;
                    }
                }

                else if (state.Peek().State == SeJsonState.Object) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        var end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        var colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return string.Empty;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement(objectName, SeJsonState.Value));
                        if (objectName == name && start == -1)
                        {
                            start = i;
                            startSateCount = state.Count;
                        }
                    }
                    else if (ch == '}')
                    {
                        i++;
                        var s = state.Pop();
                        if (s.Name == name && state.Count == startSateCount && start >= 0)
                        {
                            return content.Substring(start, i - start);
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return string.Empty;
                        }
                    }
                    else if (ch == ']') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Pop();
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return string.Empty;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return string.Empty;
                    }
                }

                else if (state.Peek().State == SeJsonState.Value) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
                    {
                        i++;
                        var skip = true;
                        var end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                Errors.Add($"Fatal - expected char \" after position {endSeek}");
                                return string.Empty;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                Errors.Add($"Fatal - expected end tag after position {endSeek}");
                                return string.Empty;
                            }
                        }

                        var objectValue = content.Substring(i, end - i).Trim();
                        if (objectName == name)
                        {
                            return objectValue;
                        }

                        i += end - i + 1;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '}') // empty value
                    {
                        i++;
                        var value = state.Pop();
                        if (state.Count > 0)
                        {
                            if (value.State == SeJsonState.Value)
                            {
                                state.Pop();
                            }
                            else
                            {
                                state.Peek().Count++;
                            }
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return string.Empty;
                        }
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 5;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if ("+-0123456789".IndexOf(ch) >= 0)
                    {
                        var sb = new StringBuilder();
                        while ("+-0123456789.Ee".IndexOf(content[i]) >= 0 && i < max)
                        {
                            sb.Append(content[i]);
                            i++;
                        }

                        if (objectName == name)
                        {
                            return sb.ToString();
                        }

                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return string.Empty;
                    }
                }

                else if (state.Peek().State == SeJsonState.Array) // array, after '['
                {
                    if (ch == ']')
                    {
                        state.Pop();
                        i++;
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return string.Empty;
                        }
                        i++;
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName + "_array_value", SeJsonState.Value));
                    }
                }

            }
            return string.Empty;
        }

        public class RootElement
        {
            public string Name { get; private set; }
            public string Json { get; private set; }

            public RootElement(string name, string json)
            {
                Name = name;
                Json = json;
            }
        }

        public List<RootElement> GetRootElements(string content)
        {
            Errors = new List<string>();
            var list = new List<RootElement>();
            var i = 0;
            var max = content.Length;
            var state = new Stack<StateElement>();
            var objectName = string.Empty;
            var start = -1;
            while (i < max)
            {
                var ch = content[i];
                if (_whiteSpace.Contains(ch)) // ignore white space
                {
                    i++;
                }

                else if (state.Count == 0) // root
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement("Root", SeJsonState.Array));
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Object) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        var end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        var colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement(objectName, SeJsonState.Value));

                        if (state.Count == 2)
                        {
                            start = i; // element in root
                        }
                    }
                    else if (ch == '}')
                    {
                        i++;
                        state.Pop();

                        if (state.Count == 2 && start >= 0)
                        {
                            var str = content.Substring(start, i - start).Trim();
                            list.Add(new RootElement(state.Peek().Name, str));
                            start = -1;
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        if (state.Count == 1 && start >= 0)
                        {
                            var str = content.Substring(start, i - start).Trim();
                            list.Add(new RootElement(state.Peek().Name, str));
                            start = i + 1;
                        }

                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == ']') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Pop();
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Value) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
                    {
                        i++;
                        var skip = true;
                        var end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                Errors.Add($"Fatal - expected char \" after position {endSeek}");
                                return list;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                Errors.Add($"Fatal - expected end tag after position {endSeek}");
                                return list;
                            }
                        }

                        if (state.Count == 2)
                        {
                            var objectValue = content.Substring(i, end - i).Trim();
                            var x = state.Peek();
                            list.Add(new RootElement(x.Name, objectValue));
                            start = -1;
                        }

                        i += end - i + 1;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '}') // empty value
                    {
                        i++;
                        var value = state.Pop();
                        if (state.Count > 0)
                        {
                            if (value.State == SeJsonState.Value)
                            {
                                var st = state.Pop();

                                if (state.Count == 2)
                                {
                                    var str = content.Substring(start, i - start).Trim();
                                    list.Add(new RootElement(state.Peek().Name, str));
                                    start = -1;
                                }
                            }
                            else
                            {
                                state.Peek().Count++;
                            }
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        //if (objectName == name)
                        //{
                        //    list.Add(null);
                        //}

                    }
                    else if (ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        //if (objectName == name)
                        //{
                        //    list.Add("true");
                        //}
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 5;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        //if (objectName == name)
                        //{
                        //    list.Add("false");
                        //}
                    }
                    else if ("+-0123456789".IndexOf(ch) >= 0)
                    {
                        var sb = new StringBuilder();
                        while (i < max && "+-0123456789.Ee".IndexOf(content[i]) >= 0)
                        {
                            sb.Append(content[i]);
                            i++;
                        }
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        //if (objectName == name)
                        //{
                        //    list.Add(sb.ToString());
                        //}
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else if (ch == '[')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Array));
                        i++;
                        //if (start < 0 && objectName == name)
                        //{
                        //    start = i;
                        //}
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == SeJsonState.Array) // array, after '['
                {
                    if (ch == ']')
                    {
                        state.Pop();
                        i++;
                        //if (state.Count > 0 && state.Peek().Name == name && start > -1)
                        //{
                        //    list.Add(content.Substring(start, i - start - 1).Trim());
                        //    start = -1;
                        //}
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        //if (start >= 0 && state.Peek().State == SeJsonState.Array && state.Peek().Name == name)
                        //{
                        //    list.Add(content.Substring(start, i - start).Trim());
                        //    start = i + 1;
                        //}
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} at position {i}");
                            return list;
                        }
                        i++;
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName, SeJsonState.Object));
                        i++;
                    }
                    else
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement(objectName + "_array_value", SeJsonState.Value));
                    }
                }
            }

            return list;
        }
    }
}
