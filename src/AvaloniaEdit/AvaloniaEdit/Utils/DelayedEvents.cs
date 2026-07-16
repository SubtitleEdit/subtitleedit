// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// Maintains a list of delayed events to raise.
    /// </summary>
    internal sealed class DelayedEvents
    {
        private readonly struct EventCall
        {
            private readonly EventHandler _handler;
            private readonly object _sender;
            private readonly EventArgs _e;

            public EventCall(EventHandler handler, object sender, EventArgs e)
            {
                _handler = handler;
                _sender = sender;
                _e = e;
            }

            public void Call()
            {
                _handler(_sender, _e);
            }
        }

        private readonly Queue<EventCall> _eventCalls = new Queue<EventCall>();

        public void DelayedRaise(EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
            {
                _eventCalls.Enqueue(new EventCall(handler, sender, e));
            }
        }

        public void RaiseEvents()
        {
            while (_eventCalls.Count > 0)
                _eventCalls.Dequeue().Call();
        }
    }
}
