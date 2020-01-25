using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class SeJsonParser
    {
        public List<string> Errors { get; private set; } = new List<string>();

        public class StateElement
        {
            public int State { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }

        private const int JStateObject = 0;
        private const int JStateArray = 1;
        private const int JStateValue = 2;
        private readonly HashSet<char> _whiteSpace = new HashSet<char> { ' ', '\r', '\n', '\t' };

        public List<string> GetAllTagsByNameAsStrings(string content, string name)
        {
            Errors = new List<string>();
            var list = new List<string>();
            int i = 0;
            int max = content.Length;
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
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateObject
                        });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateArray
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == JStateObject) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateValue
                        });
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

                else if (state.Peek().State == JStateValue) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
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
                            if (value.State == JStateValue)
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
                        state.Push(new StateElement
                        {
                            State = JStateObject,
                            Name = objectName
                        });
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
                        state.Push(new StateElement
                        {
                            State = JStateArray,
                            Name = objectName
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == JStateArray) // array, after '['
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
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateObject
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

            }
            return list;
        }

        public List<string> GetArrayElementsByName(string content, string name)
        {
            Errors = new List<string>();
            var list = new List<string>();
            int i = 0;
            int max = content.Length;
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
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateObject
                        });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateArray
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == JStateObject) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateValue
                        });
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

                else if (state.Peek().State == JStateValue) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
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
                            if (value.State == JStateValue)
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
                        state.Push(new StateElement
                        {
                            State = JStateObject,
                            Name = objectName
                        });
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
                        state.Push(new StateElement
                        {
                            State = JStateArray,
                            Name = objectName
                        });
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

                else if (state.Peek().State == JStateArray) // array, after '['
                {
                    if (ch == ']')
                    {
                        state.Pop();
                        i++;
                        if (state.Count > 0 && state.Peek().Name == name && start > -1)
                        {
                            list.Add(content.Substring(start, i - start - 1));
                            start = -1;
                        }
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        if (start >= 0 && state.Peek().State == JStateArray && state.Peek().Name == name)
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
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateObject
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

            }
            return list;
        }

        public List<string> GetArrayElements(string content)
        {
            Errors = new List<string>();
            var list = new List<string>();
            int i = 0;
            int max = content.Length;
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
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateObject
                        });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateArray
                        });
                        i++;
                        start = i;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == JStateObject) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return list;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateValue
                        });
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

                else if (state.Peek().State == JStateValue) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
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
                            if (value.State == JStateValue)
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
                        state.Push(new StateElement
                        {
                            State = JStateObject,
                            Name = objectName
                        });
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
                        state.Push(new StateElement
                        {
                            State = JStateArray,
                            Name = objectName
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

                else if (state.Peek().State == JStateArray) // array, after '['
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
                        if (start >= 0 && state.Count == 1 && state.Peek().State == JStateArray && state.Peek().Name == "Root")
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
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateObject
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return list;
                    }
                }

            }
            return list;
        }

        public string GetFirstObject(string content, string name)
        {
            Errors = new List<string>();
            int i = 0;
            int max = content.Length;
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
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateObject
                        });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = JStateArray
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return string.Empty;
                    }
                }

                else if (state.Peek().State == JStateObject) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : after position {end}");
                            return string.Empty;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateValue
                        });
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

                else if (state.Peek().State == JStateValue) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
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
                            if (value.State == JStateValue)
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
                        state.Push(new StateElement
                        {
                            State = JStateObject,
                            Name = objectName
                        });
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
                        state.Push(new StateElement
                        {
                            State = JStateArray,
                            Name = objectName
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return string.Empty;
                    }
                }

                else if (state.Peek().State == JStateArray) // array, after '['
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
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = JStateObject
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} at position {i}");
                        return string.Empty;
                    }
                }

            }
            return string.Empty;
        }
    }
}
