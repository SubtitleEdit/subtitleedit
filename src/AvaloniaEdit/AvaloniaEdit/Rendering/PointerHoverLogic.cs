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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// Encapsulates and adds pointer hover support to controls.
    /// </summary>
    public class PointerHoverLogic : IDisposable
    {
        private const double PointerHoverWidth = 2;
        private const double PointerHoverHeight = 2;
        private static readonly TimeSpan PointerHoverTime = TimeSpan.FromMilliseconds(400);

        private readonly Control _target;

        private DispatcherTimer _timer;
        private Point _hoverStartPoint;
        private PointerEventArgs _hoverLastEventArgs;
        private bool _hovering;

        /// <summary>
        /// Creates a new instance and attaches itself to the <paramref name="target" /> UIElement.
        /// </summary>
        public PointerHoverLogic(Control target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _target.PointerExited += OnPointerLeave;
            _target.PointerMoved += OnPointerMoved;
            _target.PointerEntered += OnPointerEnter;
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            var movement = _hoverStartPoint - e.GetPosition(_target);
            if (Math.Abs(movement.X) > PointerHoverWidth ||
                Math.Abs(movement.Y) > PointerHoverHeight)
            {
                StartHovering(e);
            }
            // do not set e.Handled - allow others to also handle the event
        }

        private void OnPointerEnter(object sender, PointerEventArgs e)
        {
            StartHovering(e);
            // do not set e.Handled - allow others to also handle the event
        }

        private void StartHovering(PointerEventArgs e)
        {
            StopHovering();
            _hoverStartPoint = e.GetPosition(_target);
            _hoverLastEventArgs = e;
            _timer = new DispatcherTimer(PointerHoverTime, DispatcherPriority.Background, OnTimerElapsed);
            _timer.Start();
        }

        private void OnPointerLeave(object sender, PointerEventArgs e)
        {
            StopHovering();
            // do not set e.Handled - allow others to also handle the event
        }

        private void StopHovering()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            if (_hovering)
            {
                _hovering = false;
                OnPointerHoverStopped(_hoverLastEventArgs);
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer = null;

            _hovering = true;
            OnPointerHover(_hoverLastEventArgs);
        }

        /// <summary>
        /// Occurs when the pointer starts hovering over a certain location.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerHover;

        /// <summary>
        /// Raises the <see cref="PointerHover"/> event.
        /// </summary>
        protected virtual void OnPointerHover(PointerEventArgs e)
        {
            PointerHover?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when the pointer stops hovering over a certain location.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerHoverStopped;

        /// <summary>
        /// Raises the <see cref="PointerHoverStopped"/> event.
        /// </summary>
        protected virtual void OnPointerHoverStopped(PointerEventArgs e)
        {
            PointerHoverStopped?.Invoke(this, e);
        }

        private bool _disposed;

        /// <summary>
        /// Removes the hover support from the target control.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _target.PointerExited -= OnPointerLeave;
                _target.PointerMoved -= OnPointerMoved;
                _target.PointerEntered -= OnPointerEnter;
            }
            _disposed = true;
        }
    }
}
